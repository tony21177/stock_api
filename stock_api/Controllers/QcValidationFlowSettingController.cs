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
    public class QcValidationFlowSettingController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly QcValidationFlowSettingService _qcValidationFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly GroupService _groupService;
        private readonly IValidator<CreateQcValidationFlowSettingRequest> _createQcValidationFlowSettingValidator;
        private readonly IValidator<QcValidationFlowSettingRequest> _updateQcValidationFlowSettingValidator;

        public QcValidationFlowSettingController(IMapper mapper, AuthHelpers authHelpers, QcValidationFlowSettingService qcValidationFlowSettingService, MemberService memberService, CompanyService companyService,GroupService groupService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _qcValidationFlowSettingService = qcValidationFlowSettingService;
            _companyService = companyService;
            _memberService = memberService;
            _groupService = groupService;
            _createQcValidationFlowSettingValidator = new CreateQcValidationFlowSettingValidator(qcValidationFlowSettingService, memberService,groupService);
            _updateQcValidationFlowSettingValidator = new QcValidationFlowSettingValidator(ActionTypeEnum.Update, qcValidationFlowSettingService, memberService,groupService);
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateQcValidationFlowSetting(CreateQcValidationFlowSettingRequest createRequest)
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
            createRequest.CreateQcValidationFlowSettingList.ForEach(r => r.CompId = createRequest.CompId);


            var validationResult = _createQcValidationFlowSettingValidator.Validate(createRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            _qcValidationFlowSettingService.AddQcValidationFlowSetting(createRequest.CreateQcValidationFlowSettingList, createRequest.CompId);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult UpdateQcValidationFlowSetting(QcValidationFlowSettingRequest updateRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var settingId = updateRequest.SettingId;
            var existingQcValidationFlowSetting = _qcValidationFlowSettingService.GetQcValidationFlowSettingBySettingId(settingId);
            if (existingQcValidationFlowSetting == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此審核流程不存在"
                });
            }
            if (compId != existingQcValidationFlowSetting.CompId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting) == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }

            updateRequest.CompId = existingQcValidationFlowSetting.CompId;
            var validationResult = _updateQcValidationFlowSettingValidator.Validate(updateRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            if (updateRequest.ReviewGroupId != null || updateRequest.Sequence != null)
            {
                bool seqExisted = false;
                if (updateRequest.ReviewGroupId != null && updateRequest.Sequence != null)
                {
                    seqExisted = _qcValidationFlowSettingService.IsSequenceExistForGroupIdAndExcludeSettingId(updateRequest.SettingId,updateRequest.Sequence.Value, updateRequest.ReviewGroupId, compId);
                }
                if(updateRequest.ReviewGroupId != null && updateRequest.Sequence == null)
                {
                    seqExisted = _qcValidationFlowSettingService.IsSequenceExistForGroupIdAndExcludeSettingId(updateRequest.SettingId, existingQcValidationFlowSetting.Sequence, updateRequest.ReviewGroupId, compId);
                }
                if (updateRequest.ReviewGroupId == null && updateRequest.Sequence != null)
                {
                    seqExisted = _qcValidationFlowSettingService.IsSequenceExistForGroupIdAndExcludeSettingId(updateRequest.SettingId, updateRequest.Sequence.Value, existingQcValidationFlowSetting.ReviewGroupId, compId);
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




            _qcValidationFlowSettingService.UpdateQcValidationFlowSetting(updateRequest, existingQcValidationFlowSetting);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListQcValidationFlowSettings(string? compId = null)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var companyId = compId ?? memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (compId != null && compId != memberAndPermissionSetting.CompanyWithUnit.CompId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }
            var data = _qcValidationFlowSettingService.GetAllQcValidationFlowSettingsByCompId(companyId).OrderBy(pfs => pfs.Sequence).ToList();
            var response = new CommonResponse<List<QcValidationFlowSettingVo>>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("get/{settingId}")]
        [Authorize]
        public IActionResult GetQcValidationFlowSettingDetail(string settingId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var data = _qcValidationFlowSettingService.GetQcValidationFlowSettingBySettingId(settingId);
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
        public IActionResult InActiveQcValidationFlowSetting(string settingId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var existQcValidationFlowSetting = _qcValidationFlowSettingService.GetQcValidationFlowSettingBySettingId(settingId);
            if (existQcValidationFlowSetting == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此審核流程不存在"
                });
            }
            if (existQcValidationFlowSetting != null && existQcValidationFlowSetting.CompId != compId && AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }
            _qcValidationFlowSettingService.DeleteQcValidationFlowSetting(existQcValidationFlowSetting.SettingId);

            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = null
            };
            return Ok(response);
        }
    }
}
