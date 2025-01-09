using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RejectItemController : ControllerBase
    {
        private readonly AuthLayerService _authLayerService;
        private readonly CompanyService _companyService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly IMapper _mapper;
        private readonly ILogger<RejectItemController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly StockInService _stockInService;
        private readonly PurchaseService _purchaseService;
        private readonly RejectItemService _rejectItemService;
        private readonly IValidator<RejectItemRequest> _rejectItemValidator;
        private readonly IValidator<ListRejectRecordsRequest> _listRejectRecordsValidator;

        public RejectItemController(AuthLayerService authLayerService, CompanyService companyService,
            WarehouseProductService warehouseProductService,
            IMapper mapper, ILogger<RejectItemController> logger,
            AuthHelpers authHelpers, StockInService stockInService, PurchaseService purchaseService, RejectItemService rejectionService)
        {
            _authLayerService = authLayerService;
            _companyService = companyService;
            _warehouseProductService = warehouseProductService;
            _mapper = mapper;
            _logger = logger;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _purchaseService = purchaseService;
            _rejectItemValidator = new RejectItemRequestValidator();
            _rejectItemService = rejectionService;
            _listRejectRecordsValidator = new ListRejectRecordsRequestValidator();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Reject(RejectItemRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var inStockItemRecord = _stockInService.GetInStockRecordById(request.InStockId);
            if (inStockItemRecord == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "未找到該筆入庫紀錄"
                });
            }
            if (compId != inStockItemRecord.CompId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var product = _warehouseProductService.GetProductByProductId(inStockItemRecord.ProductId);
            if (product == null )
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "未找到該品項"  
                });
            }

            PurchaseSubItem? subItem = null;
            if (inStockItemRecord.ItemId != null)
            {
                subItem = _purchaseService.GetPurchaseSubItemByItemId(inStockItemRecord.ItemId);
            }


            var (result, msg) = _rejectItemService.RejectItem(inStockItemRecord, subItem, product, memberAndPermissionSetting.Member,request);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = msg
            });

        }

        [HttpPost("search")]
        [Authorize]
        public IActionResult ListRejectItemRecords(ListRejectRecordsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }

            var validationResult = _listRejectRecordsValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var (data, totalPages) = _rejectItemService.ListRejectItemRecords(request);
            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = data,
                TotalPages = totalPages
            });
        }
    }
}
