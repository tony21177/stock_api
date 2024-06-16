using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Service;
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
        private readonly AdjustService _adjustmentService;

        public AdjustController(IMapper mapper, AuthHelpers authHelpers, StockInService stockInService, StockOutService stockOutService, WarehouseProductService warehouseProductService,AdjustService adjustService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _stockOutService = stockOutService;
            _warehouseProductService = warehouseProductService;
            _adjustmentService = adjustService;
        }

        [HttpPost("generalAdjust")]
        [AuthorizeRoles("1","3")]
        public ActionResult Adjust(AdjustRequest adjustRequest) 
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            adjustRequest.CompId = compId;
            
            var productIds = adjustRequest.AdjustItems.Select(x => x.ProductId).ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIds,compId);

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

            var (result,errorMsg) = _adjustmentService.AdjustItems(adjustRequest.AdjustItems, products, memberAndPermissionSetting.Member);
            return Ok(new CommonResponse<dynamic>(){
                Result = result,
                Message = errorMsg
            });
        }
    }
}
