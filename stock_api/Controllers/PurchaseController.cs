﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
using System.Linq;
using System.Runtime.InteropServices;

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
        private readonly IValidator<ListPurchaseRequest> _listPurchaseRequestValidator;
        private readonly IValidator<AnswerFlowRequest> _answerFlowRequestValidator;

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
            _listPurchaseRequestValidator = new ListPurchaseValidator(warehouseProductService, groupService);
            _answerFlowRequestValidator = new AnswerFlowValidator();
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreatePurchase(CreatePurchaseRequest createRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (createRequest.CompId != null && createRequest.CompId != compId && memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
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


        [HttpPost("list")]
        [Authorize]
        public IActionResult ListPurchases(ListPurchaseRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            if(request.CompId==null) request.CompId = compId;
            if (request.CompId != null && request.CompId != compId && memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _listPurchaseRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var data = _purchaseService.ListPurchase(request);
            var distinctProductIdList = data
            .SelectMany(item => item.Items) 
            .Select(item => item.ProductId) 
            .Distinct() 
            .ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);

            foreach(var vo in data)
            {
                foreach (var item in vo.Items)
                {
                    var matchedProduct = products.Where(p=>p.ProductId==item.ProductId).FirstOrDefault();
                    item.MaxSafeQuantity = matchedProduct?.MaxSafeQuantity;
                    item.ProductModel = matchedProduct?.ProductModel;
                    item.ManufacturerName = matchedProduct?.ManufacturerName;
                    item.ProductMachine = matchedProduct?.ProductMachine;
                }
            }


            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }


        [HttpGet("detail/{purchaseMainId}")]
        [Authorize]
        public IActionResult GetPurchasesDetail(string purchaseMainId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var purchaseMain = _purchaseService.GetPurchaseMainByMainId(purchaseMainId);
            if (purchaseMain == null)
            {
                return Ok(new CommonResponse<dynamic>
                {
                    Result = true,
                    Data = purchaseMain
                });
            }

            if (compId != purchaseMain.CompId && memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            List<PurchaseSubItem> purchaseSubItems = _purchaseService.GetPurchaseSubItemsByMainId(purchaseMainId);
            List<PurchaseFlow> purchaseFlows = _purchaseService.GetPurchaseFlowsByMainId(purchaseMainId).OrderBy(f=>f.Sequence).ToList();
            List<PurchaseFlowLog> purchaseFlowLogs = _purchaseService.GetPurchaseFlowLogsByMainId(purchaseMainId).OrderBy(fl => fl.UpdatedAt).ToList();
            var purchaseAndSubItemVo = _mapper.Map<PurchaseMainAndSubItemVo>(purchaseMain);
            var purchaseSubItemVoList = _mapper.Map<List<PurchaseSubItemVo>>(purchaseSubItems);
            purchaseAndSubItemVo.Items = purchaseSubItemVoList;
            purchaseAndSubItemVo.flows = purchaseFlows;
            purchaseAndSubItemVo.flowLogs = purchaseFlowLogs;

            var distinctProductIdList = purchaseSubItems.Select(s => s.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);
            purchaseSubItemVoList.ForEach(item =>
            {
                var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.MaxSafeQuantity = matchedProduct?.MaxSafeQuantity;
                item.ProductModel = matchedProduct?.ProductModel;
                item.ManufacturerName = matchedProduct?.ManufacturerName;
                item.ProductMachine = matchedProduct?.ProductMachine;
            });


            purchaseFlows.ForEach(f =>
            {
                if (f.VerifyUserId == memberAndPermissionSetting.Member.UserId)
                {
                    _purchaseService.PurchaseFlowRead(f);
                }
            });

            var response = new CommonResponse<PurchaseMainAndSubItemVo>
            {
                Result = true,
                Data = purchaseAndSubItemVo
            };
            return Ok(response);
        }

        [HttpPost("flow/answer")]
        [Authorize]
        public IActionResult FlowSign(AnswerFlowRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var verifier = memberAndPermissionSetting.Member;
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var validationResult = _answerFlowRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var purchaseFlow = _purchaseService.GetFlowsByFlowId(request.FlowId);
            if (purchaseFlow != null && purchaseFlow.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (purchaseFlow != null && purchaseFlow.VerifyUserId != verifier.UserId )
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (purchaseFlow != null && !purchaseFlow.Answer.IsNullOrEmpty())
            {
                return BadRequest(new CommonResponse<dynamic>(){
                    Result = false,
                    Message = "不能重複審核"
                });
            }

            if (purchaseFlow == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result =false,
                    Message = "審核流程不存在"
                });
            }


            var result = _purchaseService.AnswerFlow(purchaseFlow, memberAndPermissionSetting,request.Answer,request.Reason);


            var response = new CommonResponse<dynamic>
            {
                Result = result,
                Data = null
            };
            return Ok(response);
        }

        [HttpGet("flows/my")]
        [Authorize]
        public IActionResult GetFlowsSignedByMy()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;


            var purchaseFlowsSignedByMe = _purchaseService.GetFlowsByUserId(memberAndPermissionSetting.Member.UserId);
            

            var distinctMainIdList = purchaseFlowsSignedByMe.Select(f=>f.PurchaseMainId).Distinct().ToList();
            var purchaseMainList = _purchaseService.GetPurchaseMainsByMainIdList(distinctMainIdList).OrderByDescending(m=>m.UpdatedAt).ToList();
            var purchaseSubItems = _purchaseService.GetPurchaseSubItemsByMainIdList(distinctMainIdList);
            var purchaseFlows = _purchaseService.GetPurchaseFlowsByMainIdList(distinctMainIdList);
            var purchaseFlowLogs = _purchaseService.GetPurchaseFlowLogsByMainIdList(distinctMainIdList);

            var distinctProductIdList = purchaseSubItems.Select(s=>s.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);

            List <PurchaseMainAndSubItemVo> purchaseMainAndSubItemVoList = new List<PurchaseMainAndSubItemVo>();

            purchaseMainList.ForEach(m =>
            {
                var purchaseMainAndSubItemVo = _mapper.Map<PurchaseMainAndSubItemVo>(m);

                var matchedSubItems = purchaseSubItems.Where(s=>s.PurchaseMainId==m.PurchaseMainId).OrderBy(s=>s.UpdatedAt).ToList() ;
                var items = _mapper.Map<List<PurchaseSubItemVo>>(matchedSubItems);
                var matchedFlows = purchaseFlows.Where(f=>f.PurchaseMainId==m.PurchaseMainId).OrderBy(f => f.Sequence).ToList();
                var matchedFlowLogs = purchaseFlowLogs.Where(l => l.PurchaseMainId == m.PurchaseMainId).OrderBy(l => l.UpdatedAt).ToList();

                items.ForEach(item =>
                {
                    var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                    item.MaxSafeQuantity = matchedProduct?.MaxSafeQuantity;
                    item.ProductModel = matchedProduct?.ProductModel;
                    item.ManufacturerName = matchedProduct?.ManufacturerName;
                    item.ProductMachine = matchedProduct?.ProductMachine;
                });

                purchaseMainAndSubItemVo.Items = items;
                purchaseMainAndSubItemVo.flows = matchedFlows;
                purchaseMainAndSubItemVo.flowLogs = matchedFlowLogs;
                purchaseMainAndSubItemVoList.Add(purchaseMainAndSubItemVo);
            });
            purchaseMainAndSubItemVoList = purchaseMainAndSubItemVoList.OrderBy(m => m.UpdatedAt).ToList();


            var response = new CommonResponse<List<PurchaseMainAndSubItemVo>>
            {
                Result = true,
                Data = purchaseMainAndSubItemVoList
            };
            return Ok(response);
        }
    }
}
