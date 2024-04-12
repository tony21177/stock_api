﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Transactions;

namespace stock_api.Service
{
    public class PurchaseService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(StockDbContext dbContext, IMapper mapper, ILogger<PurchaseService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }


        public bool CreatePurchase(PurchaseMainSheet newPurchasePurchaseMainSheet,List<Models.PurchaseSubItem> purchaseSubItemList, List<PurchaseFlowSettingVo> purchaseFlowSettingList)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var purchaseMainId = Guid.NewGuid().ToString();
                    newPurchasePurchaseMainSheet.PurchaseMainId = purchaseMainId;
                    newPurchasePurchaseMainSheet.CurrentStatus = CommonConstants.PurchaseApplyStatus.APPLY;
                    newPurchasePurchaseMainSheet.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.NONE;
                    newPurchasePurchaseMainSheet.IsActive = true;
                    _dbContext.PurchaseMainSheets.Add(newPurchasePurchaseMainSheet);

                    foreach (var item in purchaseSubItemList)
                    {
                        item.ItemId = Guid.NewGuid().ToString();
                        item.PurchaseMainId = purchaseMainId;
                        item.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.NONE;
                    }
                    _dbContext.PurchaseSubItems.AddRange(purchaseSubItemList);


                    List<PurchaseFlow> purchaseFlows = new List<PurchaseFlow>();
                    DateTime submitedAt = DateTime.Now;
                    foreach (var item in purchaseFlowSettingList)
                    {
                        var purchaseFlow = new PurchaseFlow()
                        {
                            FlowId = Guid.NewGuid().ToString(),
                            CompId = newPurchasePurchaseMainSheet.CompId,
                            PurchaseMainId = purchaseMainId,
                            Status = CommonConstants.PurchaseFlowStatus.WAIT,
                            VerifyCompId = newPurchasePurchaseMainSheet.CompId,
                            VerifyUserId = item.UserId,
                            VerifyUserName = item.UserDisplayName,
                            Answer = CommonConstants.PurchaseFlowAnswer.EMPTY,
                            Sequence = item.Sequence,
                            SubmitAt = submitedAt,
                        };
                        purchaseFlows.Add(purchaseFlow);
                    }
                    _dbContext.PurchaseFlows.AddRange(purchaseFlows);
                    _dbContext.SaveChanges();
                    scope.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError("事務失敗[CreatePurchaseRequest]：{msg}", ex);
                    return false;
                }
            }

        }

        public List<PurchaseMainAndSubItemVo> ListPurchase(ListPurchaseRequest listPurchaseRequest)
        {
            IQueryable<PurchaseItemListView> query = _dbContext.PurchaseItemListViews;
            if (listPurchaseRequest.CompId != null)
            {
                query = query.Where(h => h.CompId == listPurchaseRequest.CompId);
            }
            if (listPurchaseRequest.StartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(listPurchaseRequest.StartDate);
                query = query.Where(h => h.UpdatedAt>= startDateTime);
            }
            if (listPurchaseRequest.EndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(listPurchaseRequest.EndDate).Value.AddDays(1);
                query = query.Where(h => h.UpdatedAt < endDateTime);
            }
            if (listPurchaseRequest.GroupId != null)
            {
                query = query.Where(h => h.GroupIds.Contains(listPurchaseRequest.GroupId));
            }
            if (listPurchaseRequest.Type != null)
            {
                query = query.Where(h => h.Type==listPurchaseRequest.Type);
            }
            if (listPurchaseRequest.CurrentStatus != null)
            {
                query = query.Where(h => h.CurrentStatus == listPurchaseRequest.CurrentStatus);
            }
            if (listPurchaseRequest.ReceiveStatus != null)
            {
                query = query.Where(h => h.ReceiveStatus == listPurchaseRequest.ReceiveStatus);
            }
            if (listPurchaseRequest.IsActive != null)
            {
                query = query.Where(h => h.IsActive == listPurchaseRequest.IsActive);
            }

            var result = query.ToList();
            Dictionary<string,List<PurchaseItemListView>> mainSheetIdMap = new Dictionary<string,List<PurchaseItemListView>>();

            foreach (var item in result)
            {
                if (!mainSheetIdMap.ContainsKey(item.PurchaseMainId))
                {
                    mainSheetIdMap.Add(item.PurchaseMainId,new List<PurchaseItemListView>());
                }
                var voList = mainSheetIdMap.GetValueOrDefault(item.PurchaseMainId);
                if (voList != null)
                {
                    voList.Add(item);
                }
            }

            List<PurchaseMainAndSubItemVo> purchaseMainAndSubItemVoList = new List<PurchaseMainAndSubItemVo> { };
            foreach (var kvp in mainSheetIdMap)
            {
                List<PurchaseSubItemVo> Items = new List<PurchaseSubItemVo>();
                kvp.Value.ForEach(vo =>
                {
                    var subItem = new PurchaseSubItemVo()
                    {
                        ItemId = vo.ItemId,
                        Comment = vo.Comment,
                        CompId = vo.CompId,
                        ProductCategory = vo.ProductCategory,
                        ProductName = vo.ProductName,
                        ProductId = vo.ProductId,
                        ProductSpec = vo.ProductSpec,
                        PurchaseMainId = vo.PurchaseMainId,
                        Quantity = vo.Quantity,
                        ReceiveQuantity = vo.ReceiveQuantity,
                        ReceiveStatus = vo.ItemReceiveStatus,
                        GroupIds = vo.GroupIds.Split(',').ToList(),
                        GroupNames = vo.ItemGroupNames.Split(",").ToList(),
                        ArrangeSupplierId = vo.ArrangeSupplierId,
                        ArrangeSupplierName = vo.ArrangeSupplierName,
                        CurrentInStockQuantity = vo.CurrentInStockQuantity,
                        CreatedAt = vo.CreatedAt.Value,
                        UpdatedAt = vo.UpdatedAt.Value,
                    };
                    Items.Add(subItem);
                });


                var vo = new PurchaseMainAndSubItemVo
                {
                    PurchaseMainId = kvp.Key,
                    ApplyDate = kvp.Value[0].ApplyDate,
                    CompId = kvp.Value[0].CompId,
                    CurrentStatus = kvp.Value[0].CurrentStatus,
                    DemandDate = kvp.Value[0].DemandDate,
                    GroupIds = kvp.Value[0].GroupIds.Split(",", StringSplitOptions.None).ToList(),
                    Remarks = kvp.Value[0].Remarks,
                    UserId = kvp.Value[0].UserId,
                    ReceiveStatus = kvp.Value[0].ReceiveStatus,
                    Type = kvp.Value[0].Type,
                    CreatedAt = kvp.Value[0].CreatedAt,
                    UpdatedAt = kvp.Value[0].UpdatedAt,
                    IsActive = kvp.Value[0].IsActive,
                    Items = Items,
                };
                purchaseMainAndSubItemVoList.Add(vo);
            }

            if (listPurchaseRequest.IsNeedFlow == true)
            {
                var differentMainSheetId = purchaseMainAndSubItemVoList.Select(m=>m.PurchaseMainId).Distinct().ToList();
                var flows = GetFlowsByPurchaseMainIds(differentMainSheetId).OrderBy(f=>f.Sequence);
                foreach (var item in purchaseMainAndSubItemVoList)
                {
                    var matchedFlows = flows.Where(f => f.PurchaseMainId == item.PurchaseMainId).ToList();
                    item.flows= matchedFlows;
                }
            }

            return purchaseMainAndSubItemVoList;
        }

        public List<PurchaseFlow> GetFlowsByPurchaseMainIds(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseFlows.Where(f=>purchaseMainIdList.Contains(f.PurchaseMainId)).ToList();

        }
    }

    
}