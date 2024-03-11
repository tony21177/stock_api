using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Service;
using stock_api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly AuthHelpers _authHelpers;
        private readonly MemberService _memberService;
        private readonly AuthLayerService _authLayerService;

        public LoginController(AuthHelpers authHelpers, MemberService memberService, AuthLayerService authLayerService)
        {
            _authHelpers = authHelpers;
            _memberService = memberService;
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
            result.Data = new Dictionary<string, string> { { "token", token }, { "displayName", memberAndPermissionSetting.Member.DisplayName },{ "userId", memberAndPermissionSetting.Member.UserId } };
            return Ok(result);


        }

        MemberAndPermissionSetting? ValidateUser(LoginRequest loginRequest)
        {
            var member = _memberService.GetMemberByAccount(loginRequest.Account);
            if (member == null) return null;
            if (member.Password != loginRequest.Password) return null;

            var authLayer = _authLayerService.GetByAuthValue(member.AuthValue);
            if (authLayer == null) return null;
            PermissionSetting permissionSetting = new PermissionSetting
            {
                IsCheckReport = authLayer.IsCheckReport,
                IsCreateAnnouce = authLayer.IsCreateAnnouce,
                IsDeleteAnnouce = authLayer.IsDeleteAnnouce,
                IsCreateHandover = authLayer.IsCreateHandover,
                IsDeleteHandover = authLayer.IsDeleteHandover,
                IsHideAnnouce = authLayer.IsHideAnnouce,
                IsMemberControl = authLayer.IsMemberControl,
                IsUpdateAnnouce = authLayer.IsUpdateAnnouce,
                IsUpdateHandover = authLayer.IsUpdateHandover,
            };


            return new MemberAndPermissionSetting(member, permissionSetting);

        }
    }
}
