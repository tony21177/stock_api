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

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly MemberService _memberService;
        private readonly AuthLayerService _authLayerService;
        private readonly IMapper _mapper;
        private readonly ILogger<MemberController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly IValidator<CreateOrUpdateMemberRequest> _createMemberRequestValidator;
        private readonly IValidator<CreateOrUpdateMemberRequest> _updateMemberRequestValidator;
        public MemberController(MemberService memberService, AuthLayerService authLayerService, IMapper mapper, ILogger<MemberController> logger, AuthHelpers authHelpers)
        {
            _memberService = memberService;
            _authLayerService = authLayerService;
            _mapper = mapper;
            _logger = logger;
            _createMemberRequestValidator = new CreateOrUpdateMemberValidator(ActionTypeEnum.Create, authLayerService, memberService);
            _updateMemberRequestValidator = new CreateOrUpdateMemberValidator(ActionTypeEnum.Update, authLayerService, memberService);
            _authHelpers = authHelpers;
        }

        [HttpGet("list")]
        [Authorize]
        public CommonResponse<List<MemberDto>> List()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);

            var data = _memberService.GetAllMembersOfComp(memberAndPermissionSetting.CompanyWithUnit.CompId);
            var memberDtos = _mapper.Map<List<MemberDto>>(data);
            var response = new CommonResponse<List<MemberDto>>()
            {
                Result = true,
                Message = "",
                Data = memberDtos
            };
            return response;
        }

        [HttpGet("owner/list")]
        [Authorize]
        public IActionResult ListAll()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit == null || memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.Owner)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var data = _memberService.GetAllMembersForOwner();
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

            var validationResult = await _createMemberRequestValidator.ValidateAsync(createMemberRequset);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var newMember = _mapper.Map<WarehouseMember>(createMemberRequset);
            newMember.UserId = Guid.NewGuid().ToString();

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
            var validationResult = await _updateMemberRequestValidator.ValidateAsync(updateMemberRequset);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var updateMember = _mapper.Map<WarehouseMember>(updateMemberRequset);
            var updatedMember = _memberService.UpdateMember(updateMember);
            var updateedMemberDto = _mapper.Map<MemberDto>(updatedMember);
            var response = new CommonResponse<MemberDto>
            {
                Result = true,
                Data = updateedMemberDto
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
            _memberService.DeleteMember(userId);

            var response = new CommonResponse<WarehouseMember>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }
    }
}
