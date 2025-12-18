using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI.Relational;
using Serilog;
using stock_api.Common.Constant;
using stock_api.Common.Settings;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Transactions;

namespace stock_api.Service
{
    public class PurchaseService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseService> _logger;
        private readonly EmailService _emailService;
        private readonly MemberService _memberService;
        private readonly SmtpSettings _smtpSettings;
        


        public PurchaseService(StockDbContext dbContext, IMapper mapper, ILogger<PurchaseService> logger, EmailService emailService,MemberService memberService,SmtpSettings smtpSettings)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _memberService = memberService;
            _smtpSettings = smtpSettings;
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

        public List<PurchaseSubItem> GetPurchaseSubItemByItemIdList(List<string> itemIdList)
        {
            return _dbContext.PurchaseSubItems.Where(s => itemIdList.Contains(s.ItemId)).ToList() ;
        }

        

        public List<PurchaseSubItem> GetPurchaseSubItemsByMainIdList(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseSubItems.Where(s => purchaseMainIdList.Contains(s.PurchaseMainId)).ToList();
        }

        public List<PurchaseFlow> GetPurchaseFlowsByMainId(string purchaseMainId)
        {
            return _dbContext.PurchaseFlows.Where(f => f.PurchaseMainId == purchaseMainId).ToList();
        }

        public List<PurchaseFlowWithAgentsVo> GetPurchaseFlowWithAgentsByMainId(string purchaseMainId)
        {
            var result = from p in _dbContext.PurchaseFlows
                         join m in _dbContext.WarehouseMembers on p.VerifyUserId equals m.UserId
                         where p.PurchaseMainId == purchaseMainId
                         select new PurchaseFlowWithAgentsVo
                         {
                             FlowId = p.FlowId,
                             CompId = p.CompId,
                             PurchaseMainId =  p.PurchaseMainId,
                             Reason = p.Reason,
                             Status = p.Status,
                             VerifyCompId = p.VerifyCompId,
                             VerifyUserId = p.VerifyUserId,
                             VerifyUserName = p.VerifyUserName,
                             Answer = p.Answer,
                             Sequence = p.Sequence,
                             ReadAt = p.ReadAt,
                             SubmitAt = p.SubmitAt,
                             CreatedAt = p.CreatedAt,
                             UpdatedAt = p.UpdatedAt,
                             Agents = m.Agents,
                             AgentNames = m.AgentNames,
                         };

            var list = result.ToList();
            list.ForEach(flow =>
            {
                if (!flow.Agents.IsNullOrEmpty())
                {
                    flow.VerifyAgentIds = flow.Agents.Split(",", StringSplitOptions.None).ToList();
                    flow.VerifyAgentNames = flow.AgentNames.Split(",", StringSplitOptions.None).ToList();
                }


            });
            return list;
        }

        public List<PurchaseFlowWithAgentsVo> GetPurchaseFlowWithAgentsByMainIdList(List<string> purchaseMainIdList)
        {
            var result = from p in _dbContext.PurchaseFlows
                         join m in _dbContext.WarehouseMembers on p.VerifyUserId equals m.UserId
                         where purchaseMainIdList.Contains( p.PurchaseMainId)
                         select new PurchaseFlowWithAgentsVo
                         {
                             FlowId = p.FlowId,
                             CompId = p.CompId,
                             PurchaseMainId = p.PurchaseMainId,
                             Reason = p.Reason,
                             Status = p.Status,
                             VerifyCompId = p.VerifyCompId,
                             VerifyUserId = p.VerifyUserId,
                             VerifyUserName = p.VerifyUserName,
                             Answer = p.Answer,
                             Sequence = p.Sequence,
                             ReadAt = p.ReadAt,
                             SubmitAt = p.SubmitAt,
                             CreatedAt = p.CreatedAt,
                             UpdatedAt = p.UpdatedAt,
                             Agents = m.Agents,
                             AgentNames = m.AgentNames,
                         };

            var list = result.ToList();
            list.ForEach(flow =>
            {
                if (!flow.Agents.IsNullOrEmpty())
                {
                    flow.VerifyAgentIds = flow.Agents.Split(",", StringSplitOptions.None).ToList();
                    flow.VerifyAgentNames = flow.AgentNames.Split(",", StringSplitOptions.None).ToList();
                }


            });
            return list;
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

        public bool CreatePurchase(PurchaseMainSheet newPurchasePurchaseMainSheet, List<PurchaseSubItem> newPurchaseSubItemList,
            List<PurchaseFlowSettingVo> purchaseFlowSettingList, List<ApplyProductFlowSettingVo> applyProductFlowSettingListForGroupReview,
            bool isItemMultiGroup, bool isOwnerCreate,WarehouseMember createMember,bool isNotStockComp = false,string? unitId=null)
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

                    List<PurchaseSubItemHistory> purchaseSubItemHistories = new List<PurchaseSubItemHistory>();
                    string formattedDate = new DateTime().ToString("yyyyMMdd");
                    string purchaseIdPrefix = purchaseMainId.Substring(0, 5);

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
                        // add history
                        PurchaseSubItemHistory newPurchaseSubItemHistory = new PurchaseSubItemHistory
                        {
                            Action = CommonConstants.PurchaseSubItemHistoryAction.ADD,
                            ItemId = item.ItemId,
                            PurchaseMainId = purchaseMainId,
                            PurchaseOrderNo = formattedDate + purchaseIdPrefix,
                            UserId = createMember.UserId,
                            UserName = createMember.DisplayName,
                            AfterValues = JsonSerializer.Serialize(item),
                        };
                        purchaseSubItemHistories.Add(newPurchaseSubItemHistory);
                    }
                    _dbContext.PurchaseSubItems.AddRange(newPurchaseSubItemList);
                    _dbContext.PurchaseSubItemHistories.AddRange(purchaseSubItemHistories);


