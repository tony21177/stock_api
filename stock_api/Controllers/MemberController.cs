using AutoMapper;
using FluentValidation;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using MaiBackend.PublicApi.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common.Constant;
using AutoMapper.Execution;
using stock_api.Common.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(PermissionFilterAttribute))]
    public class MemberController : ControllerBase
    {
        private readonly MemberService _memberService;
        private readonly GroupService _groupService;
        private readonly AuthLayerService _authLayerService;
        private readonly IMapper _mapper;
        private readonly ILogger<MemberController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly IValidator<CreateOrUpdateMemberRequest> _createMemberRequestValidator;
        private readonly IValidator<CreateOrUpdateMemberRequest> _updateMemberRequestValidator;
        private readonly IValidator<UpdateMemberGroupRequest> _updateMemberGroupRequestRequestValidator;
        public MemberController(MemberService memberService,GroupService groupService, AuthLayerService authLayerService, IMapper mapper, ILogger<MemberController> logger, AuthHelpers authHelpers)
        {
            _memberService = memberService;
            _groupService = groupService;
            _authLayerService = authLayerService;
            _mapper = mapper;
            _logger = logger;
            _createMemberRequestValidator = new CreateOrUpdateMemberValidator(ActionTypeEnum.Create, authLayerService, memberService,groupService);
            _updateMemberRequestValidator = new CreateOrUpdateMemberValidator(ActionTypeEnum.Update, authLayerService, memberService, groupService );
            _authHelpers = authHelpers;
            _updateMemberGroupRequestRequestValidator = new UpdateMemberGroupValidator(_groupService);
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult List(string? compId = null)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var companyId = compId ?? memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
            }

            var memberList = _memberService.GetAllMembersOfComp(companyId);
            List<string> distinctGroupIds = memberList
                .SelectMany(member =>
                    (member.GroupIds ?? "")
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .ToList();
            var memberDtos = _mapper.Map<List<MemberDto>>(memberList);
            var groups = _groupService.GetGroupsByIdList(distinctGroupIds);

            memberDtos.ForEach(dto =>
            {
                var matchedGroups = groups.Where(g => dto.GroupIds.Contains(g.GroupId)).ToList();
                dto.Groups = matchedGroups;
            });
            var data = memberDtos.OrderByDescending(dto => dto.CreatedAt).ToList();

            var response = new CommonResponse<List<MemberDto>>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("owner/list")]
        [Authorize]
        public IActionResult ListAll(string? compId = null)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit == null || memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var memberList = _memberService.GetAllMembersForOwner(compId);

            List<string> distinctGroupIds = memberList
           .SelectMany(member =>member.GroupIds)
           .Distinct()
           .ToList();
            var groups = _groupService.GetGroupsByIdList(distinctGroupIds);

            memberList.ForEach(dto =>
            {
                var matchedGroups = groups.Where(g => dto.GroupIds.Contains(g.GroupId)).ToList();
                dto.Groups = matchedGroups;
            });
            var data = memberList.OrderByDescending(dto => dto.CreatedAt).ToList();

            var response = new CommonResponse<List<MemberWithCompanyUnitVo>>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return Ok(response);
        }



        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create(CreateOrUpdateMemberRequest createMemberRequset)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            if (memberAndPermissionSetting == null || permissionSetting == null || !permissionSetting.IsMemberManage)
            {
                return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (createMemberRequset.CompId != null&&createMemberRequset.CompId!= memberAndPermissionSetting.CompanyWithUnit.CompId)
            {
                if (AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
                {
                    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
                }
            }


            if (createMemberRequset.CompId == null)
            {
                createMemberRequset.CompId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            }



            var validationResult = await _createMemberRequestValidator.ValidateAsync(createMemberRequset);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var newMember = _mapper.Map<WarehouseMember>(createMemberRequset);
            newMember.UserId = Guid.NewGuid().ToString();
            if (createMemberRequset.Agents != null && createMemberRequset.Agents.Count > 0)
            {
                var agentMemberList = _memberService.GetActiveMembersByUserIds(createMemberRequset.Agents,createMemberRequset.CompId);
                var agentNames = string.Join(",", agentMemberList.Select(m => m.DisplayName).ToList());
                newMember.AgentNames = agentNames;
            }

            newMember = _memberService.CreateMember(newMember!);
            var newMemberDto = _mapper.Map<MemberDto>(newMember);
            var response = new CommonResponse<MemberDto>
            {
                Result = true,
                Data = newMemberDto
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> Update(CreateOrUpdateMemberRequest updateMemberRequset)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            if (memberAndPermissionSetting == null || permissionSetting == null || !permissionSetting.IsMemberManage)
            {
                return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            

            var existingMember = _memberService.GetMemberByUserId(updateMemberRequset.UserId);
            if (existingMember == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "使用者不存在"
                });
            }

            var validationResult = await _updateMemberRequestValidator.ValidateAsync(updateMemberRequset);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            if (existingMember.CompId != memberAndPermissionSetting.CompanyWithUnit.CompId)
            {
                if (AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
                {
                    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
                }
            }


            _memberService.UpdateMember(updateMemberRequset, existingMember);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
            };
            return Ok(response);
        }

        [HttpDelete("delete/{userId}")]
        [Authorize]
        public IActionResult Delete(string userId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var permissionSetting = memberAndPermissionSetting?.PermissionSetting;
            if (memberAndPermissionSetting == null || permissionSetting == null || !permissionSetting.IsMemberManage)
            {
                return Unauthorized(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var existingMember = _memberService.GetMemberByUserId(userId);
            if (existingMember == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "使用者不存在"
                });
            }

            if (existingMember.CompId != memberAndPermissionSetting.CompanyWithUnit.CompId)
            {
                if (AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
                {
                    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
                }
            }
            _memberService.DeleteMember(userId);

            var response = new CommonResponse<WarehouseMember>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("updateMemberGroup")]
        [Authorize]
        public IActionResult UpdateMemberGroup(UpdateMemberGroupRequest updateMemberGroup)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.PermissionSetting.IsGroupManage != true)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var memberList = _memberService.GetActiveMembersByUserIds(new List<string> { updateMemberGroup.UserId},memberAndPermissionSetting.CompanyWithUnit.CompId);
            if (memberList.Count == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該使用者不存在"
                });
            }

            if (memberList[0].CompId != memberAndPermissionSetting.CompanyWithUnit.CompId)
            {
                if (AuthUtils.IsCrossCompAuthorized(memberAndPermissionSetting))
                {
                    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeCrossCompResponse());
                }
            }

            var validationResult = _updateMemberGroupRequestRequestValidator.Validate(updateMemberGroup);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            _memberService.UpdateMemberGroupIds(memberList[0], updateMemberGroup.GroupIds);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }
    }
}
