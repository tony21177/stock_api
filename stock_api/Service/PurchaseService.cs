using AutoMapper;
using Microsoft.EntityFrameworkCore;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using System.Text.Json;
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

        public PurchaseMainSheet? GetPurchaseMainByMainId(string purchaseMainId)
        {
            return _dbContext.PurchaseMainSheets.Where(m => m.PurchaseMainId == purchaseMainId).FirstOrDefault();
        }

        public List<PurchaseMainSheet> GetPurchaseMainsByMainIdList(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseMainSheets.Where(m => purchaseMainIdList.Contains(m.PurchaseMainId)).ToList();
        }


        public List<PurchaseSubItem> GetPurchaseSubItemsByMainId(string purchaseMainId)
        {
            return _dbContext.PurchaseSubItems.Where(s => s.PurchaseMainId == purchaseMainId).ToList();
        }
        public PurchaseSubItem? GetPurchaseSubItemByItemId(string itemId)
        {
            return _dbContext.PurchaseSubItems.Where(s => s.ItemId == itemId).FirstOrDefault();
        }

        public List<PurchaseSubItem> GetPurchaseSubItemsByMainIdList(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseSubItems.Where(s => purchaseMainIdList.Contains(s.PurchaseMainId)).ToList();
        }

        public List<PurchaseFlow> GetPurchaseFlowsByMainId(string purchaseMainId)
        {
            return _dbContext.PurchaseFlows.Where(f => f.PurchaseMainId == purchaseMainId).ToList();
        }
        public List<PurchaseFlow> GetPurchaseFlowsByMainIdList(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseFlows.Where(f => purchaseMainIdList.Contains(f.PurchaseMainId)).ToList();
        }

        public List<PurchaseFlowLog> GetPurchaseFlowLogsByMainId(string purchaseMainId)
        {
            return _dbContext.PurchaseFlowLogs.Where(l => l.PurchaseMainId == purchaseMainId).ToList();
        }

        public List<PurchaseFlowLog> GetPurchaseFlowLogsByMainIdList(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseFlowLogs.Where(l => purchaseMainIdList.Contains(l.PurchaseMainId)).ToList();
        }

        public bool CreatePurchase(PurchaseMainSheet newPurchasePurchaseMainSheet, List<PurchaseSubItem> newPurchaseSubItemList, List<PurchaseFlowSettingVo> purchaseFlowSettingList,bool isOwnerCreate)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var distinctItemSupplierListForNew = newPurchaseSubItemList.Select(i => i.ArrangeSupplierId).Distinct().ToList();

                    var purchaseMainId = Guid.NewGuid().ToString();
                    newPurchasePurchaseMainSheet.PurchaseMainId = purchaseMainId;
                    newPurchasePurchaseMainSheet.CurrentStatus = CommonConstants.PurchaseApplyStatus.APPLY;
                    newPurchasePurchaseMainSheet.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.NONE;
                    newPurchasePurchaseMainSheet.IsActive = true;
                    // 表示OWNER拆單後的新單供應商只有一家就不能再拆單了
                    if (distinctItemSupplierListForNew.Count == 1 && isOwnerCreate == true)
                    {
                        newPurchasePurchaseMainSheet.SplitPrcoess = CommonConstants.SplitProcess.DONE;
                    }
                    _dbContext.PurchaseMainSheets.Add(newPurchasePurchaseMainSheet);


                    foreach (var item in newPurchaseSubItemList)
                    {
                        item.ItemId = Guid.NewGuid().ToString();
                        item.PurchaseMainId = purchaseMainId;
                        item.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.NONE;
                        // 表示OWNER拆單後的新單供應商只有一家就不能再拆單了
                        if (distinctItemSupplierListForNew.Count == 1 && isOwnerCreate == true)
                        {
                            item.SplitProcess = CommonConstants.SplitProcess.DONE;
                        }
                    }
                    _dbContext.PurchaseSubItems.AddRange(newPurchaseSubItemList);


                    List<PurchaseFlow> purchaseFlows = new();
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

                    Dictionary<string,List<PurchaseSubItem>> mainIdAndPurchaseSubItmeListMapForWith = new ();
                    Dictionary<string,PurchaseMainSheet> mainIdAndPurchaseMainMapForWith = new ();



                    foreach (var item in newPurchaseSubItemList)
                    {
                        if (item.WithItemId != null&&item.WithPurchaseMainId!=null)
                        {
                            var withPurchaseMain = GetPurchaseMainByMainId(item.WithPurchaseMainId); // 表示新的採購單是從這張主單拆單過來的
                            var withSubItems = GetPurchaseSubItemsByMainId(item.WithPurchaseMainId); // 表示新的採購單item是從主單的這些item拆過來的
                            foreach (var subItem in withSubItems)
                            {
                                if (subItem.ItemId == item.WithItemId)
                                {
                                    subItem.SplitProcess = CommonConstants.SplitProcess.DONE;
                                }
                                // 表示OWNER拆單後的供應商只有一家就不能再拆單了
                                //if (distinctItemSupplierList.Count == 1 && isOwnerCreate == true)
                                //{
                                //    subItem.SplitProcess = CommonConstants.SplitProcess.DONE;
                                //}
                            }
                            if (!mainIdAndPurchaseSubItmeListMapForWith.ContainsKey(withPurchaseMain.PurchaseMainId))
                            {
                                mainIdAndPurchaseSubItmeListMapForWith[withPurchaseMain.PurchaseMainId] = withSubItems;
                            }
                            if (!mainIdAndPurchaseMainMapForWith.ContainsKey(withPurchaseMain.PurchaseMainId))
                            {
                                mainIdAndPurchaseMainMapForWith[withPurchaseMain.PurchaseMainId] = withPurchaseMain;
                            }
                            
                        }
                    }
                    foreach (var (mainId, subItemList) in mainIdAndPurchaseSubItmeListMapForWith)
                    {
                        if(subItemList.All(item=>item.SplitProcess== CommonConstants.SplitProcess.DONE))
                        {
                            mainIdAndPurchaseMainMapForWith[mainId].SplitPrcoess = CommonConstants.SplitProcess.DONE;
                        }else if (subItemList.Any(item => item.SplitProcess == CommonConstants.SplitProcess.DONE))
                        {
                            mainIdAndPurchaseMainMapForWith[mainId].SplitPrcoess = CommonConstants.SplitProcess.PART;
                        }
                    }
                    

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
            //if (listPurchaseRequest.StartDate != null)
            //{
            //    var startDateTime = DateTimeHelper.ParseDateString(listPurchaseRequest.StartDate);
            //    query = query.Where(h => h.UpdatedAt >= startDateTime);
            //}
            //if (listPurchaseRequest.EndDate != null)
            //{
            //    var endDateTime = DateTimeHelper.ParseDateString(listPurchaseRequest.EndDate).Value.AddDays(1);
            //    query = query.Where(h => h.UpdatedAt < endDateTime);
            //}
            if (listPurchaseRequest.StartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(listPurchaseRequest.StartDate);
                query = query.Where(h => h.UpdatedAt >= startDateTime);
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
                query = query.Where(h => h.Type == listPurchaseRequest.Type);
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
            Dictionary<string, List<PurchaseItemListView>> mainSheetIdMap = new Dictionary<string, List<PurchaseItemListView>>();

            foreach (var item in result)
            {
                if (!mainSheetIdMap.ContainsKey(item.PurchaseMainId))
                {
                    mainSheetIdMap.Add(item.PurchaseMainId, new List<PurchaseItemListView>());
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
                        SplitProcess = vo.SubSplitProcess,
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
                    SplitProcess = kvp.Value[0].MainSplitPrcoess,
                    OwnerProcess = kvp.Value[0].OwnerProcess,   
                    Items = Items,
                };
                purchaseMainAndSubItemVoList.Add(vo);
            }

            if (listPurchaseRequest.IsNeedFlow == true)
            {
                var differentMainSheetId = purchaseMainAndSubItemVoList.Select(m => m.PurchaseMainId).Distinct().ToList();
                var flows = GetFlowsByPurchaseMainIds(differentMainSheetId).OrderBy(f => f.Sequence);
                foreach (var item in purchaseMainAndSubItemVoList)
                {
                    var matchedFlows = flows.Where(f => f.PurchaseMainId == item.PurchaseMainId).ToList();
                    item.flows = matchedFlows;
                }
            }

            return purchaseMainAndSubItemVoList;
        }

        public List<PurchaseFlow> GetFlowsByPurchaseMainIds(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseFlows.Where(f => purchaseMainIdList.Contains(f.PurchaseMainId)).ToList();

        }

        public PurchaseFlow? GetFlowsByPurchaseMainId(string purchaseMainId)
        {
            return _dbContext.PurchaseFlows.Where(f => f.PurchaseMainId == purchaseMainId).FirstOrDefault();

        }

        public PurchaseFlow? GetFlowsByFlowId(string flowId)
        {
            return _dbContext.PurchaseFlows.Where(f => f.FlowId == flowId).FirstOrDefault();

        }

        public List<PurchaseFlow> GetFlowsByUserId(string userId)
        {
            return _dbContext.PurchaseFlows.Where(f => f.VerifyUserId == userId).ToList();

        }

        public void UpdatePurchaseOwnerProcess(PurchaseMainSheet main,String status)
        {
            main.OwnerProcess = status;
            _dbContext.SaveChanges();
        }

        public void PurchaseFlowRead(PurchaseFlow flow)
        {
            flow.ReadAt = DateTime.Now;
            _dbContext.SaveChanges();
        }

        public List<PurchaseFlow> GetBeforeFlows(PurchaseFlow nowFlow)
        {
            return _dbContext.PurchaseFlows.Where(f=>f.Sequence<nowFlow.Sequence&&f.CompId==nowFlow.CompId&&f.PurchaseMainId==nowFlow.PurchaseMainId).OrderBy(f=>f.Sequence).ToList();
        }

        public bool AnswerFlow(PurchaseFlow flow, MemberAndPermissionSetting verifierMemberAndPermission, string answer, string? reason)
        {
            string purchaseMainId = flow.PurchaseMainId;
            PurchaseMainSheet purchaseMain = GetPurchaseMainByMainId(purchaseMainId);
            List<PurchaseSubItem> purchaseSubItems = GetPurchaseSubItemsByMainId(purchaseMainId);
            var (preFlow, nextFlow) = FindPreviousAndNextFlow(flow);
            return AnswerFlowInTransactionScope(preFlow, nextFlow, flow, purchaseMain, purchaseSubItems, verifierMemberAndPermission, answer, reason);
        }

        public (PurchaseFlow?, PurchaseFlow?) FindPreviousAndNextFlow(PurchaseFlow flow)
        {
            List<PurchaseFlow> purchaseFlows = _dbContext.PurchaseFlows.Where(f => f.PurchaseMainId == flow.PurchaseMainId).OrderBy(f => f.Sequence).ToList();

            return (purchaseFlows.FirstOrDefault(f => f.Sequence < flow.Sequence), purchaseFlows.FirstOrDefault(f => f.Sequence > flow.Sequence));
        }

        private bool AnswerFlowInTransactionScope(PurchaseFlow? preFlow, PurchaseFlow? nextPurchase, PurchaseFlow currentFlow, PurchaseMainSheet purchaseMain,List<PurchaseSubItem> purchaseSubItems, MemberAndPermissionSetting verifierMemberAndPermission, string answer, string? reason)
        {
            WarehouseMember verifyMember = verifierMemberAndPermission.Member;
            var verifyCompId = verifierMemberAndPermission.CompanyWithUnit.CompId;
            using var scope = new TransactionScope();
            try
            {
                // 更新Flow
                currentFlow.Reason = reason;
                currentFlow.VerifyCompId = verifyCompId;
                currentFlow.VerifyUserId = verifyMember.UserId;
                currentFlow.VerifyUserName = verifyMember.DisplayName;
                currentFlow.Answer = answer;
                currentFlow.SubmitAt = DateTime.Now;

                // 更新主單狀態
                if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase == null)
                {
                    currentFlow.Status = answer;
                    // 已完成所有flow 更新主單狀態
                    purchaseMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.AGREE;
                    List<AcceptanceItem> acceptanceItems = new();
                    foreach (var item in purchaseSubItems)
                    {
                        var acceptanceItem = new AcceptanceItem()
                        {
                            AcceptId = Guid.NewGuid().ToString(),
                            CompId = item.CompId,
                            ItemId = item.ItemId,
                            OrderQuantity = item.Quantity??0,
                            ProductId = item.ProductId,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            ProductSpec = item.ProductSpec,
                            UdiserialCode = item.UdiserialCode,
                            PurchaseMainId = purchaseMain.PurchaseMainId,
                            ArrangeSupplierId = item.ArrangeSupplierId,
                            ArrangeSupplierName = item.ArrangeSupplierName,
                        };
                        acceptanceItems.Add(acceptanceItem);
                    }
                    _dbContext.AcceptanceItems.AddRange(acceptanceItems);

                }
                if (answer == CommonConstants.AnswerPurchaseFlow.REJECT)
                {
                    currentFlow.Status = answer;
                    purchaseMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.REJECT;
                }
                if (answer == CommonConstants.AnswerPurchaseFlow.BACK)
                {
                    currentFlow.Status = answer;

                    currentFlow.Answer = "";
                    if (preFlow != null)
                    {
                        preFlow.Status = "";
                        preFlow.Answer = "";
                    }
                }

                // 新增log
                var newFlowLog = new PurchaseFlowLog()
                {
                    LogId = Guid.NewGuid().ToString(),
                    CompId = currentFlow.CompId,
                    PurchaseMainId = currentFlow.PurchaseMainId,
                    UserId = verifyMember.UserId,
                    UserName = verifyMember.DisplayName,
                    Sequence = currentFlow.Sequence,
                    Action = answer,
                    Remarks = reason
                };
                _dbContext.PurchaseFlowLogs.Add(newFlowLog);
                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[AnswerFlow]：{msg}", ex);
                return false;
            }
        }

        public List<PurchaseSubItem> GetPurchaseSubItemListByItemList(List<string> itemIdList)
        {
            return _dbContext.PurchaseSubItems.Where(s=>itemIdList.Contains(s.ItemId)).ToList();
        }


        public bool UpdateItemsSupplier(UpdatePurchaseItemSupplierRequest request, List<PurchaseSubItem> purchaseSubItems, List<WarehouseProduct> products,List<Supplier> suppliers)
        {
            using var scope = new TransactionScope();
            try
            {
                foreach (var item in request.UpdateItems)
                {
                    var matchedPurchaseSubItem = purchaseSubItems.Where(s => s.ItemId == item.ItemId).FirstOrDefault();

                    if (matchedPurchaseSubItem != null)
                    {
                        var matchedSupplier = suppliers.Where(s => s.Id==item.ArrangeSupplierId).First();
                        matchedPurchaseSubItem.ArrangeSupplierId = matchedSupplier.Id;
                        matchedPurchaseSubItem.ArrangeSupplierName = matchedSupplier.Name;
                        var matchedProduct = products.Where(p => p.ProductId == matchedPurchaseSubItem.ProductId && p.CompId == matchedPurchaseSubItem.CompId).FirstOrDefault();
                        if (matchedProduct != null)
                        {
                            matchedProduct.DefaultSupplierId = matchedSupplier.Id;
                            matchedProduct.DefaultSupplierName = matchedSupplier.Name;
                        }

                    }
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateItemsSupplier]：{msg}", ex);
                return false;
            }
        }

        public bool UpdateOrDeleteSubItems(UpdateOrDeleteSubItemInFlowRequest request,PurchaseMainSheet purchaseMainSheet,List<PurchaseSubItem> purchaseSubItemList,
            PurchaseFlow flow,WarehouseMember user,string compId)
        {
            using var scope = new TransactionScope();
            try
            {
                var beforeSubItemsJsonString = JsonSerializer.Serialize(purchaseSubItemList);
                request.UpdateSubItemList.ForEach(subItem =>
                {
                    var matchedUpdateItem = purchaseSubItemList.Where(i=>i.ItemId==subItem.ItemId).FirstOrDefault();
                    if (matchedUpdateItem != null)
                    {
                        matchedUpdateItem.Quantity = subItem.Quantity;
                    }
                });
                _dbContext.PurchaseSubItems.Where(subItem => request.DeleteSubItemIdList.Contains(subItem.ItemId)).ExecuteDelete();

                var modifiedSubItems = _dbContext.PurchaseSubItems.Where(i => i.PurchaseMainId == purchaseMainSheet.PurchaseMainId).ToList();
                var afterSubItemsJsonString = JsonSerializer.Serialize(modifiedSubItems);
                var newPurchaseFlowLog = new PurchaseFlowLog()
                {
                    LogId = Guid.NewGuid().ToString(),
                    CompId = compId,
                    PurchaseMainId = purchaseMainSheet.PurchaseMainId,
                    UserId = user.UserId,
                    UserName = user.DisplayName,
                    Sequence = flow.Sequence,
                    Action = CommonConstants.PurchaseFlowLogAction.MODIFY,
                    BeforeSubItems = beforeSubItemsJsonString,
                    AfterSubItems = afterSubItemsJsonString,
                };
                _dbContext.PurchaseFlowLogs.Add(newPurchaseFlowLog);
                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateOrDeleteSubItems]：{msg}", ex);
                return false;
            }
        }
    }
}