                    List<PurchaseFlow> purchaseFlows = new();
                    DateTime submitedAt = DateTime.Now;
                    if (isItemMultiGroup == true)
                    {
                        foreach (var item in purchaseFlowSettingList)
                        {
                            var purchaseFlowForMultiGroup = new PurchaseFlow()
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
                            purchaseFlows.Add(purchaseFlowForMultiGroup);
                        }
                    }
                    if (isItemMultiGroup == false)
                    {
                        foreach (var item in applyProductFlowSettingListForGroupReview)
                        {
                            var purchaseFlowForSingleGroup = new PurchaseFlow()
                            {
                                FlowId = Guid.NewGuid().ToString(),
                                CompId = newPurchasePurchaseMainSheet.CompId,
                                PurchaseMainId = purchaseMainId,
                                Status = CommonConstants.PurchaseFlowStatus.WAIT,
                                VerifyCompId = newPurchasePurchaseMainSheet.CompId,
                                VerifyUserId = item.ReviewUserId,
                                VerifyUserName = item.ReviewUserName,
                                Answer = CommonConstants.PurchaseFlowAnswer.EMPTY,
                                Sequence = item.Sequence,
                                SubmitAt = submitedAt,
                            };
                            purchaseFlows.Add(purchaseFlowForSingleGroup);
                        }
                    }

                    if (isNotStockComp)
                    {
                        var  memberCompVoList = _memberService.GetReviewMemberCompVoList(unitId);
                        var noStockReviewers = memberCompVoList.Where(m=>m.IsNoStockReviewer==true).ToList();
                        // 找出院區需要跨comp審核者
                        var noStockReviewersCrossComp = noStockReviewers.Where(m=>m.Type!=CommonConstants.CompanyType.OWNER).ToList();
                        // 找出得標廠商需要審核者
                        var noStockReviewersOfOwner = noStockReviewers.Where(m => m.Type == CommonConstants.CompanyType.OWNER).ToList();
                        noStockReviewersCrossComp.ForEach(r =>
                        {
                            var maxSeq = 1;
                            if (purchaseFlows.Count > 0)
                            {
                                maxSeq = purchaseFlows.Select(f => f.Sequence).Max();
                            }

                            var purchaseFlow = new PurchaseFlow()
                            {
                                FlowId = Guid.NewGuid().ToString(),
                                CompId = newPurchasePurchaseMainSheet.CompId,
                                PurchaseMainId = purchaseMainId,
                                Status = CommonConstants.PurchaseFlowStatus.WAIT,
                                VerifyCompId = r.CompId,
                                VerifyUserId = r.UserId,
                                VerifyUserName = r.DisplayName,
                                Answer = CommonConstants.PurchaseFlowAnswer.EMPTY,
                                Sequence = maxSeq + 1,
                                SubmitAt = submitedAt,
                            };
                            purchaseFlows.Add(purchaseFlow);
                        });
                        noStockReviewersOfOwner.ForEach(r =>
                        {
                            var maxSeq = 1;
                            if (purchaseFlows.Count > 0)
                            {
                                maxSeq = purchaseFlows.Select(f => f.Sequence).Max();
                            }
                            var purchaseFlow = new PurchaseFlow()
                            {
                                FlowId = Guid.NewGuid().ToString(),
                                CompId = newPurchasePurchaseMainSheet.CompId,
                                PurchaseMainId = purchaseMainId,
                                Status = CommonConstants.PurchaseFlowStatus.WAIT,
                                VerifyCompId = r.CompId,
                                VerifyUserId = r.UserId,
                                VerifyUserName = r.DisplayName,
                                Answer = CommonConstants.PurchaseFlowAnswer.EMPTY,
                                Sequence = maxSeq + 1,
                                SubmitAt = submitedAt,
                            };
                            purchaseFlows.Add(purchaseFlow);
                        });
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
                                    subItem.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.AGREE;
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
                            if (withSubItems.All(item => item.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.AGREE))
                            {
                                withPurchaseMain.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.AGREE;
                            }else if(withSubItems.Any(item => item.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.AGREE))
                            {
                                withPurchaseMain.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.PART_AGREE;
                            }else if (withSubItems.All(item => item.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE))
                            {
                                withPurchaseMain.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE;
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



                    var purchaseFlow = purchaseFlows.OrderBy(setting=>setting.Sequence).FirstOrDefault();
                    if (purchaseFlow != null)
                    {
                        DateTime now = DateTime.Now;
                        string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(now), newPurchasePurchaseMainSheet.PurchaseMainId.AsSpan(0, 5))} 需要您審核";
                        string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMainId}>{purchaseMainId}</a>";
                        if (newPurchasePurchaseMainSheet.Type == CommonConstants.PurchaseType.URGENT)
                        {
                            title = "!!!!急件" + title;
                            content = $"<h2 style='color: red;'>急件請盡速處理</h2>" + content;
                            SendMailByFlow(purchaseFlow, title, content);
                        }
                        else
                        {
                            title = "以下採購單需要審核";
                            var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(now), newPurchasePurchaseMainSheet.PurchaseMainId.AsSpan(0, 5));
                            var receiver = _memberService.GetMembersByUserId(purchaseFlow.VerifyUserId);
                            EmailNotify emailNotify = new EmailNotify()
                            {
                                Title = title,
                                Content = content,
                                UserId = purchaseFlow.VerifyUserId,
                                Email = receiver.Email,
                                PurchaseNumber = purchaseNumber,
                                Type = CommonConstants.EmailNotifyType.PURCHASE
                            };
                            _emailService.AddEmailNotify(emailNotify);
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
            _logger.LogInformation("[ListPurchase]---1---{time}", DateTime.Now);
            IQueryable<PurchaseItemListView> query = _dbContext.PurchaseItemListViews;
            if (listPurchaseRequest.CompId != null)
            {
                query = query.Where(h => h.CompId == listPurchaseRequest.CompId);
            }
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
            _logger.LogInformation("[ListPurchase]---2---{time}", DateTime.Now);
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
            _logger.LogInformation("[ListPurchase]---3---{time}", DateTime.Now);
            List<PurchaseMainAndSubItemVo> purchaseMainAndSubItemVoList = new List<PurchaseMainAndSubItemVo> { };
            var flows = GetAllFlowsByCompId(listPurchaseRequest.CompId).OrderBy(f => f.Sequence);
            _logger.LogInformation("[ListPurchase]---4---{time}", DateTime.Now);
            _logger.LogInformation("mainSheetIdMap count:{count}", mainSheetIdMap.Count);

