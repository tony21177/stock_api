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

        public ApplyNewProductMain? GetApplyNewProductMainByApplyId(string applyId)
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
                string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{newApplyNewProductMain.ApplyId}>{newApplyNewProductMain.ApplyId}</a>";
                SendMailByFlowSetting(firstFlow, title, content);

                if (firstFlow != null)
                {
                    title = "申請新品項單需要審核";
                    var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(now), newApplyNewProductMain.ApplyId.AsSpan(0, 5));
                    var receiver = _memberService.GetMembersByUserId(firstFlow.ReviewUserId);
                    EmailNotify emailNotify = new EmailNotify()
                    {
                        Title = title,
                        Content = content,
                        UserId = firstFlow.ReviewUserId,
                        Email = receiver.Email,
                        PurchaseNumber = purchaseNumber,
                        Type = CommonConstants.EmailNotifyType.APPLY_NEW_PRODUCT
                    };
                    _emailService.AddEmailNotify(emailNotify);
                }


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

        

        public (List<ApplyNewProductMainWithFlowVo>,int) ListApplyNewProductMain(ListApplyNewProductMainRequest listRequest,bool enablePagination)
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
            if (enablePagination)
            {
                query = query.Skip((listRequest.PaginationCondition.Page - 1) * listRequest.PaginationCondition.PageSize).Take(listRequest.PaginationCondition.PageSize);
            }

            var applyNewProductMainList = query.ToList();
            var applyNewProductMainWithFlowList = _mapper.Map<List<ApplyNewProductMainWithFlowVo>>(applyNewProductMainList);

            var allApplyId = applyNewProductMainList.Select(m=>m.ApplyId).Distinct().ToList();

            var allRelatedFlows =  _dbContext.ApplyNewProductFlows.Where(f=>allApplyId.Contains(f.ApplyId)).ToList();

            applyNewProductMainWithFlowList.ForEach(main =>
            {
                var matchedFlows = allRelatedFlows.Where(f => f.ApplyId == main.ApplyId).OrderBy(f => f.Sequence).ToList();
                main.Flows = matchedFlows;
            });

            return (applyNewProductMainWithFlowList,totalPages);
        }

        public List<ApplyNewProductFlow> GetFlowsByApplyIds(List<string> applyIdList)
        {
            return _dbContext.ApplyNewProductFlows.Where(f => applyIdList.Contains(f.ApplyId)).ToList();

        }

        public List<ApplyNewProductFlow> GetFlowsByApplyIds(string applyId)
        {
            return _dbContext.ApplyNewProductFlows.Where(f => f.ApplyId== applyId).ToList();

        }

        public ApplyNewProductFlow? GetFlowByFlowId(string flowId)
        {
            return _dbContext.ApplyNewProductFlows.Where(f => f.FlowId == flowId).FirstOrDefault();

        }
    

        public List<ApplyNewProductFlow> GetBeforeFlows(ApplyNewProductFlow nowFlow)
        {
            return _dbContext.ApplyNewProductFlows.Where(f=>f.Sequence<nowFlow.Sequence&&f.CompId==nowFlow.CompId&&f.ApplyId==nowFlow.ApplyId).OrderBy(f=>f.Sequence).ToList();
        }

        public bool AnswerFlow(ApplyNewProductFlow flow, string answer, string? reason, bool? isOwner)
        {
            string applyId = flow.ApplyId;
            ApplyNewProductMain main = GetApplyNewProductMainByApplyId(applyId);
            var (preFlow, nextFlow) = FindPreviousAndNextFlow(flow);
            return AnswerFlowInTransactionScope(preFlow, nextFlow, flow, main, answer, reason, isOwner);
        }

        public (ApplyNewProductFlow?, ApplyNewProductFlow?) FindPreviousAndNextFlow(ApplyNewProductFlow flow)
        {
            List<ApplyNewProductFlow> flows = _dbContext.ApplyNewProductFlows.Where(f => f.ApplyId == flow.ApplyId).OrderBy(f => f.Sequence).ToList();

            return (flows.FirstOrDefault(f => f.Sequence < flow.Sequence), flows.FirstOrDefault(f => f.Sequence > flow.Sequence));
        }

        private bool AnswerFlowInTransactionScope(ApplyNewProductFlow? preFlow, ApplyNewProductFlow? nextPurchase, ApplyNewProductFlow currentFlow, ApplyNewProductMain applyNewProductMain, string answer, string? reason,bool? isOwner)
        {
            List<WarehouseMember> ownerList = new();
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    ownerList = _memberService.GetOwnerMembers();
                    // 更新Flow
                    currentFlow.Reason = reason;
                    if (answer != CommonConstants.AnswerApplyNewProductFlow.BACK)
                    {
                        currentFlow.ReviewCompId = currentFlow.ReviewCompId;
                        currentFlow.ReviewUserId = currentFlow.ReviewUserId;
                        currentFlow.ReviewUserName = currentFlow.ReviewUserName;
                        currentFlow.ReviewGroupId = currentFlow.ReviewGroupId;
                        currentFlow.ReviewGroupName = currentFlow.ReviewGroupName;
                    }

                    currentFlow.Answer = answer;
                    currentFlow.SubmitAt = DateTime.Now;

                    // 更新主單狀態
                    if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase == null)
                    {
                        currentFlow.Status = answer;
                        applyNewProductMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.AGREE;
                    }
                    if (answer == CommonConstants.AnswerPurchaseFlow.REJECT)
                    {
                        currentFlow.Status = answer;
                        applyNewProductMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.REJECT;
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
                            applyNewProductMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.REJECT;

                        }
                    }
                    if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner == true)
                    {
                        currentFlow.Status = "";
                        currentFlow.Answer = "";
                        applyNewProductMain.CurrentStatus = CommonConstants.PurchaseApplyStatus.BACK;
                    }


                    // 新增log
                    var newFlowLog = new ApplyProductFlowLog()
                    {
                        LogId = Guid.NewGuid().ToString(),
                        CompId = currentFlow.CompId,
                        ApplyId = currentFlow.ApplyId,
                        UserId = currentFlow.ReviewUserId,
                        UserName = currentFlow.ReviewUserName,
                        Sequence = currentFlow.Sequence,
                        Action = answer,
                        Remarks = reason
                    };
                    _dbContext.ApplyProductFlowLogs.Add(newFlowLog);
                    _dbContext.SaveChanges();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError("事務失敗[AnswerFlowInTransactionScope]：{msg}", ex);
                    return false;
                }
            }

            if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase == null)
            {
                string title = $"申請新品項單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 需要您處理";
                string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
                SendMailToOwner(title, content, ownerList);

            }
            if (answer == CommonConstants.AnswerPurchaseFlow.AGREE && nextPurchase != null)
            {
                string title = $"申請新品項單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 需要您審核";
                string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
                using (var scope2 = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        title = "申請新品項單單需要審核";
                        var purchaseNumber = string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5));
                        var receiver = _memberService.GetMembersByUserId(nextPurchase.ReviewUserId);
                        EmailNotify emailNotify = new EmailNotify()
                        {
                            Title = title,
                            Content = content,
                            UserId = receiver.UserId,
                            Email = receiver.Email,
                            PurchaseNumber = purchaseNumber,
                            Type = CommonConstants.EmailNotifyType.APPLY_NEW_PRODUCT
                        };
                        _emailService.AddEmailNotify(emailNotify);
                        _dbContext.SaveChanges();
                        scope2.Complete();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("事務失敗[AnswerFlowInTransactionScope]：{msg}", ex);
                        return false;
                    }
                }
            }
            // 發送郵件通知
            //SendNotificationEmails(answer, applyNewProductMain, preFlow, nextPurchase, currentFlow, isOwner,ownerList);
            return true;
        }
       

        private async Task SendMailByFlowSetting(ApplyProductFlowSetting applyProductFlowSetting, String title, String content)
        {
            var receiver = _memberService.GetMembersByUserId(applyProductFlowSetting.ReviewUserId);
            if (receiver != null)
            {

                if (!string.IsNullOrEmpty(receiver.Email))
                {
                    await _emailService.SendAsync(title, content, receiver.Email);
                    _logger.LogInformation("[寄信]標題:{title},收件者:{email}", title, receiver.Email);
                }
                    
            }
        }

        private async Task SendMailByFlow(ApplyNewProductFlow flow, String title, String content)
        {
            var receiver = _memberService.GetMembersByUserId(flow.ReviewUserId);
            if (receiver != null)
            {

                if (!string.IsNullOrEmpty(receiver.Email))
                {
                    await _emailService.SendAsync(title, content, receiver.Email);
                    _logger.LogInformation("[寄信]標題:{title},收件者:{email}", title, receiver.Email);
                }
                    
            }
        }

        private async Task SendMailByMain(ApplyNewProductMain main, String title, String content)
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
        }

        private void SendMailToOwner(String title, String content,List<WarehouseMember> ownerList)
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

        private void SendNotificationEmails(string answer, ApplyNewProductMain applyNewProductMain, ApplyNewProductFlow? preFlow, ApplyNewProductFlow? nextPurchase, ApplyNewProductFlow currentFlow, bool? isOwner,List<WarehouseMember> ownerList)
        {
            if (answer == CommonConstants.AnswerApplyNewProductFlow.AGREE && nextPurchase == null)
            {
                string title = $"申請新品項單據:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 需要您處理";
                string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
                SendMailToOwner(title, content, ownerList);
            }
            if (answer == CommonConstants.AnswerApplyNewProductFlow.AGREE && nextPurchase != null)
            {
                string title = $"申請新品項單據:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 需要您審核";
                string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
                SendMailByFlow(nextPurchase, title, content);
            }
            //if (answer == CommonConstants.AnswerApplyNewProductFlow.REJECT)
            //{
            //    string title = $"申請新品項單據:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 已被拒絕";
            //    string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
            //    SendMailByMain(applyNewProductMain, title, content);
            //}
            if (answer == CommonConstants.AnswerApplyNewProductFlow.BACK && isOwner != true)
            {
                if (preFlow != null)
                {
                    string title = $"申請新品項單據:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 需要您審核";
                    string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
                    SendMailByFlow(preFlow, title, content);
                }
                else
                {
                    //string title = $"申請新品項單據:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 已被退回";
                    //string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
                    //SendMailByMain(applyNewProductMain, title, content);
                }
            }
            //if (answer == CommonConstants.AnswerPurchaseFlow.BACK && isOwner == true)
            //{
            //    string title = $"申請新品項單據:{string.Concat(DateTimeHelper.FormatDateStringForEmail(applyNewProductMain.CreatedAt), applyNewProductMain.ApplyId.AsSpan(0, 5))} 已被退回";
            //    string content = $"<a href={_smtpSettings.Domain}/product_item_verify/{applyNewProductMain.ApplyId}>{applyNewProductMain.ApplyId}</a>";
            //    SendMailByFlow(currentFlow, title, content);
            //}
        }

        public void UpdateApplyNewProductToDone(ApplyNewProductMain main)
        {
            main.CurrentStatus = CommonConstants.ApplyNewProductCurrentStatus.DONE;
            _dbContext.SaveChanges();
        }
    }
}
