using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using System;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly PurchaseFlowSettingService _purchaseFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly PurchaseService _purchaseService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly GroupService _groupService;
        private readonly IValidator<CreatePurchaseRequest> _createPurchaseValidator;

        public PurchaseController(IMapper mapper, AuthHelpers authHelpers, PurchaseFlowSettingService purchaseFlowSettingService, MemberService memberService, CompanyService companyService, PurchaseService purchaseService, WarehouseProductService warehouseProductService,GroupService groupService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _purchaseFlowSettingService = purchaseFlowSettingService;
            _memberService = memberService;
            _companyService = companyService;
            _purchaseService = purchaseService;
            _warehouseProductService = warehouseProductService;
            _groupService = groupService;
            _createPurchaseValidator = new CreatePurchaseValidator(warehouseProductService, groupService);
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreatePurchase(CreatePurchaseRequest createRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (createRequest.CompId != null && createRequest.CompId != compId && memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.Owner)
            {
                // 非OWNER不可幫其他組織申請單
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            createRequest.CompId ??= compId;

            var validationResult = _createPurchaseValidator.Validate(createRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var demandDateTime = DateTimeHelper.ParseDateString(createRequest.DemandDate);

            var newPurchaseMain = new PurchaseMainSheet()
            {
                CompId = createRequest.CompId,
                DemandDate = new DateOnly(demandDateTime.Value.Year, demandDateTime.Value.Month, demandDateTime.Value.Day),
                GroupIds = createRequest.GroupIds == null ? null : string.Join(',', createRequest.GroupIds),
                Remarks = createRequest.Remarks,
                UserId = memberAndPermissionSetting.Member.UserId,
                Type = createRequest.Type,
                IsActive = true
            };

            // sub item
            var productIds = createRequest.PurchaseSubItems.Select(x => x.ProductId).ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIds, createRequest.CompId);
            var distinctGroupIds = createRequest.PurchaseSubItems
            .SelectMany(request => request.GroupIds ?? Enumerable.Empty<string>())
            .Distinct().ToList();
            var groups = _groupService.GetGroupsByIdList(distinctGroupIds);

            var purchaseSubItemList = new List<Models.PurchaseSubItem>();
            createRequest.PurchaseSubItems.ForEach(purchaseSubItem =>
            {
                var matchedProduct = products.Where(p=>p.ProductId == purchaseSubItem.ProductId).First();

                var matchedGroups = groups
                .Where(g => purchaseSubItem.GroupIds?.Contains(g.GroupId) ?? false)
                .OrderBy(g => purchaseSubItem.GroupIds.IndexOf(g.GroupId))
                .ToList();


                var newPurchaseSubItem = new Models.PurchaseSubItem()
                {
                    Comment = purchaseSubItem.Comment,
                    CompId = createRequest.CompId,
                    ProductId = matchedProduct.ProductId,
                    ProductName = matchedProduct.ProductName,
                    ProductCategory = matchedProduct.ProductCategory,
                    ProductSpec = matchedProduct.ProductSpec,
                    Quantity = purchaseSubItem.Quantity,
                    GroupIds = string.Join(",", matchedGroups.Select(g => g.GroupId).ToList()),
                    GroupNames = string.Join(",", matchedGroups.Select(g => g.GroupName).ToList()),
                    CurrentInStockQuantity = matchedProduct.InStockQuantity
                };
                purchaseSubItemList.Add(newPurchaseSubItem);
            });
            List<PurchaseFlowSettingVo> purchaseFlowSettingList = _purchaseFlowSettingService.GetAllPurchaseFlowSettingsByCompId(createRequest.CompId);
            var result = _purchaseService.CreatePurchase(newPurchaseMain, purchaseSubItemList, purchaseFlowSettingList.Where(s=>s.IsActive==true).ToList());
            var response = new CommonResponse<dynamic>
            {
                Result = result,
                Data = null
            };
            return Ok(response);
        }

        //[HttpPost("update")]
        //[Authorize]
        //public IActionResult UpdatePurchaseFlowSetting(CreateOrUpdatePurchaseFlowSettingRequest updateRequest)
        //{
        //    var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
        //    var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
        //    var flowId = updateRequest.FlowId;
        //    var existingPurchaseFlowSetting =_purchaseFlowSettingService.GetPurchaseFlowSettingByFlowId(flowId);
        //    if (existingPurchaseFlowSetting==null)
        //    {
        //        return BadRequest(new CommonResponse<dynamic>
        //        {
        //            Result = false,
        //            Message = "此審核流程不存在"
        //        });
        //    }
        //    if (compId != existingPurchaseFlowSetting.CompId)
        //    {
        //        return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
        //    }

        //    updateRequest.CompId = compId;
        //    var validationResult = _updatePurchaseFlowSettingValidator.Validate(updateRequest);

        //    if (!validationResult.IsValid)
        //    {
        //        return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
        //    }

            
        //    _purchaseFlowSettingService.UpdatePurchaseFlowSetting(updateRequest, existingPurchaseFlowSetting);
        //    var response = new CommonResponse<dynamic>
        //    {
        //        Result = true,
        //        Data = null
        //    };
        //    return Ok(response);
        //}

        //[HttpGet("list")]
        //[Authorize]
        //public IActionResult ListPurchaseFlowSettings()
        //{
        //    var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
        //    var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
        //    var data = _purchaseFlowSettingService.GetAllPurchaseFlowSettingsByCompId(compId).OrderBy(pfs => pfs.Sequence);
        //    var response = new CommonResponse<dynamic>
        //    {
        //        Result = true,
        //        Data = data
        //    };
        //    return Ok(response);
        //}

        //[HttpGet("get/{flowId}")]
        //[Authorize]
        //public IActionResult GetPurchaseFlowSettingDetail(string flowId)
        //{
        //    var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
        //    var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
        //    var data = _purchaseFlowSettingService.GetPurchaseFlowSettingByFlowId(flowId);
        //    if (data != null&&data.CompId!=compId)
        //    {
        //        return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
        //    }


        //    var response = new CommonResponse<dynamic>
        //    {
        //        Result = true,
        //        Data = data
        //    };
        //    return Ok(response);
        //}

        //[HttpDelete("delete/{flowId}")]
        //[Authorize]
        //public IActionResult InActivePurchaseFlowSetting(string flowId)
        //{
        //    var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
        //    var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
        //    var existPurchaseFlowSetting = _purchaseFlowSettingService.GetPurchaseFlowSettingByFlowId(flowId);
        //    if (existPurchaseFlowSetting != null && existPurchaseFlowSetting.CompId != compId)
        //    {
        //        return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
        //    }
        //    _purchaseFlowSettingService.UpdatePurchaseFlowSetting(new CreateOrUpdatePurchaseFlowSettingRequest() { FlowId=flowId,IsActive=false}, existPurchaseFlowSetting);

        //    var response = new CommonResponse<dynamic>
        //    {
        //        Result = true,
        //        Data = null
        //    };
        //    return Ok(response);
        //}
    }
}
