using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Service;
using stock_api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;
using stock_api.Service.ValueObject;
using System.Collections.Generic;
using stock_api.Models;
using stock_api.Scheduler;
using Microsoft.IdentityModel.Tokens;

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
        private readonly WarehouseProductService _warehouseProductService;
        private readonly EmailService _emailService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(AuthHelpers authHelpers, MemberService memberService,CompanyService companyService, AuthLayerService authLayerService, WarehouseProductService warehouseProductService, EmailService emailService, ILogger<LoginController> logger)
        {
            _authHelpers = authHelpers;
            _memberService = memberService;
            _companyService = companyService;
            _authLayerService = authLayerService;
            _warehouseProductService = warehouseProductService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var result = new CommonResponse<Dictionary<string, dynamic>>
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
            result.Data = new Dictionary<string, dynamic> { { "token", token }, { "displayName", memberAndPermissionSetting.Member.DisplayName },{ "userId", memberAndPermissionSetting.Member.UserId }
            ,{ "compId", memberAndPermissionSetting.Member.CompId } ,{ "compName", memberAndPermissionSetting.CompanyWithUnit.Name },{ "compType", memberAndPermissionSetting.CompanyWithUnit.Type },{ "unitId", memberAndPermissionSetting.CompanyWithUnit.UnitId }
            ,{ "unitName", memberAndPermissionSetting.CompanyWithUnit.UnitName },{"isNoStockReviewer",memberAndPermissionSetting.Member.IsNoStockReviewer} };

            //NotifyNotEnoughProduct(memberAndPermissionSetting.Member.CompId, memberAndPermissionSetting.Member);
            return Ok(result);
        }

        private async Task NotifyNotEnoughProduct(string compId,WarehouseMember receiver)
        {
            List<NotifyProductQuantity> notifyProductQuantityList = await _warehouseProductService.FindAllProductQuantityNotifyList(compId);
            string emailTitle = "庫存品項不足通知";
            string emailBody = ProductQuantityNotifyJob.GenerateHtmlString(notifyProductQuantityList);
            if (!receiver.Email.IsNullOrEmpty())
            {
                await _emailService.SendAsync(emailTitle, emailBody, receiver.Email);
                _logger.LogInformation("[寄信]標題:{title},收件者:{email}", emailTitle, receiver.Email);
            }
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