            List<PurchaseSubItemVo> allPurchaseSubItemVoList = new List<PurchaseSubItemVo>();
            foreach (var purchaseItemListView in result)
            {
                var subItem = new PurchaseSubItemVo()
                {
                    ItemId = purchaseItemListView.ItemId,
                    Comment = purchaseItemListView.Comment,
                    CompId = purchaseItemListView.CompId,
                    ProductCategory = purchaseItemListView.ProductCategory,
                    ProductName = purchaseItemListView.ProductName,
                    ProductId = purchaseItemListView.ProductId,
                    ProductSpec = purchaseItemListView.ProductSpec,
                    PurchaseMainId = purchaseItemListView.PurchaseMainId,
                    Quantity = purchaseItemListView.Quantity,
                    ReceiveQuantity = purchaseItemListView.ReceiveQuantity,
                    ReceiveStatus = purchaseItemListView.ItemReceiveStatus,
                    GroupIds = purchaseItemListView.GroupIds.Split(',').ToList(),
                    GroupNames = purchaseItemListView.ItemGroupNames.Split(",").ToList(),
                    ArrangeSupplierId = purchaseItemListView.ArrangeSupplierId,
                    ArrangeSupplierName = purchaseItemListView.ArrangeSupplierName,
                    CurrentInStockQuantity = purchaseItemListView.CurrentInStockQuantity,
                    CreatedAt = purchaseItemListView.CreatedAt.Value,
                    UpdatedAt = purchaseItemListView.UpdatedAt.Value,
                    SplitProcess = purchaseItemListView.SubSplitProcess,
                    OwnerProcess = purchaseItemListView.SubOwnerProcess,
                };
                allPurchaseSubItemVoList.Add(subItem);
            }
            _logger.LogInformation("[ListPurchase]---5---{time}", DateTime.Now);

