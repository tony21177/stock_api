using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common.Constant;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Service;
using stock_api.Utils;
using stock_api.Models;
using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Controllers.Validator;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseProductController : ControllerBase
    {

        private readonly AuthLayerService _authLayerService;
        private readonly CompanyService _companyService;
        private readonly GroupService _groupService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthlayerController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly IValidator<WarehouseProductSearchRequest> _searchProductRequestValidator;

        public WarehouseProductController(AuthLayerService authLayerService, WarehouseProductService warehouseProductService,CompanyService companyService, GroupService groupService,IMapper mapper, ILogger<AuthlayerController> logger, AuthHelpers authHelpers)
        {
            _authLayerService = authLayerService;
            _warehouseProductService = warehouseProductService;
            _groupService = groupService;
            _mapper = mapper;
            _logger = logger;
            _authHelpers = authHelpers;
            _companyService = companyService;
            _searchProductRequestValidator = new SearchProductRequestValidator(companyService, groupService);
        }

        [HttpPost("search")]
        [Authorize]
        public IActionResult SearchWarehouseProduct(WarehouseProductSearchRequest searchRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var compType = memberAndPermissionSetting.CompanyWithUnit.Type;

            if (searchRequest.CompId == null)
            {
                searchRequest.CompId = compId;
            }

            if(searchRequest.CompId!=null&& searchRequest.CompId != compId && compType != CommonConstants.CompanyType.Owner)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _searchProductRequestValidator.Validate(searchRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var (data,totalPages) = _warehouseProductService.SearchProduct(searchRequest);

            var response = new CommonResponse<List<WarehouseProduct>>()
            {
                Result = true,
                Message = "",
                Data = data,
                TotalPages = totalPages
            };
            return Ok(response);

        }
    }
}
