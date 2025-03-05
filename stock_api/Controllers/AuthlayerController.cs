using AutoMapper;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service;
using stock_api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using stock_api.Controllers.Validator;
using stock_api.Common.Constant;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthlayerController : Controller
    {
        private readonly AuthLayerService _authLayerService;
        private readonly CompanyService _companyService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthlayerController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly IValidator<ResetAllAuthRequest> _resetAllAuthRequestValidator;
        public AuthlayerController(AuthLayerService authLayerService,CompanyService companyService, IMapper mapper, ILogger<AuthlayerController> logger, AuthHelpers authHelper)
        {
            _authLayerService = authLayerService;
            _companyService = companyService;
            _mapper = mapper;
            _logger = logger;
            _authHelpers = authHelper;
            _resetAllAuthRequestValidator = new ResetAllAuthValidator(companyService);
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult List(string? compId = null)
        {
            // Gary 加入 AuthLayer可以跨公司
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var companyId = compId ?? memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (compId!=null&&compId!= memberAndPermissionSetting.CompanyWithUnit.CompId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }

            var data = _authLayerService.GetAllAuthlayers(companyId);
            var response = new CommonResponse<List<WarehouseAuthlayer>>()
            {
                Result = true,
                Message = "version 1",
                Data = data
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [AuthorizeRoles("1")]
        public CommonResponse<List<WarehouseAuthlayer>> Update(List<UpdateAuthlayerRequest> updateAuthlayerListRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var data = _authLayerService.UpdateAuthlayers(updateAuthlayerListRequest);

            var response = new CommonResponse<List<WarehouseAuthlayer>>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return response;
        }

        [HttpPost("create")]
        [AuthorizeRoles("1")]
        public CommonResponse<WarehouseAuthlayer> Create(CreateAuthlayerRequest createAuthlayerRequest)
        {
            var newAuthLayer = _mapper.Map<WarehouseAuthlayer>(createAuthlayerRequest);
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            newAuthLayer.CompId = compId;
            var data = _authLayerService.AddAuthlayer(newAuthLayer);

            var response = new CommonResponse<WarehouseAuthlayer>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return response;
        }

        [HttpDelete("delete/{id}")]
        [AuthorizeRoles("1")]
        public IActionResult Delete(int id)
        {

            _authLayerService.DeleteAuthLayer(id);

            var response = new CommonResponse<WarehouseAuthlayer>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("owner/resetAll")]
        [Authorize]
        public IActionResult ResetAll(ResetAllAuthRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var validationResult = _resetAllAuthRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var result = _authLayerService.ResetAllAuthLayer(request.CompId);

            var response = new CommonResponse<WarehouseAuthlayer>()
            {
                Result = result,
                Message = "",
                Data = null
            };
            return Ok(response);
        }


    }
}

