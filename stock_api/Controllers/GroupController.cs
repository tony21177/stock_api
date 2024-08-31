using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Common.Constant;
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
    public class GroupController : ControllerBase
    {
        private readonly AuthLayerService _authLayerService;
        private readonly CompanyService _companyService;
        private readonly MemberService _memberService;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly GroupService _groupService;
        private readonly IValidator<CreateGroupRequest> _createGroupRequestValidator;
        private readonly IValidator<UpdateGroupRequest> _updateGroupRequestValidator;


        public GroupController(AuthLayerService authLayerService, CompanyService companyService, MemberService memberService, IMapper mapper, AuthHelpers authHelpers, GroupService groupService)
        {
            _authLayerService = authLayerService;
            _companyService = companyService;
            _memberService = memberService;
            _mapper = mapper;
            _authHelpers = authHelpers;
            _groupService = groupService;
            _createGroupRequestValidator = new CreateGroupValidator();
            _updateGroupRequestValidator = new UpdateGroupValidator();

        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateGroup(CreateGroupRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (memberAndPermissionSetting.PermissionSetting.IsGroupManage != true)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _createGroupRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var newGroup = _mapper.Map<WarehouseGroup>(request);
            newGroup.CompId = compId;
            _groupService.AddGroup(newGroup);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListAllGroup()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var data = _groupService.GetGroups(compId);
            var sortedData = data.OrderByDescending(e => e.CreatedAt).ToList();


            var response = new CommonResponse<List<WarehouseGroup>>()
            {
                Result = true,
                Message = "",
                Data = sortedData
            };
            return Ok(response);
        }

        [HttpPost("listByCompId")]
        [Authorize]
        public IActionResult ListGroupsByCompId(ListGroupRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var loginUserCompId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var companyWithUnitVo = _companyService.GetCompanyWithUnitByCompanyId(request.CompId);
            //if(companyWithUnitVo == null){
            //    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            //}
            //if (loginUserCompId != companyWithUnitVo.CompId && memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            //{
            //    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            //}
            var data = _groupService.GetGroupsByCompId(request.CompId);
            var sortedData = data.OrderByDescending(_e => _e.CreatedAt).ToList();

            var response = new CommonResponse<List<WarehouseGroup>>()
            {
                Result = true,
                Message = "",
                Data = sortedData
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult UpdateGroup(UpdateGroupRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (memberAndPermissionSetting.PermissionSetting.IsGroupManage != true)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _updateGroupRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var existingGroup = _groupService.GetGroupByGroupId(request.GroupId);
            if (existingGroup == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "群組不存在"
                });
            }
            if (existingGroup.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            _groupService.UpdateGroup(request, existingGroup);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpDelete("{groupId}")]
        [Authorize]
        public IActionResult InActiveGroup(string groupId)
        {

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.PermissionSetting.IsGroupManage != true)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }


            var existingGroup = _groupService.GetGroupByGroupId(groupId);
            if (existingGroup == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "群組不存在"
                });
            }
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (existingGroup.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            _groupService.UpdateGroup(new UpdateGroupRequest()
            {
                IsActive = false
            }, existingGroup);

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
