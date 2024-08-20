using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class ReportController : ControllerBase
    {
        private readonly AuthLayerService _authLayerService;
        private readonly CompanyService _companyService;
        private readonly GroupService _groupService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly StockInService _stockInService;
        private readonly PurchaseService _purchaseService;
        private readonly StockOutService _stockOutService;
        private readonly ReportService _reportService;
        private readonly IValidator<GetProductInAndOutStockRecordsRequest> _getProductInAndOutStockRecordsValidator;

        public ReportController(AuthLayerService authLayerService, CompanyService companyService, GroupService groupService,
            WarehouseProductService warehouseProductService, IMapper mapper, ILogger<ReportController> logger, AuthHelpers authHelpers,
            StockInService stockInService, PurchaseService purchaseService, StockOutService stockOutService,ReportService reportService)
        {
            _authLayerService = authLayerService;
            _companyService = companyService;
            _groupService = groupService;
            _warehouseProductService = warehouseProductService;
            _mapper = mapper;
            _logger = logger;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _purchaseService = purchaseService;
            _stockOutService = stockOutService;
            _reportService = reportService;
            _getProductInAndOutStockRecordsValidator = new GetProductInAndOutStockRecordsValidator(groupService);
        }

        [HttpPost("getProductInAndOutStockRecords")]
        [Authorize]
        public IActionResult GetProductInAndOutStockRecords(GetProductInAndOutStockRecordsRequest getProductInAndOutStockRecordsRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            getProductInAndOutStockRecordsRequest.CompId = compId;

            var group = _groupService.GetGroupByGroupId(getProductInAndOutStockRecordsRequest.GroupId);

            var products = _warehouseProductService.GetProductsByGroupId(getProductInAndOutStockRecordsRequest.GroupId);

            var validationResult = _getProductInAndOutStockRecordsValidator.Validate(getProductInAndOutStockRecordsRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var data = _reportService.GetProductInAndOutRecords(group, products, getProductInAndOutStockRecordsRequest);

            return Ok(new CommonResponse<List<ProductInAndOutRecord>>
            {
                Result = true,
                Data = data
            });
        }



    }
}
