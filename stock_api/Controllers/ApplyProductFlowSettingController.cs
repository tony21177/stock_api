using AutoMapper;
using FluentValidation;
using MaiBackend.PublicApi.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Common.Utils;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplyProductFlowSettingController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly ApplyProductFlowSettingService _applyProductFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly GroupService _groupService;
        private readonly IValidator<CreateApplyProductFlowSettingRequest> _createApplyProductFlowSettingValidator;
        private readonly IValidator<ApplyProductFlowSettingRequest> _updateApplyProductFlowSettingValidator;

        public ApplyProductFlowSettingController(IMapper mapper, AuthHelpers authHelpers, ApplyProductFlowSettingService applyProductFlowSettingService, MemberService memberService, CompanyService companyService,GroupService groupService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _applyProductFlowSettingService = applyProductFlowSettingService;
            _companyService = companyService;
            _memberService = memberService;
            _groupService = groupService;
            _createApplyProductFlowSettingValidator = new CreateApplyProductFlowSettingValidator(applyProductFlowSettingService, memberService,groupService);
            _updateApplyProductFlowSettingValidator = new ApplyProductFlowSettingValidator(ActionTypeEnum.Update,applyProductFlowSettingService,memberService,groupService);
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreatePurchaseFlowSetting(CreateApplyProductFlowSettingRequest createRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (createRequest.CompId != null && createRequest.CompId != compId)
            {
                if (AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
                {
                    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
                }
            }
            if (createRequest.CompId == null)
            {
                createRequest.CompId = compId;
                
                var t = 123;
            }
            createRequest.CreateApplyProductFlowSettingList.ForEach(r => r.CompId = createRequest.CompId);


            var validationResult = _createApplyProductFlowSettingValidator.Validate(createRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            _applyProductFlowSettingService.AddApplyProductFlowSetting(createRequest.CreateApplyProductFlowSettingList,createRequest.CompId);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult UpdateApplyProductFlowSetting(ApplyProductFlowSettingRequest updateRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var settingId = updateRequest.SettingId;
            var existingApplyProductFlowSetting = _applyProductFlowSettingService.GetApplyProductFlowSettingBySettingId(settingId);
            if (existingApplyProductFlowSetting == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此審核流程不存在"
                });
            }
            if (compId != existingApplyProductFlowSetting.CompId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting) == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }

            updateRequest.CompId = existingApplyProductFlowSetting.CompId;
            var validationResult = _updateApplyProductFlowSettingValidator.Validate(updateRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            if (updateRequest.ReviewGroupId != null || updateRequest.Sequence != null)
            {
                bool seqExisted = false;
                if (updateRequest.ReviewGroupId != null && updateRequest.Sequence != null)
                {
                    seqExisted = _applyProductFlowSettingService.IsSequenceExistForGroupIdAndExcludeSettingId(updateRequest.SettingId,updateRequest.Sequence.Value, updateRequest.ReviewGroupId, compId);
                }
                if(updateRequest.ReviewGroupId != null && updateRequest.Sequence == null)
                {
                    seqExisted = _applyProductFlowSettingService.IsSequenceExistForGroupIdAndExcludeSettingId(updateRequest.SettingId, existingApplyProductFlowSetting.Sequence, updateRequest.ReviewGroupId, compId);
                }
                if (updateRequest.ReviewGroupId == null && updateRequest.Sequence != null)
                {
                    seqExisted = _applyProductFlowSettingService.IsSequenceExistForGroupIdAndExcludeSettingId(updateRequest.SettingId, updateRequest.Sequence.Value, existingApplyProductFlowSetting.ReviewGroupId, compId);
                }
                if (seqExisted)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "此群組審核流程已存在相同sequence"
                    });
                }


            }



            
            _applyProductFlowSettingService.UpdateApplyProductFlowSetting(updateRequest, existingApplyProductFlowSetting);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListApplyProductFlowSettings(string? compId = null)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var companyId = compId ?? memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (compId != null && compId != memberAndPermissionSetting.CompanyWithUnit.CompId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }
            var data = _applyProductFlowSettingService.GetAllApplyProductFlowSettingsByCompId(companyId).OrderBy(pfs => pfs.Sequence).ToList();
            var response = new CommonResponse<List<ApplyProductFlowSettingVo>>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("get/{settingId}")]
        [Authorize]
        public IActionResult GetApplyProductFlowSettingDetail(string settingId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var data = _applyProductFlowSettingService.GetApplyProductFlowSettingBySettingId(settingId);
            if(data!=null&&data.CompId!=compId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }


            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }

        [HttpDelete("delete/{settingId}")]
        [Authorize]
        public IActionResult InActiveApplyProductFlowSetting(string settingId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var existApplyProductFlowSetting = _applyProductFlowSettingService.GetApplyProductFlowSettingBySettingId(settingId);
            if (existApplyProductFlowSetting == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此審核流程不存在"
                });
            }
            if (existApplyProductFlowSetting != null && existApplyProductFlowSetting.CompId != compId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }
            _applyProductFlowSettingService.DeleteApplyProductFlowSetting(existApplyProductFlowSetting.SettingId);

            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }
    }
}
