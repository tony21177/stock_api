using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdjustController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly StockInService _stockInService;
        private readonly StockOutService _stockOutService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly SupplierService _supplierService;
        private readonly AdjustService _adjustmentService;
        private readonly IValidator<ListAdjustItemsRequest> _listAdjustItemsRequestValidator;

        public AdjustController(IMapper mapper, AuthHelpers authHelpers, StockInService stockInService, StockOutService stockOutService, WarehouseProductService warehouseProductService, AdjustService adjustService,SupplierService supplierService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _stockOutService = stockOutService;
            _warehouseProductService = warehouseProductService;
            _supplierService = supplierService;
            _adjustmentService = adjustService;
            _listAdjustItemsRequestValidator = new ListAdjustItemsValidator(supplierService);
        }

        [HttpPost("generalAdjust")]
        [AuthorizeRoles("1", "3")]
        public ActionResult Adjust(AdjustRequest adjustRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            adjustRequest.CompId = compId;

            var productIds = adjustRequest.AdjustItems.Select(x => x.ProductId).ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIds, compId);
            var lotNumberBatchList = adjustRequest.AdjustItems.Select(i=>i.LotNumberBatch).Where(batch=>batch!=null).ToList();
            if (lotNumberBatchList.Count > 0)
            {
                var duplicateBatchList = _stockInService.GetDuplicateBatchList(lotNumberBatchList);
                if (duplicateBatchList.Count > 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"批次:{string.Join(",", duplicateBatchList)}已重複"
                    });
                }
            }


            foreach (var item in adjustRequest.AdjustItems)
            {
                var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                if (matchedProduct == null)
                {
                    return BadRequest(new CommonResponse<dynamic>()
                    {
                        Result = false,
                        Message = $"品項:{item.ProductId}不存在",
                    });
                }
                if (matchedProduct.InStockQuantity != item.BeforeQuantity)
                {
                    return BadRequest(new CommonResponse<dynamic>()
                    {
                        Result = false,
                        Message = $"品項:{item.ProductId}不存在",
                    });
                }
            }

            var (result, errorMsg) = _adjustmentService.AdjustItems(adjustRequest.AdjustItems, products, memberAndPermissionSetting.Member);
            return Ok(new CommonResponse<dynamic>()
            {
                Result = result,
                Message = errorMsg
            });
        }

        [HttpPost("list")]
        [Authorize]
        public ActionResult ListAdjustItemView(ListAdjustItemsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            request.CompId = compId;
            var validationResult = _listAdjustItemsRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var (data,totalPages) = _adjustmentService.ListAdjustMainWithItemsByCondition(request);
            var allProductIdList = data.SelectMany(main=>main.Items.Select(item=>item.ProductId)).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(allProductIdList, compId);

            foreach (var main in data)
            {
                foreach (var item in main.Items)
                {
                    var matchedProduct = products.Where(p=>p.ProductId==item.ProductId).FirstOrDefault();
                    if (matchedProduct != null)
                    {
                        item.ProductName = matchedProduct.ProductName;
                        item.GroupName = matchedProduct.GroupNames;
                        item.ProductSpec = matchedProduct.ProductSpec;
                        item.ProductUnit = matchedProduct.Unit;
                        item.DefaultSupplierName = matchedProduct.DefaultSupplierName;
                    }
                }
                main.Items = main.Items.OrderBy(i => i.ProductCode).ToList();
            }


            return Ok(new CommonResponse<List<AdjustMainWithItemsVo>>
            {
                Result = true,
                Data = data,
                TotalPages = totalPages
            });
        }
    }
}
