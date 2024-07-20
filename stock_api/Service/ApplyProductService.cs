using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Settings;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using System.Text.Json;
using System.Transactions;

namespace stock_api.Service
{
    public class ApplyProductService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseService> _logger;
        private readonly EmailService _emailService;
        private readonly MemberService _memberService;
        private readonly SmtpSettings _smtpSettings;


        public ApplyProductService(StockDbContext dbContext, IMapper mapper, ILogger<PurchaseService> logger, EmailService emailService,MemberService memberService,SmtpSettings smtpSettings)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _memberService = memberService;
            _smtpSettings = smtpSettings;
        }

        public ApplyNewProductMain? GetApplyNewProductMainByMainId(string applyId)
        {
            return _dbContext.ApplyNewProductMains.Where(m => m.ApplyId == applyId).FirstOrDefault();
        }

        public List<ApplyNewProductMain> GetApplyNewProductMainsByApplyIdList(List<string> applyIdList)
        {
            return _dbContext.ApplyNewProductMains.Where(m => applyIdList.Contains(m.ApplyId)).ToList();
        }

        public List<ApplyNewProductFlow> GetFlowsByApplyId(string applyId)
        {
            return _dbContext.ApplyNewProductFlows.Where(f => f.ApplyId == applyId).ToList();
        }
        public List<ApplyNewProductFlow> GetFlowsByApplyIdList(List<string> applyIdList)
        {
            return _dbContext.ApplyNewProductFlows.Where(f => applyIdList.Contains(f.ApplyId)).ToList();
        }

        public List<ApplyProductFlowLog> GetFlowLogsByApplyId(string applyId)
        {
            return _dbContext.ApplyProductFlowLogs.Where(l => l.ApplyId == applyId).ToList();
        }

        public List<ApplyProductFlowLog> GetFlowLogsByApplyIdList(List<string> applyIdList)
        {
            return _dbContext.ApplyProductFlowLogs.Where(l => applyIdList.Contains(l.ApplyId)).ToList();
        }

        public (bool,string?) CreateApplyProductMain(ApplyNewProductMain newApplyNewProductMain,  List<ApplyProductFlowSettingVo> applyProductFlowSettingVoList)
        {
            using var scope = new TransactionScope();
            try
            {

                _dbContext.ApplyNewProductMains.Add(newApplyNewProductMain);


                List<ApplyNewProductFlow> flows = new();
                DateTime submitedAt = DateTime.Now;
                var matchedApplyProductFlowSettingVoList = applyProductFlowSettingVoList.Where(s => s.ReviewGroupId == newApplyNewProductMain.ProductGroupId).ToList();
                foreach (var setting in matchedApplyProductFlowSettingVoList)
                {
                    var flow = new ApplyNewProductFlow()
                    {
                        FlowId = Guid.NewGuid().ToString(),
                        ApplyId = newApplyNewProductMain.ApplyId,
                        Answer = CommonConstants.PurchaseFlowAnswer.EMPTY,
                        CompId = newApplyNewProductMain.CompId,
                        Status = CommonConstants.ApplyNewProductFlowStatus.WAIT,
                        SubmitAt = submitedAt,
                        ReviewCompId = setting.CompId,
                        ReviewUserId = setting.ReviewUserId,
                        ReviewUserName = setting.ReviewUserName,
                        ReviewGroupId = setting.ReviewGroupId,
                        ReviewGroupName = setting.ReviewGroupName,
                        Sequence = setting.Sequence,
                    };

                    flows.Add(flow);
                }
                _dbContext.ApplyNewProductFlows.AddRange(flows);

                var firstFlow = matchedApplyProductFlowSettingVoList.OrderBy(s => s.Sequence).FirstOrDefault();
                DateTime now = DateTime.Now;
                string title = $"申請新品項單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(now), firstFlow.SettingId.AsSpan(0, 5))} 需要您審核";
                string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{firstFlow.SettingId}>{firstFlow.SettingId}</a>";
                SendMailByFlowSetting(firstFlow, title, content);

                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[CreateApplyProductMain]：{msg}", ex);
                return (false, ex.Message);
            }

        }

        

        public (List<ApplyNewProductMainWithFlowVo>,int) ListApplyNewProductMain(ListApplyNewProductMainRequest listRequest)
        {
            IQueryable<ApplyNewProductMain> query = _dbContext.ApplyNewProductMains;
            if (listRequest.CompId != null)
            {
                query = query.Where(h => h.CompId == listRequest.CompId);
            }
            if (listRequest.ApplyId != null)
            {
                query = query.Where(h => h.ApplyId == listRequest.ApplyId);
            }
            if (listRequest.StartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(listRequest.StartDate);
                query = query.Where(h => h.CreatedAt >= startDateTime);
            }
            if (listRequest.EndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(listRequest.EndDate).Value.AddDays(1);
                query = query.Where(h => h.CreatedAt < endDateTime);
            }
            if (listRequest.ProductGroupId != null)
            {
                query = query.Where(h => h.ProductGroupId== listRequest.ProductGroupId);
            }
            
            if (listRequest.CurrentStatus != null)
            {
                query = query.Where(h => h.CurrentStatus == listRequest.CurrentStatus);
            }
            var result = query.ToList();


            if (!string.IsNullOrEmpty(listRequest.Keywords))
            {
                query = query.Where(h => 
                 h.ApplyReason!=null && h.ApplyReason.Contains(listRequest.Keywords)
                || h.ApplyReason != null && h.ApplyReason.Contains(listRequest.Keywords)
                || h.ApplyRemarks != null && h.ApplyRemarks.Contains(listRequest.Keywords)
                || h.ApplyProductName != null && h.ApplyProductName.Contains(listRequest.Keywords)
                || h.ApplyProductSpec != null && h.ApplyProductSpec.Contains(listRequest.Keywords)
                || h.ProductGroupId != null && h.ProductGroupId.Contains(listRequest.Keywords)
                || h.ProductGroupName != null && h.ProductGroupName.Contains(listRequest.Keywords)
                || h.ApplyId != null && h.ApplyId.Contains(listRequest.Keywords)
                );
            }
            if (listRequest.PaginationCondition.OrderByField == null) listRequest.PaginationCondition.OrderByField = "CreatedAt";
            if (listRequest.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(listRequest.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "ApplyProductName" => query.OrderByDescending(h => h.ApplyProductName),
                    "CurrentStatus" => query.OrderByDescending(h => h.CurrentStatus),
                    "ApplyQuantity" => query.OrderByDescending(h => h.ApplyQuantity),
                    "ProductGroupId" => query.OrderByDescending(h => h.ProductGroupId),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.CreatedAt),
                };
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(listRequest.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "ApplyProductName" => query.OrderBy(h => h.ApplyProductName),
                    "CurrentStatus" => query.OrderBy(h => h.CurrentStatus),
                    "ApplyQuantity" => query.OrderBy(h => h.ApplyQuantity),
                    "ProductGroupId" => query.OrderBy(h => h.ProductGroupId),
                    "CreatedAt" => query.OrderBy(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderBy(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.CreatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / listRequest.PaginationCondition.PageSize);
            query = query.Skip((listRequest.PaginationCondition.Page - 1) * listRequest.PaginationCondition.PageSize).Take(listRequest.PaginationCondition.PageSize);
            var applyNewProductMainList = query.ToList();
            var applyNewProductMainWithFlowList = _mapper.Map<List<ApplyNewProductMainWithFlowVo>>(applyNewProductMainList);

            var allApplyId = applyNewProductMainList.Select(m=>m.ApplyId).Distinct().ToList();

            var allRelatedFlows =  _dbContext.ApplyNewProductFlows.Where(f=>allApplyId.Contains(f.ApplyId)).ToList();

            applyNewProductMainWithFlowList.ForEach(main =>
            {
                var matchedFlows = allRelatedFlows.Where(f => f.ApplyId == main.ApplyId).OrderByDescending(f => f.Sequence).ToList();
                main.Flows = matchedFlows;
            });

            return (applyNewProductMainWithFlowList,totalPages);
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

        public bool UpdatePurchaseOwnerProcess(PurchaseMainSheet main,List<PurchaseSubItem> allPurchaseSubItems ,UpdateOwnerProcessRequest request)
        {
            using var scope = new TransactionScope();
            try
            {
                List<PurchaseSubItem> toUpdateSubItems = new();
                if (request.ItemIds != null)
                {
                    toUpdateSubItems = allPurchaseSubItems.Where(s => request.ItemIds.Contains(s.ItemId)).ToList();
                }
                toUpdateSubItems.ForEach(s => { s.OwnerProcess = request.OwnerProcess;s.SplitProcess = CommonConstants.SplitProcess.DONE; });
                if (allPurchaseSubItems.All(s => s.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.AGREE))
                {
                    main.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.AGREE;
                    main.SplitPrcoess = CommonConstants.SplitProcess.DONE;

                    // 為什麼做這檢查是因為有可能金萬林忘了按同意 就自行安排出貨,然後單位也接著驗收入庫,main.ReceiveStatus就更改不等於NONE了
                    // 這時單純金萬霖再補按同意就只更新OwnerProcess,SplitPrcoess
                    if (main.ReceiveStatus== CommonConstants.PurchaseReceiveStatus.NONE)
                    {
                        main.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.DELIVERED;
                    }

                }
                else if (allPurchaseSubItems.Any(s => s.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.AGREE))
                {
                    main.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.PART_AGREE;
                    main.SplitPrcoess = CommonConstants.SplitProcess.PART;
                    if (main.ReceiveStatus == CommonConstants.PurchaseReceiveStatus.NONE)
                    {
                        main.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.DELIVERED;
                    }
                }else if(allPurchaseSubItems.All(s => s.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE))
                {
                    main.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE;
                    main.SplitPrcoess = CommonConstants.SplitProcess.DONE;
                    main.CurrentStatus = CommonConstants.PurchaseApplyStatus.CLOSE;
                }

                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdatePurchaseOwnerProcess]：{msg}", ex);
                return false;
            }
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

        //public bool AnswerFlow(PurchaseFlow flow, MemberAndPermissionSetting verifierMemberAndPermission, string answer, string? reason,bool? isOwner)
        //{
        //    string purchaseMainId = flow.PurchaseMainId;
        //    PurchaseMainSheet purchaseMain = GetPurchaseMainByMainId(purchaseMainId);
        //    List<PurchaseSubItem> purchaseSubItems = GetPurchaseSubItemsByMainId(purchaseMainId);
        //    var (preFlow, nextFlow) = FindPreviousAndNextFlow(flow);
        //    return AnswerFlowInTransactionScope(preFlow, nextFlow, flow, purchaseMain, purchaseSubItems, verifierMemberAndPermission, answer, reason,isOwner);
        //}

        public (PurchaseFlow?, PurchaseFlow?) FindPreviousAndNextFlow(PurchaseFlow flow)
        {
            List<PurchaseFlow> purchaseFlows = _dbContext.PurchaseFlows.Where(f => f.PurchaseMainId == flow.PurchaseMainId).OrderBy(f => f.Sequence).ToList();

            return (purchaseFlows.FirstOrDefault(f => f.Sequence < flow.Sequence), purchaseFlows.FirstOrDefault(f => f.Sequence > flow.Sequence));
        }

        private bool AnswerFlowInTransactionScope(PurchaseFlow? preFlow, PurchaseFlow? nextPurchase, PurchaseFlow currentFlow, PurchaseMainSheet purchaseMain,List<PurchaseSubItem> purchaseSubItems, MemberAndPermissionSetting verifierMemberAndPermission, string answer, string? reason,bool? isOwner)
        {
            WarehouseMember verifyMember = verifierMemberAndPermission.Member;
            var verifyCompId = verifierMemberAndPermission.CompanyWithUnit.CompId;
            using var scope = new TransactionScope();
            try
            {
                // 更新Flow
                currentFlow.Reason = reason;
                if (answer!= CommonConstants.PurchaseApplyStatus.BACK)
                {
                    currentFlow.VerifyCompId = verifyCompId;
                    currentFlow.VerifyUserId = verifyMember.UserId;
                    currentFlow.VerifyUserName = verifyMember.DisplayName;
                }
                
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
                if (answer == CommonConstants.AnswerPurchaseFlow.BACK&&isOwner!=true)
                {
                    currentFlow.Status = answer;

                    currentFlow.Answer = "";
                    if (preFlow != null)
                    {
                        preFlow.Status = "";
                        preFlow.Answer = "";
                    }
                    purchaseMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.BACK;
                }
                if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner == true)
                {
                    currentFlow.Status = "";
                    currentFlow.Answer = "";
                    purchaseMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.BACK;
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
                if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase == null)
                {
                    string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 需要您處理";
                    string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                    SendMailToOwner(title, content);
                }
                if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase != null)
                {
                    string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 需要您審核";
                    string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                    SendMailByFlow(nextPurchase,title, content);
                }
                if (answer == CommonConstants.AnswerPurchaseFlow.REJECT)
                {
                    string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 已被拒絕";
                    string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                    SendMailByPurchaseMain(purchaseMain, title, content);
                }
                if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner != true)
                {
                    if (preFlow != null)
                    {
                        string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 需要您審核";
                        string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                        SendMailByFlow(preFlow, title, content);
                    }
                    else
                    {
                        string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 已被退回";
                        string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                        SendMailByPurchaseMain(purchaseMain, title, content);
                    }
                }
                if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner == true)
                {
                    string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 已被退回";
                    string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                    SendMailByFlow(currentFlow, title, content);
                }

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
                _dbContext.AcceptanceItems.Where(acceptItem => request.DeleteSubItemIdList.Contains(acceptItem.ItemId)).ExecuteDelete();

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

        public List<PurchaseSubItem> GetUndonePurchaseSubItems(string compId,string productId)
        {
            return _dbContext.PurchaseSubItems.Where(s=>s.ReceiveStatus!=CommonConstants.PurchaseSubItemReceiveStatus.DONE&& s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE
            &&s.CompId == compId&&s.ProductId==productId).ToList();
        }

        public List<PurchaseSubItem> GetUndonePurchaseSubItems(List<string> productIdList)
        {
            return _dbContext.PurchaseSubItems.Where(s => s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE && s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE
            && productIdList.Contains(s.ProductId)).ToList();
        }

        public float GetInProcessingOrderQuantity(string productId)
        {
            var unDoneProcessingSubItem = _dbContext.PurchaseSubItems.Where(s => s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE
            && s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE
            && s.ProductId == productId).ToList();
            return unDoneProcessingSubItem.Select(s => s.Quantity ?? 0.0f).DefaultIfEmpty(0.0f).Sum();
        }

        private async Task SendMailByFlowSetting(ApplyProductFlowSetting applyProductFlowSetting, String title, String content)
        {
            var receiver = _memberService.GetMembersByUserId(applyProductFlowSetting.ReviewUserId);
            if (receiver != null)
            {

                if (!string.IsNullOrEmpty(receiver.Email))
                    await _emailService.SendAsync(title, content, receiver.Email);
            }
        }

        private async Task SendMailByFlow(PurchaseFlow flow, String title, String content)
        {
            var receiver = _memberService.GetMembersByUserId(flow.VerifyUserId);
            if (receiver != null)
            {

                if (!string.IsNullOrEmpty(receiver.Email))
                    await _emailService.SendAsync(title, content, receiver.Email);
            }
        }

        private async Task SendMailByPurchaseMain(PurchaseMainSheet main, String title, String content)
        {
            var receiver = _memberService.GetMembersByUserId(main.UserId);
            if (receiver != null)
            {

                if (!string.IsNullOrEmpty(receiver.Email))
                    await _emailService.SendAsync(title, content, receiver.Email);
            }
        }

        private void SendMailToOwner(String title, String content)
        {
            List<WarehouseMember> receiver = _memberService.GetOwnerMembers();
            receiver.ForEach(async r =>
            {
                if (!string.IsNullOrEmpty(r.Email))
                {
                    await _emailService.SendAsync(title, content, r.Email);
                }
            });

        }
    }
}
