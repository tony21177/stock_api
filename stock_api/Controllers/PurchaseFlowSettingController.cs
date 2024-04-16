using AutoMapper;
using FluentValidation;
using MaiBackend.PublicApi.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseFlowSettingController: ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly PurchaseFlowSettingService _purchaseFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly IValidator<CreateOrUpdatePurchaseFlowSettingRequest> _createPurchaseFlowSettingValidator;
        private readonly IValidator<CreateOrUpdatePurchaseFlowSettingRequest> _updatePurchaseFlowSettingValidator;

        public PurchaseFlowSettingController(IMapper mapper, AuthHelpers authHelpers, PurchaseFlowSettingService purchaseFlowSettingService,MemberService memberService, CompanyService companyService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _purchaseFlowSettingService = purchaseFlowSettingService;
            _companyService = companyService;
            _memberService = memberService;
            _createPurchaseFlowSettingValidator = new CreateOrUpdatePurchaseFlowSettingValidator(ActionTypeEnum.Create,purchaseFlowSettingService, memberService,companyService);
            _updatePurchaseFlowSettingValidator = new CreateOrUpdatePurchaseFlowSettingValidator(ActionTypeEnum.Update, purchaseFlowSettingService, memberService, companyService);
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreatePurchaseFlowSetting(CreateOrUpdatePurchaseFlowSettingRequest createRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            createRequest.CompId = compId;

            var validationResult = _createPurchaseFlowSettingValidator.Validate(createRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var newPurchaseFlowSetting = _mapper.Map<PurchaseFlowSetting>(createRequest);
            _purchaseFlowSettingService.AddPurchaseFlowSetting(newPurchaseFlowSetting);
            var response = new CommonResponse<MemberDto>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult UpdatePurchaseFlowSetting(CreateOrUpdatePurchaseFlowSettingRequest updateRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var flowId = updateRequest.FlowId;
            // TODO
            var existingPurchaseFlowSetting =_purchaseFlowSettingService.GetPurchaseFlowSettingByFlowId(flowId);
            if (existingPurchaseFlowSetting==null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此審核流程不存在"
                });
            }
            if (compId != existingPurchaseFlowSetting.CompId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            updateRequest.CompId = compId;
            var validationResult = _updatePurchaseFlowSettingValidator.Validate(updateRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            
            _purchaseFlowSettingService.UpdatePurchaseFlowSetting(updateRequest, existingPurchaseFlowSetting);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListPurchaseFlowSettings()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var data = _purchaseFlowSettingService.GetAllPurchaseFlowSettingsByCompId(compId).OrderBy(pfs => pfs.Sequence);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("get/{flowId}")]
        [Authorize]
        public IActionResult GetPurchaseFlowSettingDetail(string flowId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var data = _purchaseFlowSettingService.GetPurchaseFlowSettingVoByFlowId(flowId);
            if (data != null&&data.CompId!=compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }


            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }

        [HttpDelete("delete/{flowId}")]
        [Authorize]
        public IActionResult InActivePurchaseFlowSetting(string flowId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var existPurchaseFlowSetting = _purchaseFlowSettingService.GetPurchaseFlowSettingByFlowId(flowId);
            if (existPurchaseFlowSetting != null && existPurchaseFlowSetting.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            _purchaseFlowSettingService.InactivePurchaseFlowSetting(existPurchaseFlowSetting.FlowId,false);

            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }
    }
}