            foreach (var kvp in mainSheetIdMap)
            {
                List<PurchaseSubItemVo> matchedSubItemVoList = allPurchaseSubItemVoList.Where(vo => vo.PurchaseMainId == kvp.Key).ToList();

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
                    Items = matchedSubItemVoList,
                };
                purchaseMainAndSubItemVoList.Add(vo);
            }
            _logger.LogInformation("[ListPurchase]---6---{time}", DateTime.Now);
            if (listPurchaseRequest.IsNeedFlow == true)
            {
                var differentMainSheetId = purchaseMainAndSubItemVoList.Select(m => m.PurchaseMainId).Distinct().ToList();
                foreach (var item in purchaseMainAndSubItemVoList)
                {
                    var matchedFlows = flows.Where(f => f.PurchaseMainId == item.PurchaseMainId).ToList();
                    var rejectedFlowIndex = matchedFlows.FindIndex(f => f.Status == CommonConstants.PurchaseFlowStatus.REJECT);
                    if (rejectedFlowIndex >= 0)
                    {
                        matchedFlows = matchedFlows.GetRange(0, rejectedFlowIndex + 1);
                    }
                    item.flows = _mapper.Map<List<PurchaseFlowWithAgentsVo>>(matchedFlows);
                }
            }
            _logger.LogInformation("[ListPurchase]---7---{time}", DateTime.Now);
            

            return purchaseMainAndSubItemVoList;
        }

        public List<PurchaseMainAndSubItemVo> ListMyReviewPurchase(ListMyReviewPurchaseRequest listMyReviewPurchaseRequest)
        {
            IQueryable<PurchaseItemListView> query = _dbContext.PurchaseItemListViews;
            
            if (listMyReviewPurchaseRequest.StartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(listMyReviewPurchaseRequest.StartDate);
                query = query.Where(h => h.UpdatedAt >= startDateTime);
            }
            if (listMyReviewPurchaseRequest.EndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(listMyReviewPurchaseRequest.EndDate).Value.AddDays(1);
                query = query.Where(h => h.UpdatedAt < endDateTime);
            }
            if (listMyReviewPurchaseRequest.Type != null)
            {
                query = query.Where(h => h.Type == listMyReviewPurchaseRequest.Type);
            }
            query = query.Where(h => h.CurrentStatus == CommonConstants.PurchaseCurrentStatus.APPLY);

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
            var flows = GetAllFlowsByCompId(null).OrderBy(f => f.Sequence);

            List<PurchaseSubItemVo> allPurchaseSubItemVoList = new List<PurchaseSubItemVo>();
            foreach (var purchaseItemListView in result)
            {
                var subItem = new PurchaseSubItemVo()
                {
                    ItemId = purchaseItemListView.ItemId,
                    Comment = purchaseItemListView.Comment,
                    CompId = purchaseItemListView.CompId,
                    ProductCategory = purchaseItemListView.ProductCategory,
                    ProductName = purchaseItemListView.ProductName,
                    ProductId = purchaseItemListView.ProductId,
                    ProductSpec = purchaseItemListView.ProductSpec,
                    PurchaseMainId = purchaseItemListView.PurchaseMainId,
                    Quantity = purchaseItemListView.Quantity,
                    ReceiveQuantity = purchaseItemListView.ReceiveQuantity,
                    ReceiveStatus = purchaseItemListView.ItemReceiveStatus,
                    GroupIds = purchaseItemListView.GroupIds.Split(',').ToList(),
                    GroupNames = purchaseItemListView.ItemGroupNames.Split(",").ToList(),
                    ArrangeSupplierId = purchaseItemListView.ArrangeSupplierId,
                    ArrangeSupplierName = purchaseItemListView.ArrangeSupplierName,
                    CurrentInStockQuantity = purchaseItemListView.CurrentInStockQuantity,
                    CreatedAt = purchaseItemListView.CreatedAt.Value,
                    UpdatedAt = purchaseItemListView.UpdatedAt.Value,
                    SplitProcess = purchaseItemListView.SubSplitProcess,
                    OwnerProcess = purchaseItemListView.SubOwnerProcess
                };
                allPurchaseSubItemVoList.Add(subItem);
            }

            foreach (var kvp in mainSheetIdMap)
            {
                List<PurchaseSubItemVo> matchedSubItemVoList = allPurchaseSubItemVoList.Where(vo => vo.PurchaseMainId == kvp.Key).ToList();

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
                    Items = matchedSubItemVoList,
                };
                purchaseMainAndSubItemVoList.Add(vo);
            }
            
            var differentMainSheetId = purchaseMainAndSubItemVoList.Select(m => m.PurchaseMainId).Distinct().ToList();
            foreach (var item in purchaseMainAndSubItemVoList)
            {
                var matchedFlows = flows.Where(f => f.PurchaseMainId == item.PurchaseMainId).ToList();
                var rejectedFlowIndex = matchedFlows.FindIndex(f => f.Status == CommonConstants.PurchaseFlowStatus.REJECT);
                if (rejectedFlowIndex >= 0)
                {
                    matchedFlows = matchedFlows.GetRange(0, rejectedFlowIndex + 1);
                }
                item.flows = _mapper.Map<List<PurchaseFlowWithAgentsVo>>(matchedFlows);
            }
            purchaseMainAndSubItemVoList = purchaseMainAndSubItemVoList.FindAll(element=> element.flows!=null&&element.flows.Find(f=>f.VerifyUserId== listMyReviewPurchaseRequest.UserId)!=null).ToList();
            return purchaseMainAndSubItemVoList;
        }

        public List<PurchaseFlow> GetFlowsByPurchaseMainIds(List<string> purchaseMainIdList)
        {
            return _dbContext.PurchaseFlows.Where(f => purchaseMainIdList.Contains(f.PurchaseMainId)).ToList();

        }
        public List<PurchaseFlow> GetAllFlowsByCompId(string? compId)
        {
            if (compId != null)
            {
                return _dbContext.PurchaseFlows.Where(f => f.CompId == compId).ToList();

            }
            return _dbContext.PurchaseFlows.ToList();
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
                foreach (var item in toUpdateSubItems)
                {
                    // 如果 sub item 全部同意，代表該 sub item 已經出貨，更新 sub item 的 ReceiveStatus 為 DELIVERED
                    if (item.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE)
                    {
                        item.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.CLOSE;
                    }
                }
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

                    // 當所有 sub items 只有 NOT_AGREE 和 AGREE 兩種狀態時，將主單結案
                    if (allPurchaseSubItems.All(s => s.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE 
                        || s.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.AGREE))
                    {
                        main.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.CLOSE;
                        main.CurrentStatus = CommonConstants.PurchaseApplyStatus.CLOSE;
                        var subItems = _dbContext.PurchaseSubItems.Where(s => s.PurchaseMainId == main.PurchaseMainId).ToList();
                    }
                }else if(allPurchaseSubItems.All(s => s.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE))
                {
                    main.OwnerProcess = CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE;
                    main.SplitPrcoess = CommonConstants.SplitProcess.DONE;
                    main.CurrentStatus = CommonConstants.PurchaseApplyStatus.CLOSE;
                    var subItems = _dbContext.PurchaseSubItems.Where(s => s.PurchaseMainId == main.PurchaseMainId).ToList();
                    subItems.ForEach(s => s.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.CLOSE);
                }

                if(allPurchaseSubItems.All(s=>s.ReceiveStatus== CommonConstants.PurchaseSubItemReceiveStatus.CLOSE|| s.ReceiveStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE))
                {
                    main.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.CLOSE;
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

        public void UpdatePurchaseOwnerComment(PurchaseMainSheet main,  UpdateOwnerCommentRequest request)
        {
            main.OwnerComment = request.OwnerComment;
            _dbContext.SaveChanges();
        }

        public void UpdateSubItemOwnerComment(PurchaseSubItem item, UpdateSubItemOwnerCommentRequest request)
        {
            item.OwnerComment = request.OwnerComment;
            _dbContext.SaveChanges();
        }

        public void UpdateSubItemVendorComment(PurchaseSubItem item, UpdateSubItemVendorCommentRequest request)
        {
            item.VendorComment = request.VendorComment;
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

        public bool AnswerFlow(PurchaseFlow flow, MemberAndPermissionSetting verifierMemberAndPermission, string answer, string? reason,bool? isOwner,bool isVerifiedByAgent)
        {
            string purchaseMainId = flow.PurchaseMainId;
            PurchaseMainSheet purchaseMain = GetPurchaseMainByMainId(purchaseMainId);
            List<PurchaseSubItem> purchaseSubItems = GetPurchaseSubItemsByMainId(purchaseMainId);
            var (preFlow, nextFlow) = FindPreviousAndNextFlow(flow);
            return AnswerFlowInTransactionScope(preFlow, nextFlow, flow, purchaseMain, purchaseSubItems, verifierMemberAndPermission, answer, reason,isOwner, isVerifiedByAgent);
        }

        public (PurchaseFlow?, PurchaseFlow?) FindPreviousAndNextFlow(PurchaseFlow flow)
        {
            List<PurchaseFlow> purchaseFlows = _dbContext.PurchaseFlows.Where(f => f.PurchaseMainId == flow.PurchaseMainId).OrderBy(f => f.Sequence).ToList();

            return (purchaseFlows.FirstOrDefault(f => f.Sequence < flow.Sequence), purchaseFlows.FirstOrDefault(f => f.Sequence > flow.Sequence));
        }

        private bool AnswerFlowInTransactionScope(PurchaseFlow? preFlow, PurchaseFlow? nextPurchase, PurchaseFlow currentFlow, PurchaseMainSheet purchaseMain,List<PurchaseSubItem> purchaseSubItems, MemberAndPermissionSetting verifierMemberAndPermission, string answer, string? reason,bool? isOwner, bool isVerifiedByAgent)
        {
            WarehouseMember verifyMember = verifierMemberAndPermission.Member;
            var verifyCompId = verifierMemberAndPermission.CompanyWithUnit.CompId;
            var originVerifierName = currentFlow.VerifyUserName;
            List<WarehouseMember> ownerList = new();
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // 更新Flow
                    currentFlow.Reason = reason;
                    if (answer != CommonConstants.PurchaseApplyStatus.BACK)
                    {
                        
                        currentFlow.VerifyCompId = verifyCompId;
                        currentFlow.VerifyUserId = verifyMember.UserId;
                        if (isVerifiedByAgent == true)
                        {
                            currentFlow.VerifyUserName = verifyMember.DisplayName+"(代"+ originVerifierName + ")";
                        }
                        else
                        {
                            currentFlow.VerifyUserName = verifyMember.DisplayName;
                        }
                        
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
                                OrderQuantity = item.Quantity ?? 0,
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
                        purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.CLOSE;
                        var subItems = _dbContext.PurchaseSubItems.Where(s => s.PurchaseMainId == purchaseMain.PurchaseMainId).ToList();
                        subItems.ForEach(s => s.ReceiveStatus=CommonConstants.PurchaseSubItemReceiveStatus.CLOSE);
                    }
                    if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner != true)
                    {
                        currentFlow.Status = answer;

                        currentFlow.Answer = "";
                        if (preFlow != null)
                        {
                            preFlow.Status = "";
                            preFlow.Answer = "";
                        }
                        else
                        {
                            purchaseMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.REJECT;
                            purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.CLOSE;
                            var subItems = _dbContext.PurchaseSubItems.Where(s => s.PurchaseMainId == purchaseMain.PurchaseMainId).ToList();
                            subItems.ForEach(s => s.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.CLOSE);
                        }
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
                        UserName = isVerifiedByAgent==false?verifyMember.DisplayName: verifyMember.DisplayName + "(代" + originVerifierName + ")",
                        Sequence = currentFlow.Sequence,
                        Action = answer,
                        Remarks = reason
                    };
                    

                    ownerList = _memberService.GetOwnerMembers();
                    var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5));
                    _emailService.UpdateEmailNotifyIsDoneByIdPurchaseNumber(purchaseNumber);

                    _dbContext.PurchaseFlowLogs.Add(newFlowLog);
                    _dbContext.SaveChanges();
                    scope.Complete();

                }
                catch (Exception ex)
                {
                    _logger.LogError("事務失敗[AnswerFlow]：{msg}", ex);
                    return false;
                }
            }
            

            if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase == null)
            {
                string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 需要您處理";
                string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                if (purchaseMain.Type == CommonConstants.PurchaseType.URGENT)
                {
                    title = "!!!!急件" + title;
                    content = $"<h2 style='color: red;'>急件請盡速處理</h2>" + content;
                    SendMailToOwner(title, content,ownerList);

                    // 不需通知流程跑完
                    //title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 審核流程已跑完";
                    //content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                    //SendMailByPurchaseMain(purchaseMain, title,content);
                }
            }
            if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase != null)
            {
                string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 需要您審核";
                string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                if (purchaseMain.Type == CommonConstants.PurchaseType.URGENT)
                {
                    title = "!!!!急件" + title;
                    content = $"<h2 style='color: red;'>急件請盡速處理</h2>" + content;
                    SendMailByFlow(nextPurchase, title, content);
                }
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        title = "以下採購單需要審核";
                        var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5));
                        var receiver = _memberService.GetMembersByUserId(nextPurchase.VerifyUserId);
                        EmailNotify emailNotify = new EmailNotify()
                        {
                            Title = title,
                            Content = content,
                            UserId = receiver.UserId,
                            Email = receiver.Email,
                            PurchaseNumber = purchaseNumber,
                            Type = CommonConstants.EmailNotifyType.PURCHASE
                        };
                        _emailService.AddEmailNotify(emailNotify);
                        _dbContext.SaveChanges();
                        scope.Complete();
                    }

                }
            }
            if (answer == CommonConstants.AnswerPurchaseFlow.REJECT)
            {
                //string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 已被拒絕";
                //string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                //if (purchaseMain.Type == CommonConstants.PurchaseType.URGENT)
                //{
                //    title = "!!!!急件" + title;
                //    content = $"<h2 style='color: red;'>急件已被退件</h2>" + content;
                //    SendMailByPurchaseMain(purchaseMain, title, content);

                //}
                //else
                //{
                //    using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                //    {
                //        title = "以下採購單被拒絕";
                //        var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5));
                //        var receiver = _memberService.GetMembersByUserId(purchaseMain.UserId);
                //        SendMailByPurchaseMain(purchaseMain, title, content);
                //    }
                //}
            }
            if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner != true)
            {
                if (preFlow != null)
                {
                    string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 需要您審核";
                    string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                    if (purchaseMain.Type == CommonConstants.PurchaseType.URGENT)
                    {
                        title = "!!!!急件" + title;
                        content = $"<h2 style='color: red;'>急件請盡速處理</h2>" + content;
                        SendMailByFlow(preFlow, title, content);
                    }
                    else
                    {
                        using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            title = "以下採購單需要審核";
                            var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5));
                            var receiver = _memberService.GetMembersByUserId(preFlow.VerifyUserId);
                            EmailNotify emailNotify = new EmailNotify()
                            {
                                Title = title,
                                Content = content,
                                UserId = receiver.UserId,
                                Email = receiver.Email,
                                PurchaseNumber = purchaseNumber,
                                Type = CommonConstants.EmailNotifyType.PURCHASE
                            };
                            _emailService.AddEmailNotify(emailNotify);
                            _dbContext.SaveChanges();
                            scope.Complete();
                        }
                    }
                }
                else
                {
                    //string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 已被退回";
                    //string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                    //if (purchaseMain.Type == CommonConstants.PurchaseType.URGENT)
                    //{
                    //    title = "!!!!急件" + title;
                    //    content = $"<h2 style='color: red;'>急件已被退回</h2>" + content;
                    //    SendMailByPurchaseMain(purchaseMain, title, content);
                    //}
                    //else
                    //{
                    //    using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        title = "以下採購已被退回";
                    //        var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5));
                    //        var receiver = _memberService.GetMembersByUserId(purchaseMain.UserId);
                    //        EmailNotify emailNotify = new EmailNotify()
                    //        {
                    //            Title = title,
                    //            Content = content,
                    //            UserId = receiver.UserId,
                    //            Email = receiver.Email,
                    //            PurchaseNumber = purchaseNumber,
                    //            Type = CommonConstants.EmailNotifyType.PURCHASE
                    //        };
                    //        _emailService.AddEmailNotify(emailNotify);
                    //        _dbContext.SaveChanges();
                    //        scope.Complete();
                    //    }
                    //}
                }
            }
            if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner == true)
            {
                //string title = $"採購單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5))} 已被退回";
                //string content = $"<a href={_smtpSettings.Domain}/purchase_flow_detail/{purchaseMain.PurchaseMainId}>{purchaseMain.PurchaseMainId}</a>";
                //if (purchaseMain.Type == CommonConstants.PurchaseType.URGENT)
                //{
                //    title = "!!!!急件" + title;
                //    content = $"<h2 style='color: red;'>急件已被退回</h2>" + content;
                //    SendMailByFlow(currentFlow, title, content);
                //}
                //else
                //{
                //    using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                //    {
                //        title = "以下採購已被退回";
                //        var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(purchaseMain.ApplyDate), purchaseMain.PurchaseMainId.AsSpan(0, 5));
                //        var receiver = _memberService.GetMembersByUserId(currentFlow.VerifyUserId);
                //        EmailNotify emailNotify = new EmailNotify()
                //        {
                //            Title = title,
                //            Content = content,
                //            UserId = receiver.UserId,
                //            Email = receiver.Email,
                //            PurchaseNumber = purchaseNumber,
                //            Type = CommonConstants.EmailNotifyType.PURCHASE
                //        };
                //        _emailService.AddEmailNotify(emailNotify);
                //        _dbContext.SaveChanges();
                //        scope.Complete();
                //    }
                //}
                
            }

            return true;
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
                // Delete history
                var allSubItems = _dbContext.PurchaseSubItems.Where(s=>s.PurchaseMainId==purchaseMainSheet.PurchaseMainId).ToList();
                var matchedDeletedSubItems = allSubItems.Where(subItem => request.DeleteSubItemIdList.Contains(subItem.ItemId)).ToList();
                matchedDeletedSubItems.ForEach(subItem => {
                    string beforeJsonString = JsonSerializer.Serialize(subItem);
                    string formattedDate = purchaseMainSheet.ApplyDate.ToString("yyyyMMdd");
                    string purchaseIdPrefix = purchaseMainSheet.PurchaseMainId.Substring(0, 5);
                    PurchaseSubItemHistory newPurchaseSubItemHistory = new PurchaseSubItemHistory
                    {
                        Action = CommonConstants.PurchaseSubItemHistoryAction.DELETE,
                        ItemId = subItem.ItemId,
                        PurchaseMainId = subItem.PurchaseMainId,
                        PurchaseOrderNo = formattedDate + purchaseIdPrefix,
                        UserId = user.UserId,
                        UserName = user.DisplayName,
                        BeforeValues = beforeJsonString,
                    };
                    _dbContext.PurchaseSubItemHistories.Add(newPurchaseSubItemHistory);
                });

                _dbContext.PurchaseSubItems.Where(subItem => request.DeleteSubItemIdList.Contains(subItem.ItemId)).ExecuteDelete();
                _dbContext.AcceptanceItems.Where(acceptItem => request.DeleteSubItemIdList.Contains(acceptItem.ItemId)).ExecuteDelete();
                
                // update history
                request.UpdateSubItemList.ForEach(subItem =>
                {
                    var updateSubItemId = subItem.ItemId;
                    if (!request.DeleteSubItemIdList.Contains(updateSubItemId))
                    {
                        var matchedUpdateItem = purchaseSubItemList.Where(i => i.ItemId == subItem.ItemId).FirstOrDefault();
                        if (matchedUpdateItem != null)
                        {
                            string beforeJsonString = JsonSerializer.Serialize(matchedUpdateItem);
                            string formattedDate = purchaseMainSheet.ApplyDate.ToString("yyyyMMdd");
                            string purchaseIdPrefix = purchaseMainSheet.PurchaseMainId.Substring(0, 5);
                            matchedUpdateItem.Quantity = subItem.Quantity;
                            string afterJsonString = JsonSerializer.Serialize(matchedUpdateItem);

                            PurchaseSubItemHistory newPurchaseSubItemHistory = new PurchaseSubItemHistory
                            {
                                Action = CommonConstants.PurchaseSubItemHistoryAction.MODIFY,
                                ItemId = matchedUpdateItem.ItemId,
                                PurchaseMainId = matchedUpdateItem.PurchaseMainId,
                                PurchaseOrderNo = formattedDate + purchaseIdPrefix,
                                UserId = user.UserId,
                                UserName = user.DisplayName,
                                BeforeValues = beforeJsonString,
                                AfterValues = afterJsonString
                            };
                            _dbContext.AcceptanceItems.Where(acceptItem => acceptItem.ItemId == matchedUpdateItem.ItemId).ExecuteUpdate(a => a.SetProperty(a => a.OrderQuantity, subItem.Quantity));
                            _dbContext.PurchaseSubItemHistories.Add(newPurchaseSubItemHistory);

                            var matchedAcceptanceItem = _dbContext.AcceptanceItems.Where(a => a.ItemId == matchedUpdateItem.ItemId).FirstOrDefault();
                            if (matchedAcceptanceItem != null)
                            {
                                // 判斷是否全部驗收完
                                if (matchedAcceptanceItem.AcceptQuantity != null && matchedAcceptanceItem.AcceptQuantity >= subItem.Quantity)
                                {
                                    matchedAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                                    matchedUpdateItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                                    matchedUpdateItem.InStockQuantity = matchedAcceptanceItem.AcceptQuantity;
                                    matchedUpdateItem.ReceiveQuantity = matchedUpdateItem.InStockQuantity;
                                }
                                else if (matchedAcceptanceItem.AcceptQuantity != null && matchedAcceptanceItem.AcceptQuantity > 0 && matchedAcceptanceItem.AcceptQuantity < subItem.Quantity)
                                {
                                    // 判斷是否部分驗收
                                    _logger.LogInformation("[品項部分驗收] AcceptId:${acceptId},AcceptQuantity:${AcceptQuantity},OrderQuantity:${OrderQuantity}", matchedAcceptanceItem.AcceptId, matchedAcceptanceItem.AcceptQuantity, subItem.Quantity);
                                    matchedAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                                    matchedUpdateItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                                    matchedUpdateItem.InStockQuantity = matchedAcceptanceItem.AcceptQuantity;
                                    matchedUpdateItem.ReceiveQuantity = matchedUpdateItem.InStockQuantity;
                                }
                                else if (matchedAcceptanceItem.OrderQuantity == 0 || subItem.Quantity == 0)
                                {
                                    matchedAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.CLOSE;
                                    matchedUpdateItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.CLOSE;
                                    matchedUpdateItem.InStockQuantity = matchedAcceptanceItem.AcceptQuantity;
                                    matchedUpdateItem.ReceiveQuantity = matchedUpdateItem.InStockQuantity;
                                }

                            }           
                        }
                    }
                });

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


            using var scope2 = new TransactionScope();
            try
            {
                var allAcceptItems = _dbContext.AcceptanceItems.Where(i => i.PurchaseMainId == purchaseMainSheet.PurchaseMainId && i.CompId == compId).ToList();
                if (allAcceptItems.All(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE))
                {
                    purchaseMainSheet.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.ALL_ACCEPT;
                }
                else if (allAcceptItems.Any(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.PART ))
                {
                    purchaseMainSheet.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.PART_ACCEPT;
                }
                _dbContext.SaveChanges();
                scope2.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateOrDeleteSubItems][更新採購單狀態失敗]：{msg}", ex);
                return false;
            }
        }

        // 得標廠商修改採購項目子項
        public bool OwnerUpdateOrDeleteSubItems(UpdateOrDeleteSubItemInFlowRequest request,PurchaseMainSheet purchaseMainSheet,List<PurchaseSubItem> purchaseSubItemList,WarehouseMember user)
        {
            using var scope = new TransactionScope();
            try
            {
                var beforeSubItemsJsonString = JsonSerializer.Serialize(purchaseSubItemList);
                // Delete history
                var matchedDeletedSubItems = _dbContext.PurchaseSubItems.Where(subItem => request.DeleteSubItemIdList.Contains(subItem.ItemId)).ToList();
                matchedDeletedSubItems.ForEach(subItem => {
                    string beforeJsonString = JsonSerializer.Serialize(subItem);
                    string formattedDate = purchaseMainSheet.ApplyDate.ToString("yyyyMMdd");
                    string purchaseIdPrefix = purchaseMainSheet.PurchaseMainId.Substring(0, 5);
                    PurchaseSubItemHistory newPurchaseSubItemHistory = new PurchaseSubItemHistory
                    {
                        Action = CommonConstants.PurchaseSubItemHistoryAction.DELETE,
                        ItemId = subItem.ItemId,
                        PurchaseMainId = subItem.PurchaseMainId,
                        PurchaseOrderNo = formattedDate + purchaseIdPrefix,
                        UserId = user.UserId,
                        UserName = user.DisplayName,
                        BeforeValues = beforeJsonString,
                    };
                    _dbContext.PurchaseSubItemHistories.Add(newPurchaseSubItemHistory);
                });
                _dbContext.PurchaseSubItems.Where(subItem => request.DeleteSubItemIdList.Contains(subItem.ItemId)).ExecuteDelete();
                _dbContext.AcceptanceItems.Where(acceptItem => request.DeleteSubItemIdList.Contains(acceptItem.ItemId)).ExecuteDelete();

                // update history
                request.UpdateSubItemList.ForEach(subItem =>
                {
                    if (!request.DeleteSubItemIdList.Contains(subItem.ItemId))
                    {
                        var matchedUpdateItem = purchaseSubItemList.Where(i => i.ItemId == subItem.ItemId).FirstOrDefault();
                        if (matchedUpdateItem != null)
                        {
                            string beforeJsonString = JsonSerializer.Serialize(matchedUpdateItem);
                            string formattedDate = purchaseMainSheet.ApplyDate.ToString("yyyyMMdd");
                            string purchaseIdPrefix = purchaseMainSheet.PurchaseMainId.Substring(0, 5);
                            matchedUpdateItem.Quantity = subItem.Quantity;
                            string afterJsonString = JsonSerializer.Serialize(matchedUpdateItem);

                            PurchaseSubItemHistory newPurchaseSubItemHistory = new PurchaseSubItemHistory
                            {
                                Action = CommonConstants.PurchaseSubItemHistoryAction.MODIFY,
                                ItemId = matchedUpdateItem.ItemId,
                                PurchaseMainId = matchedUpdateItem.PurchaseMainId,
                                PurchaseOrderNo = formattedDate + purchaseIdPrefix,
                                UserId = user.UserId,
                                UserName = user.DisplayName,
                                BeforeValues = beforeJsonString,
                                AfterValues = afterJsonString
                            };
                            _dbContext.AcceptanceItems.Where(acceptItem => acceptItem.ItemId == matchedUpdateItem.ItemId).ExecuteUpdate(a => a.SetProperty(a => a.OrderQuantity, subItem.Quantity));
                            _dbContext.PurchaseSubItemHistories.Add(newPurchaseSubItemHistory);
                            var matchedAcceptanceItem = _dbContext.AcceptanceItems.Where(a => a.ItemId == matchedUpdateItem.ItemId).FirstOrDefault();
                            if (matchedAcceptanceItem != null)
                            {
                                // 判斷是否全部驗收完
                                if (matchedAcceptanceItem.AcceptQuantity != null && matchedAcceptanceItem.AcceptQuantity >= subItem.Quantity)
                                {
                                    matchedAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                                    matchedUpdateItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                                    matchedUpdateItem.InStockQuantity = matchedAcceptanceItem.AcceptQuantity;
                                    matchedUpdateItem.ReceiveQuantity = matchedUpdateItem.InStockQuantity;
                                }
                                else if (matchedAcceptanceItem.AcceptQuantity != null && matchedAcceptanceItem.AcceptQuantity > 0 && matchedAcceptanceItem.AcceptQuantity < subItem.Quantity)
                                {
                                    // 判斷是否部分驗收
                                    _logger.LogInformation("[品項部分驗收] AcceptId:${acceptId},AcceptQuantity:${AcceptQuantity},OrderQuantity:${OrderQuantity}", matchedAcceptanceItem.AcceptId, matchedAcceptanceItem.AcceptQuantity, subItem.Quantity);
                                    matchedAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                                    matchedUpdateItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                                    matchedUpdateItem.InStockQuantity = matchedAcceptanceItem.AcceptQuantity;
                                    matchedUpdateItem.ReceiveQuantity = matchedUpdateItem.InStockQuantity;
                                }
                            }
                        }
                    }
                });
                _dbContext.SaveChanges();
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateOrDeleteSubItems]：{msg}", ex);
                return false;
            }

            using var scope2 = new TransactionScope();
            try
            {
                var allAcceptItems = _dbContext.AcceptanceItems.Where(i => i.PurchaseMainId == purchaseMainSheet.PurchaseMainId).ToList();
                if (allAcceptItems.All(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE))
                {
                    purchaseMainSheet.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.ALL_ACCEPT;
                }
                else if (allAcceptItems.Any(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.PART))
                {
                    purchaseMainSheet.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.PART_ACCEPT;
                }
                _dbContext.SaveChanges();
                scope2.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OwnerUpdateOrDeleteSubItems][更新採購單狀態失敗]：{msg}", ex);
                return false;
            }
        }

        

        public float GetInProcessingOrderQuantity(string productId)
        {
            var allEffectivePurchaseMain = _dbContext.PurchaseMainSheets.Where(m=>m.CurrentStatus!=CommonConstants.PurchaseCurrentStatus.CLOSE&&
            m.CurrentStatus!= CommonConstants.PurchaseCurrentStatus.REJECT).ToList();
            var allEffectivePurchaseMainId = allEffectivePurchaseMain.Select(m => m.PurchaseMainId).ToList();

            var unDoneProcessingSubItem = _dbContext.PurchaseSubItems.Where(s => s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE
            && s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE
            && s.ProductId == productId&&s.OwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE
            && allEffectivePurchaseMainId.Contains(s.PurchaseMainId)).ToList();
            return unDoneProcessingSubItem.Select(s => s.Quantity ?? 0.0f).DefaultIfEmpty(0.0f).Sum();
        }

        public List<PurchaseItemListView> GetNotDonePurchaseSubItemByProductIdList(List<string> productIdList)
        {
            return _dbContext.PurchaseItemListViews.Where(s => s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE
            //&& s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE 
            && s.ItemReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE
            && s.OwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE
            && s.SubOwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE
            && s.CurrentStatus != CommonConstants.PurchaseCurrentStatus.REJECT
            && s.CurrentStatus != CommonConstants.PurchaseCurrentStatus.CLOSE
            && productIdList.Contains(s.ProductId)).ToList();
        }

        public List<PurchaseItemListView> GetUndonePurchaseSubItems(string compId, string productId)
        {
            return _dbContext.PurchaseItemListViews.Where(s => s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE
            //&& s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE
            && s.ItemReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE
            && s.OwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE
            && s.SubOwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE
            && s.CurrentStatus != CommonConstants.PurchaseCurrentStatus.REJECT 
            && s.CurrentStatus != CommonConstants.PurchaseCurrentStatus.CLOSE
            && s.CompId == compId
            && s.ProductId == productId).ToList();
        }

        private async Task SendMailByFlowSetting(PurchaseFlowSettingVo purchaseFlowSettingVo, String title, String content)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                var receiver = _memberService.GetMembersByUserId(purchaseFlowSettingVo.UserId);
                if (receiver != null)
                {

                    if (!string.IsNullOrEmpty(receiver.Email))
                    {
                        await _emailService.SendAsync(title, content, receiver.Email);
                        _logger.LogInformation("[寄信]標題:{title},收件者:{email}", title, receiver.Email);
                    }
                }
                scope.Complete();
            }
        }

        private async Task SendMailByFlow(PurchaseFlow flow, String title, String content)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                var receiver = _memberService.GetMembersByUserId(flow.VerifyUserId);
                if (receiver != null)
                {

                    if (!string.IsNullOrEmpty(receiver.Email))
                    {
                        await _emailService.SendAsync(title, content, receiver.Email);
                        _logger.LogInformation("[寄信]標題:{title},收件者:{email}", title, receiver.Email);
                    }
                }
                scope.Complete();
            }
        }

        private async Task SendMailByPurchaseMain(PurchaseMainSheet main, String title, String content)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                var receiver = _memberService.GetMembersByUserId(main.UserId);
                if (receiver != null)
                {

                    if (!string.IsNullOrEmpty(receiver.Email))
                    {
                        await _emailService.SendAsync(title, content, receiver.Email);
                        _logger.LogInformation("[寄信]標題:{title},收件者:{email}", title, receiver.Email);
                    }
                }
                scope.Complete();
            }
        }

        public List<PurchaseDetailView> GetPurchaseDetailListByItemIdList(List<string> itemIdList)
        {
            return _dbContext.PurchaseDetailViews.Where(v => itemIdList.Contains(v.ItemId)).ToList();  
        }

        // 新增：取得每個 productId 的上次訂購日期 (不包含指定的 purchaseMainId)
        public Dictionary<string, DateTime?> GetLastOrderDateByProductIds(List<string> productIdList, string? excludePurchaseMainId = null)
        {
            if (productIdList == null || productIdList.Count == 0)
            {
                return new Dictionary<string, DateTime?>();
            }

            var query = from s in _dbContext.PurchaseSubItems
                        join m in _dbContext.PurchaseMainSheets on s.PurchaseMainId equals m.PurchaseMainId
                        where productIdList.Contains(s.ProductId)
                              && m.OwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE
                              && m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.REJECT 
                              && m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.CLOSE
                              && s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE
                        select new { s.ProductId, m.PurchaseMainId, m.ApplyDate };

            var list = query.ToList();

            var grouped = list
                .Where(x => excludePurchaseMainId == null || x.PurchaseMainId != excludePurchaseMainId)
                .GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => (DateTime?)g.Max(x => x.ApplyDate));

            return grouped;
        }

        private async Task SendMailToOwner(String title, String content,List<WarehouseMember> ownerList)
        {
            
            ownerList.ForEach(async r =>
            {
                if (!string.IsNullOrEmpty(r.Email))
                {
                    await _emailService.SendAsync(title, content, r.Email);
                    _logger.LogInformation("[寄信]標題:{title},收件者:{email}", title, r.Email);
                }
            });

        }

        public List<PurchaseSubItemHistory> ListSubItemListHistory(string purchaseMainId)
        {
            return _dbContext.PurchaseSubItemHistories.Where(h => h.PurchaseMainId == purchaseMainId).ToList();
        }
    }
}
