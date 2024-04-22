using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Service;
using stock_api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly AuthHelpers _authHelpers;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly AuthLayerService _authLayerService;

        public LoginController(AuthHelpers authHelpers, MemberService memberService,CompanyService companyService, AuthLayerService authLayerService)
        {
            _authHelpers = authHelpers;
            _memberService = memberService;
            _companyService = companyService;
            _authLayerService = authLayerService;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var result = new CommonResponse<Dictionary<string, string>>
            {
                Result = false,
                Message = "登入失敗",
                Data = null
            };
            var memberAndPermissionSetting = ValidateUser(loginRequest);
            if (memberAndPermissionSetting == null) return BadRequest(result);
            if (memberAndPermissionSetting.Member.IsActive == false)
            {
                result.Result = false;
                result.Message = "帳號停用";
                return BadRequest(result);
            }

            var token = _authHelpers.GenerateToken(memberAndPermissionSetting);
            result.Result = true;
            result.Message = "登入成功";
            result.Data = new Dictionary<string, string> { { "token", token }, { "displayName", memberAndPermissionSetting.Member.DisplayName },{ "userId", memberAndPermissionSetting.Member.UserId }
            ,{ "compId", memberAndPermissionSetting.Member.CompId } ,{ "compName", memberAndPermissionSetting.CompanyWithUnit.Name },{ "compType", memberAndPermissionSetting.CompanyWithUnit.Type },{ "unitId", memberAndPermissionSetting.CompanyWithUnit.UnitId }
            ,{ "unitName", memberAndPermissionSetting.CompanyWithUnit.UnitName }};
            return Ok(result);


        }

        MemberAndPermissionSetting? ValidateUser(LoginRequest loginRequest)
        {
            var member = _memberService.GetMemberByAccount(loginRequest.Account);
            if (member == null ) return null;
            var compWithUnit = _companyService.GetCompanyWithUnitByCompanyId(member.CompId);
            if (compWithUnit == null) return null;
            if (member.Password != loginRequest.Password) return null;

            var authLayer = _authLayerService.GetByAuthValue(member.AuthValue, member.CompId);
            if (authLayer == null) return null;
            PermissionSetting permissionSetting = new()
            {
                IsApplyItemManage = authLayer.IsApplyItemManage.Value,
                IsGroupManage = authLayer.IsGroupManage.Value,
                IsInBoundManage = authLayer.IsInBoundManage.Value,
                IsInventoryManage = authLayer.IsInventoryManage.Value,
                IsItemManage = authLayer.IsItemManage.Value,
                IsMemberManage = authLayer.IsMemberManage.Value,
                IsOutBoundManage = authLayer.IsOutBoundManage.Value,
                IsRestockManage = authLayer.IsRestockManage.Value,
                IsVerifyManage = authLayer.IsVerifyManage.Value,
            };


            return new MemberAndPermissionSetting(member, permissionSetting, compWithUnit);

        }
    }
}
