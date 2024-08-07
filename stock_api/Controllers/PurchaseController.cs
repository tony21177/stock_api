﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI.Common;
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
        private readonly ApplyProductFlowSettingService _applyProductFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly PurchaseService _purchaseService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly GroupService _groupService;
        private readonly SupplierService _supplierService;
        private readonly IValidator<CreatePurchaseRequest> _createPurchaseValidator;
        private readonly IValidator<ListPurchaseRequest> _listPurchaseRequestValidator;
        private readonly IValidator<AnswerFlowRequest> _answerFlowRequestValidator;
        private readonly IValidator<UpdateOwnerProcessRequest> _updateOwnerProcessRequestValidator;


        public PurchaseController(IMapper mapper, AuthHelpers authHelpers, PurchaseFlowSettingService purchaseFlowSettingService, MemberService memberService, CompanyService companyService
            ,SupplierService supplierService, PurchaseService purchaseService, WarehouseProductService warehouseProductService, GroupService groupService, ApplyProductFlowSettingService applyProductFlowSettingService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _purchaseFlowSettingService = purchaseFlowSettingService;
            _memberService = memberService;
            _companyService = companyService;
            _purchaseService = purchaseService;
            _warehouseProductService = warehouseProductService;
            _groupService = groupService;
            _supplierService = supplierService;
            _createPurchaseValidator = new CreatePurchaseValidator(warehouseProductService, groupService, purchaseService);
            _listPurchaseRequestValidator = new ListPurchaseValidator(warehouseProductService, groupService);
            _answerFlowRequestValidator = new AnswerFlowValidator();
            _updateOwnerProcessRequestValidator = new UpdateOwnerProcessValidator();
            _applyProductFlowSettingService = applyProductFlowSettingService;
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

            //品項組別
            bool isItemMultiGroup = false;
            List<string> itemGroupIdList = new();
            createRequest.PurchaseSubItems.ForEach(i =>
            {
                if (i.GroupIds != null)
                {
                    itemGroupIdList.AddRange(i.GroupIds);
                }
            });
            itemGroupIdList = itemGroupIdList.Distinct().ToList();
            if(itemGroupIdList.Count>1) isItemMultiGroup= true;

            List<ApplyProductFlowSettingVo> applyProductFlowSettingListForGroupReview = new();
            // 沒有跨組別走組別審核流程
            if (isItemMultiGroup == false&& itemGroupIdList.Count==1)
            {
                applyProductFlowSettingListForGroupReview = _applyProductFlowSettingService.GetApplyProductFlowSettingVoListByGroupId(itemGroupIdList[0]);
                if (applyProductFlowSettingListForGroupReview.Count == 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "尚未建立組別審核流程關卡"
                    });
                }
            }

            List<PurchaseFlowSettingVo> purchaseFlowSettingList = _purchaseFlowSettingService.GetAllPurchaseFlowSettingsByCompId(createRequest.CompId).Where(s => s.IsActive == true).ToList();
            if (purchaseFlowSettingList.Count == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "尚未建立跨組別審核流程關卡"
                });
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
                var matchedProduct = products.Where(p => p.ProductId == purchaseSubItem.ProductId).First();

                var matchedGroups = groups
                .Where(g => purchaseSubItem.GroupIds?.Contains(g.GroupId) ?? false)
                .OrderBy(g => purchaseSubItem.GroupIds.IndexOf(g.GroupId))
                .ToList();


                var newPurchaseSubItem = new Models.PurchaseSubItem()
                {
                    Comment = purchaseSubItem.Comment,
                    CompId = createRequest.CompId,
                    ProductId = matchedProduct.ProductId,
                    ProductCode = matchedProduct.ProductCode,
                    ProductName = matchedProduct.ProductName,
                    ProductCategory = matchedProduct.ProductCategory,
                    ProductSpec = matchedProduct.ProductSpec,
                    UdiserialCode = matchedProduct.UdiserialCode,
                    Quantity = purchaseSubItem.Quantity,
                    GroupIds = string.Join(",", matchedGroups.Select(g => g.GroupId).ToList()),
                    GroupNames = string.Join(",", matchedGroups.Select(g => g.GroupName).ToList()),
                    CurrentInStockQuantity = matchedProduct.InStockQuantity,
                    WithPurchaseMainId = purchaseSubItem.WithPurchaseMainId,
                    WithItemId = purchaseSubItem.WithItemId,
                    WithCompId = purchaseSubItem.WithCompId,
                    ArrangeSupplierId = matchedProduct.DefaultSupplierId,
                    ArrangeSupplierName = matchedProduct.DefaultSupplierName,
                };
                purchaseSubItemList.Add(newPurchaseSubItem);
            });
            var result = _purchaseService.CreatePurchase(newPurchaseMain, purchaseSubItemList, purchaseFlowSettingList.Where(s => s.IsActive == true).ToList(), applyProductFlowSettingListForGroupReview,isItemMultiGroup, memberAndPermissionSetting.CompanyWithUnit.Type == CommonConstants.CompanyType.OWNER);
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

            if (request.CompId == null) request.CompId = compId;
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
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, request.CompId);

            foreach (var vo in data)
            {
                foreach (var item in vo.Items)
                {
                    var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                    item.MaxSafeQuantity = matchedProduct?.MaxSafeQuantity;
                    item.ProductModel = matchedProduct?.ProductModel;
                    item.ManufacturerName = matchedProduct?.ManufacturerName;
                    item.ProductMachine = matchedProduct?.ProductMachine;
                    item.ProductUnit = matchedProduct?.Unit;
                    item.UnitConversion = matchedProduct?.UnitConversion;
                    item.TestCount = matchedProduct?.TestCount;
                    item.Delivery = matchedProduct?.Delievery;
                    item.PackageWay = matchedProduct?.PackageWay;
                    item.ProductCode = matchedProduct?.ProductCode;
                    item.SupplierUnitConvertsion = matchedProduct?.SupplierUnitConvertsion;
                    item.SupplierUnit = matchedProduct?.SupplierUnit;
                    item.StockLocation = matchedProduct?.StockLocation;
                }
            }
            data = data.OrderByDescending(item => item.ApplyDate).ToList();

            //
            int totalPages = 0;
            var orderByField = request.PaginationCondition.OrderByField;
            if (orderByField != null)
            {
                orderByField = StringUtils.CapitalizeFirstLetter(orderByField);
                if (request.PaginationCondition.IsDescOrderBy)
                {
                    switch (orderByField)
                    {
                        case "ApplyDate":
                            data = data.OrderByDescending(item => item.ApplyDate).ToList();
                            break;
                        case "DemandDate":
                            data = data.OrderByDescending(item => item.DemandDate).ToList();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (orderByField)
                    {
                        case "ApplyDate":
                            data = data.OrderBy(item => item.ApplyDate).ToList();
                            break;
                        case "DemandDate":
                            data = data.OrderBy(item => item.DemandDate).ToList();
                            break;
                        default:
                            break;
                    }
                }
                int totalItems = data.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);
                data = data.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize).ToList();
            }

            var response = new CommonResponse<List<PurchaseMainAndSubItemVo>>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("owner/list")]
        [Authorize]
        public IActionResult OwnerListPurchases()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            // ListPurchaseRequest request = new() { CurrentStatus = CommonConstants.PurchaseFlowAnswer.AGREE,ReceiveStatus= CommonConstants.PurchaseReceiveStatus.NONE };
            ListPurchaseRequest request = new() { CurrentStatus = CommonConstants.PurchaseFlowAnswer.AGREE };

            var data = _purchaseService.ListPurchase(request);
            var distinctProductIdList = data
            .SelectMany(item => item.Items)
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();
            var products = _warehouseProductService.GetProductsByProductIds(distinctProductIdList);

            foreach (var vo in data)
            {
                foreach (var item in vo.Items)
                {
                    var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                    item.MaxSafeQuantity = matchedProduct?.MaxSafeQuantity;
                    item.ProductModel = matchedProduct?.ProductModel;
                    item.ManufacturerName = matchedProduct?.ManufacturerName;
                    item.ProductMachine = matchedProduct?.ProductMachine;
                    item.ProductUnit = matchedProduct?.Unit;
                    item.UnitConversion = matchedProduct?.UnitConversion;
                    item.TestCount = matchedProduct?.TestCount;
                    item.Delivery = matchedProduct?.Delievery;
                    item.PackageWay = matchedProduct?.PackageWay;
                    item.ProductCode = matchedProduct?.ProductCode;
                    item.SupplierUnitConvertsion = matchedProduct?.SupplierUnitConvertsion;
                    item.SupplierUnit = matchedProduct?.SupplierUnit;
                    item.OpenedSealName = matchedProduct?.OpenedSealName;
                    item.StockLocation = matchedProduct?.StockLocation;
                }
            }
            data = data.OrderByDescending(item => item.ApplyDate).ToList();

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
            List<PurchaseFlow> purchaseFlows = _purchaseService.GetPurchaseFlowsByMainId(purchaseMainId).OrderBy(f => f.Sequence).ToList();
            List<PurchaseFlowLog> purchaseFlowLogs = _purchaseService.GetPurchaseFlowLogsByMainId(purchaseMainId).OrderBy(fl => fl.UpdatedAt).ToList();
            var distinctWithCompId = purchaseSubItems.Where(i=>i.WithCompId!=null).Select(i=>i.WithCompId).Distinct().ToList();
            List<CompanyWithUnitVo> companyWithUnitVoList = _companyService.GetCompanyWithUnitListByCompanyIdList(distinctWithCompId);

            var purchaseAndSubItemVo = _mapper.Map<PurchaseMainAndSubItemVo>(purchaseMain);
            var purchaseSubItemVoList = _mapper.Map<List<PurchaseSubItemVo>>(purchaseSubItems);

            var distinctProductIdList = purchaseSubItems.Select(s => s.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, purchaseMain.CompId);
            purchaseSubItemVoList.ForEach(item =>
            {
                var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.MaxSafeQuantity = matchedProduct?.MaxSafeQuantity;
                item.ProductModel = matchedProduct?.ProductModel;
                item.ManufacturerName = matchedProduct?.ManufacturerName;
                item.ProductMachine = matchedProduct?.ProductMachine;
                item.ProductUnit = matchedProduct?.Unit;
                item.UnitConversion = matchedProduct?.UnitConversion;
                item.TestCount = matchedProduct?.TestCount;
                item.Delivery = matchedProduct?.Delievery;
                item.PackageWay = matchedProduct?.PackageWay;
                item.ProductCode = matchedProduct?.ProductCode;
                item.SupplierUnit = matchedProduct?.SupplierUnit;
                item.SupplierUnitConvertsion = matchedProduct?.SupplierUnitConvertsion;
                item.StockLocation = matchedProduct?.StockLocation;
                if (item.WithCompId != null)
                {
                    var matchedCompanyWithUnitVo = companyWithUnitVoList.Where(c=>c.CompId==item.WithCompId).FirstOrDefault();
                    if (matchedCompanyWithUnitVo != null)
                    {
                        item.WithCompName = matchedCompanyWithUnitVo.UnitName+matchedCompanyWithUnitVo.Name;
                    }
                }

            });
            purchaseAndSubItemVo.Items = purchaseSubItemVoList;
            purchaseAndSubItemVo.flows = purchaseFlows;
            purchaseAndSubItemVo.flowLogs = purchaseFlowLogs;

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

            if(purchaseFlow!=null && memberAndPermissionSetting.CompanyWithUnit.Type == CommonConstants.CompanyType.OWNER&&request.Answer==CommonConstants.AnswerPurchaseFlow.BACK)
            {
                // 金萬林退回
                var backResult = _purchaseService.AnswerFlow(purchaseFlow, memberAndPermissionSetting, request.Answer, request.Reason,true);
                var backResponse = new CommonResponse<dynamic>
                {
                    Result = backResult,
                    Data = null
                };
                return Ok(backResponse);

            }

            if (purchaseFlow != null && purchaseFlow.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (purchaseFlow != null && purchaseFlow.VerifyUserId != verifier.UserId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (purchaseFlow != null && !purchaseFlow.Answer.IsNullOrEmpty())
            {
                return BadRequest(new CommonResponse<dynamic>() {
                    Result = false,
                    Message = "不能重複審核"
                });
            }

            if (purchaseFlow == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "審核流程不存在"
                });
            }
            var beforeFlows = _purchaseService.GetBeforeFlows(purchaseFlow);
            if (beforeFlows.Any(f => f.Answer == CommonConstants.PurchaseFlowAnswer.EMPTY))
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "之前的審核流程還在跑"
                });
            }

            var result = _purchaseService.AnswerFlow(purchaseFlow, memberAndPermissionSetting, request.Answer, request.Reason,false);


            var response = new CommonResponse<dynamic>
            {
                Result = result,
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("flow/updateOrDeleteItem")]
        [Authorize]
        public IActionResult UpdateOrDeleteSubItemWhenFlow(UpdateOrDeleteSubItemInFlowRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var verifier = memberAndPermissionSetting.Member;
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var currentPurchaseFlow = _purchaseService.GetFlowsByPurchaseMainIds(new List<string> { request.PurchaseMainId})
                .Where(f=>f.Status==CommonConstants.PurchaseFlowStatus.WAIT).OrderBy(f=>f.Sequence).FirstOrDefault();
            if (currentPurchaseFlow == null || currentPurchaseFlow.VerifyUserId != verifier.UserId)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "未輪到你的審核流程,不可修改數量或刪除採購品項"
                });
            }
            PurchaseMainSheet? purchaseMainSheet = _purchaseService.GetPurchaseMainByMainId(request.PurchaseMainId);
            List<PurchaseSubItem> existingSubItemList = _purchaseService.GetPurchaseSubItemsByMainId(request.PurchaseMainId);
            if (purchaseMainSheet==null|| purchaseMainSheet.IsActive==false||existingSubItemList.Count == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該採購單未存在"
                });
            }

            var result = _purchaseService.UpdateOrDeleteSubItems(request,purchaseMainSheet,existingSubItemList,currentPurchaseFlow,verifier,compId);

            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }

        [HttpPost("flow/ownerUpdateOrDeleteItem")]
        [Authorize]
        public IActionResult OwnerUpdateOrDeleteSubItemWhenFlow(UpdateOrDeleteSubItemInFlowRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var verifier = memberAndPermissionSetting.Member;
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            PurchaseMainSheet? purchaseMainSheet = _purchaseService.GetPurchaseMainByMainId(request.PurchaseMainId);
            List<PurchaseSubItem> existingSubItemList = _purchaseService.GetPurchaseSubItemsByMainId(request.PurchaseMainId);
            if (purchaseMainSheet==null|| purchaseMainSheet.IsActive==false||existingSubItemList.Count == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該採購單未存在"
                });
            }

            var result = _purchaseService.OwnerUpdateOrDeleteSubItems(request,purchaseMainSheet,existingSubItemList);

            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }

        [HttpGet("flows/my")]
        [Authorize]
        public IActionResult GetFlowsSignedByMy()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;


            var purchaseFlowsSignedByMe = _purchaseService.GetFlowsByUserId(memberAndPermissionSetting.Member.UserId);


            var distinctMainIdList = purchaseFlowsSignedByMe.Select(f => f.PurchaseMainId).Distinct().ToList();
            var purchaseMainList = _purchaseService.GetPurchaseMainsByMainIdList(distinctMainIdList).OrderByDescending(m => m.UpdatedAt).ToList();
            var purchaseSubItems = _purchaseService.GetPurchaseSubItemsByMainIdList(distinctMainIdList);
            var purchaseFlows = _purchaseService.GetPurchaseFlowsByMainIdList(distinctMainIdList);
            var purchaseFlowLogs = _purchaseService.GetPurchaseFlowLogsByMainIdList(distinctMainIdList);

            var distinctProductIdList = purchaseSubItems.Select(s => s.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);

            List<PurchaseMainAndSubItemVo> purchaseMainAndSubItemVoList = new List<PurchaseMainAndSubItemVo>();

            purchaseMainList.ForEach(m =>
            {
                var purchaseMainAndSubItemVo = _mapper.Map<PurchaseMainAndSubItemVo>(m);

                var matchedSubItems = purchaseSubItems.Where(s => s.PurchaseMainId == m.PurchaseMainId).OrderBy(s => s.UpdatedAt).ToList();
                var items = _mapper.Map<List<PurchaseSubItemVo>>(matchedSubItems);
                var matchedFlows = purchaseFlows.Where(f => f.PurchaseMainId == m.PurchaseMainId).OrderBy(f => f.Sequence).ToList();
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

        [HttpPost("updateItemSupplier")]
        [Authorize]
        public IActionResult ChangeItemsSupplier(UpdatePurchaseItemSupplierRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var subItemIdList = request.UpdateItems.Select(i=>i.ItemId).ToList();

            var subItems = _purchaseService.GetPurchaseSubItemListByItemList(subItemIdList);
            var distinctMainIds = subItems.Select(s => s.PurchaseMainId).Distinct().ToList();
            if (distinctMainIds.Count > 1)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "只能更新同一個主採購單內的品項,不可跨採購單更新"
                });
            }
            var purchaseMain = _purchaseService.GetPurchaseMainByMainId(distinctMainIds[0]);
            if (purchaseMain == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "沒有對應的採購單"
                });
            }
            var suppliers = _supplierService.GetSuppliersByIdList(request.UpdateItems.Select(i=>i.ArrangeSupplierId).ToList());
            foreach(var  item in request.UpdateItems){
                if (!suppliers.Select(s => s.Id).ToList().Contains(item.ArrangeSupplierId))
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"此{item.ArrangeSupplierId}不存在"
                    }); 
                }
            }

            var productIdList = subItems.Select(i => i.ProductId).ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIdList, purchaseMain.CompId);

            var result = _purchaseService.UpdateItemsSupplier(request, subItems, products, suppliers);

            var response = new CommonResponse<dynamic>
            {
                Result = result,
            };
            return Ok(response);
        }

        [HttpPost("products/notEnoughQuantity")]
        [Authorize]
        public IActionResult ListNotEnoughProducts(ListNotEnoughProductsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;
            var data = _warehouseProductService.ListNotEnoughProducts(request);
            var response = new CommonResponse<dynamic>
            {
                Result = true,
                Data = data,
            };
            return Ok(response);

        }

        [HttpPost("owner/updateOwnerProcess")]
        [Authorize]
        public IActionResult UpdateOwnerProcess(UpdateOwnerProcessRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }


            var validationResult = _updateOwnerProcessRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            PurchaseMainSheet purchaseMainSheet = _purchaseService.GetPurchaseMainByMainId(request.PurchaseMainId);
            if (purchaseMainSheet == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "採購單不存在"
                });
            }
            List<PurchaseSubItem> allPurchaseSubItems = _purchaseService.GetPurchaseSubItemsByMainId(request.PurchaseMainId);

            _purchaseService.UpdatePurchaseOwnerProcess(purchaseMainSheet, allPurchaseSubItems, request);

            var response = new CommonResponse<dynamic>
            {
                Result = true,
            };
            return Ok(response);
        }

        [HttpPost("owner/updateOwnerComment")]
        [Authorize]
        public IActionResult UpdateOwnerComment(UpdateOwnerCommentRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            PurchaseMainSheet purchaseMainSheet = _purchaseService.GetPurchaseMainByMainId(request.PurchaseMainId);
            if (purchaseMainSheet == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "採購單不存在"
                });
            }
           

            _purchaseService.UpdatePurchaseOwnerComment(purchaseMainSheet, request);

            var response = new CommonResponse<dynamic>
            {
                Result = true,
            };
            return Ok(response);
        }
    } 
}
