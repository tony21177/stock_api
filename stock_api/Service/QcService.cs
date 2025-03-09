using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Settings;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Transactions;

namespace stock_api.Service
{
    public class QcService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<QcService> _logger;
        private readonly SmtpSettings _smtpSettings;
        private readonly EmailService _emailService;
        private readonly MemberService _memberService;

        public QcService(StockDbContext dbContext, IMapper mapper, ILogger<QcService> logger, EmailService emailService, MemberService memberService, SmtpSettings smtpSettings)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _memberService = memberService;
            _smtpSettings = smtpSettings;
        }


        public (List<UnDoneQcLot>,int) ListUnDoneQcLotList(ListUnDoneQcLotRequest request)
        {
            var needQcProductList = _dbContext.WarehouseProducts.Where(p=>p.IsActive==true&&p.IsNeedAcceptProcess==true&&p.QcType!=CommonConstants.QcTypeConstants.NONE).ToList();    
            var needQcProductIdList = needQcProductList.Select(p=>p.ProductId).ToList();
            var unDoneLotNumberQcInStockRecords = _dbContext.InStockItemRecords.Where(i=>i.CompId== request.CompId && i.QcTestStatus==CommonConstants.QcTestStatus.NONE
            &&i.IsNeedQc==true&&i.QcType!=CommonConstants.QcTypeConstants.NONE).ToList();
            unDoneLotNumberQcInStockRecords = unDoneLotNumberQcInStockRecords.Where(r => needQcProductIdList.Contains(r.ProductId)).ToList();
            var unDoneLotNumberQcInStockIdList = unDoneLotNumberQcInStockRecords.Select(r => r.InStockId).ToList();
            List<UnDoneQcLot> unDoneQcLotList = new();
            foreach (var inStockItemRecord in unDoneLotNumberQcInStockRecords)
            {
                var matchedProduct = needQcProductList.Where(p=>p.ProductId == inStockItemRecord.ProductId).FirstOrDefault();
                var unDoneQcLot = new UnDoneQcLot()
                {
                    ProductId = inStockItemRecord.ProductId,
                    ProductCode = inStockItemRecord.ProductCode,
                    ProductName = inStockItemRecord.ProductName,
                    LotNumber = inStockItemRecord.LotNumber,
                    LotNumberBatch = inStockItemRecord.LotNumberBatch,
                    QcType = matchedProduct.QcType,
                    QcTestStatus = inStockItemRecord.QcTestStatus,
                    ProductModel = matchedProduct.ProductModel,
                    InStockTime = inStockItemRecord.CreatedAt,
                    InStockUserId = inStockItemRecord.UserId,
                    InStockUserName = inStockItemRecord.UserName,
                    GroupIdList = matchedProduct.GroupIds==null?null:matchedProduct.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                    GroupNameList = matchedProduct.GroupNames == null ? null : matchedProduct.GroupNames.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                };
                unDoneQcLotList.Add(unDoneQcLot);
            }
            var newLotNumberInStockIdList = _dbContext.ProductNewLotnumberViews.ToList().Where(e=>e.LotNumber!= "N/A"&&e.CompId== request.CompId).ToList().Select(e=>e.InStockId).ToList();
            
            var newLotNumberInStockItems = _dbContext.InStockItemRecords.Where(i=>newLotNumberInStockIdList.Contains(i.InStockId)&& i.QcTestStatus == CommonConstants.QcTestStatus.NONE).ToList();
            var allProducts = _dbContext.WarehouseProducts.ToList();
            foreach (var inStockItemRecord in newLotNumberInStockItems)
            {
                if (unDoneLotNumberQcInStockIdList.Contains(inStockItemRecord.InStockId)) continue;
                var matchedProduct = allProducts.Where(p => p.ProductId == inStockItemRecord.ProductId).FirstOrDefault();
                if(matchedProduct.IsNeedAcceptProcess==false||matchedProduct.QcType==CommonConstants.QcTypeConstants.NONE) continue;
                var unDoneQcLot = new UnDoneQcLot()
                {
                    ProductId = inStockItemRecord.ProductId,
                    ProductCode = inStockItemRecord.ProductCode,
                    ProductName = inStockItemRecord.ProductName,
                    LotNumber = inStockItemRecord.LotNumber,
                    LotNumberBatch = inStockItemRecord.LotNumberBatch,
                    QcType = matchedProduct.QcType,
                    QcTestStatus = inStockItemRecord.QcTestStatus,
                    ProductModel = matchedProduct.ProductModel,
                    InStockTime = inStockItemRecord.CreatedAt,
                    AcceptedAt = inStockItemRecord.CreatedAt,
                    InStockUserId = inStockItemRecord.UserId,
                    InStockUserName = inStockItemRecord.UserName,
                    GroupIdList = matchedProduct.GroupIds == null ? null : matchedProduct.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                    GroupNameList = matchedProduct.GroupNames == null ? null : matchedProduct.GroupNames.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                };
                unDoneQcLotList.Add(unDoneQcLot);
            }

            if (request.GroupId != null)
            {
                unDoneQcLotList = unDoneQcLotList.Where(e=> e.GroupIdList!=null&& string.Join(" ",e.GroupIdList).Contains(request.GroupId)).ToList();
            }
            if (request.Keywords != null)
            {
                unDoneQcLotList = unDoneQcLotList.Where(e=>e.IsContainKeywords(request.Keywords)).ToList();
            }


            string? orderByField = request.PaginationCondition.OrderByField;
            if (orderByField == null)
            {
                orderByField = "AcceptedAt";
            }
            orderByField = StringUtils.CapitalizeFirstLetter(orderByField);
            if (request.PaginationCondition.IsDescOrderBy)
            {
                switch (orderByField)
                {
                    case "ProductName":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.ProductName).ToList();
                        break;
                    case "ProductCode":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.ProductCode).ToList();
                        break;
                    case "LotNumber":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.LotNumber).ToList();
                        break;
                    case "LotNumberBatch":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.LotNumberBatch).ToList();
                        break;
                    case "QcType":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.QcType).ToList();
                        break;
                    case "PurchaseMainId":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.PurchaseMainId).ToList();
                        break;
                    case "AcceptUserName":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.AcceptUserName).ToList();
                        break;
                    case "AcceptedAt":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.AcceptedAt).ToList();
                        break;
                    case "InStockTime":
                        unDoneQcLotList = unDoneQcLotList.OrderByDescending(item => item.AcceptedAt).ToList();
                        break;
                }
            }
            else
            {
                switch (orderByField)
                {
                    case "ProductName":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.ProductName).ToList();
                        break;
                    case "ProductCode":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.ProductCode).ToList();
                        break;
                    case "LotNumber":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.LotNumber).ToList();
                        break;
                    case "LotNumberBatch":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.LotNumberBatch).ToList();
                        break;
                    case "QcType":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.QcType).ToList();
                        break;
                    case "PurchaseMainId":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.PurchaseMainId).ToList();
                        break;
                    case "AcceptUserName":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.AcceptUserName).ToList();
                        break;
                    case "AcceptedAt":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.AcceptedAt).ToList();
                        break;
                    case "InStockTime":
                        unDoneQcLotList = unDoneQcLotList.OrderBy(item => item.AcceptedAt).ToList();
                        break;
                }
            }
            int totalPages = 0;
            int totalItems = unDoneQcLotList.Count;
            totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);
            unDoneQcLotList = unDoneQcLotList.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize).ToList();

            return (unDoneQcLotList,totalPages);
        }

        public (bool,string?) CreateQcValidation(QcValidationMain newQcValidationMain,List<QcValidationDetail> newQcValidationDetailList
            , List<QcAcceptanceDetail> newQcAcceptanceDetail, List<QcValidationFlowSettingVo> qcValidationFlowSettings)
        {
            using var scope = new TransactionScope();
            try
            {
                _dbContext.QcValidationMains.Add(newQcValidationMain);
                _dbContext.QcValidationDetails.AddRange(newQcValidationDetailList);
                _dbContext.QcAcceptanceDetails.AddRange(newQcAcceptanceDetail);
                List<QcFlow> qcFlows = new List<QcFlow>();
                DateTime submitedAt = DateTime.Now;
                foreach (var flow in qcValidationFlowSettings)
                {
                    qcFlows.Add(new QcFlow
                    {
                        FlowId = Guid.NewGuid().ToString(),
                        CompId = newQcValidationMain.CompId,
                        MainId = newQcValidationMain.MainId,
                        Status = CommonConstants.QcFlowStatus.WAIT,
                        ReviewCompId = newQcValidationMain.CompId,
                        ReviewUserId = flow.ReviewUserId,
                        ReviewUserName = flow.ReviewUserName,
                        ReviewGroupId = flow.ReviewGroupId,
                        ReviewGroupName = flow.ReviewGroupName,
                        Answer = CommonConstants.PurchaseFlowAnswer.EMPTY,
                        Sequence = flow.Sequence,
                        SubmitAt = submitedAt,
                    });
                }
                _dbContext.QcFlows.AddRange(qcFlows);
                var firstFlow = qcFlows.OrderBy(s => s.Sequence).FirstOrDefault();
                DateTime now = DateTime.Now;
                string title = $"品質確效單:{string.Concat(DateTimeHelper.FormatDateStringForEmail(now), firstFlow.MainId)} 需要您審核";
                string content = $"<a href={_smtpSettings.Domain}/qc_detail/{firstFlow.MainId}>{firstFlow.MainId}</a>";
                SendMailByFlowSetting(firstFlow, title, content);

                if (firstFlow != null)
                {
                    title = "品質確效單需要審核";
                    var receiver = _memberService.GetMembersByUserId(firstFlow.ReviewUserId);
                    EmailNotify emailNotify = new EmailNotify()
                    {
                        Title = title,
                        Content = content,
                        UserId = firstFlow.ReviewUserId,
                        Email = receiver.Email,
                        PurchaseNumber = firstFlow.MainId,
                        Type = CommonConstants.EmailNotifyType.QC
                    };
                    _emailService.AddEmailNotify(emailNotify);
                }


                if (newQcValidationMain.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER)
                {
                    // 更新QcTestStatus成DONE表示已做過確效
                    _dbContext.InStockItemRecords.Where(i => i.IsNeedQc == true && i.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER && i.LotNumber == newQcValidationMain.LotNumber)
                        .ExecuteUpdate(item => item.SetProperty(x=>x.QcTestStatus,CommonConstants.QcTestStatus.DONE));
                }
                if (newQcValidationMain.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH)
                {
                    _dbContext.InStockItemRecords.Where(i => i.IsNeedQc == true && i.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH && i.LotNumberBatch == newQcValidationMain.LotNumberBatch)
                        .ExecuteUpdate(item => item.SetProperty(x => x.QcTestStatus, CommonConstants.QcTestStatus.DONE));
                }

                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[CreateQcValidation]：{msg}", ex);
                return (false, ex.Message);
            }

        }

        public (List<QcValidationMain>,int) ListQcMain(ListMainWithDetailRequest request) {
            IQueryable<QcValidationMain> query = _dbContext.QcValidationMains;
            if (request.CompId != null)
            {
                query = query.Where(h => h.CompId == request.CompId);
            }
            if (request.MainId != null)
            {
                query = query.Where(h => h.MainId == request.MainId);
            }
            if (request.PurchaseMainId != null)
            {
                query = query.Where(h => h.PurchaseMainId == request.PurchaseMainId);
            }
            if (request.InStockId != null)
            {
                query = query.Where(h => h.InStockId == request.InStockId);
            }
            if (request.QcStartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(request.QcStartDate);
                query = query.Where(h => h.CreatedAt >= startDateTime);
            }
            if (request.QcEndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.QcEndDate).Value.AddDays(1);
                query = query.Where(h => h.CreatedAt < endDateTime);
            }
            if (request.LotNumber != null)
            {
                query = query.Where(h => h.LotNumber == request.LotNumber);
            }
            if (request.LotNumberBatch != null)
            {
                query = query.Where(h => h.LotNumberBatch == request.LotNumberBatch);
            }
            if (request.QcType != null)
            {
                query = query.Where(h => h.QcType == request.QcType);
            }
            if (!string.IsNullOrEmpty(request.Keywords))
            {
                query = query.Where(h => 
                h.MainId.Contains(request.MainId)
                || h.PurchaseMainId.Contains(request.Keywords)
                || h.PurchaseSubItemId.Contains(request.Keywords)
                || h.InStockId.Contains(request.Keywords)
                || h.InStockUserName.Contains(request.Keywords)
                || h.ProductCode.Contains(request.Keywords)
                || h.ProductName.Contains(request.Keywords)
                || h.ProductSpec.Contains(request.Keywords)
                || h.LotNumber.Contains(request.Keywords)
                || h.LotNumberBatch.Contains(request.Keywords)
                || h.ValidationType.Contains(request.Keywords)
                || h.ValidationMethod.Contains(request.Keywords)
                || h.ValidationItemName.Contains(request.Keywords)
                || h.Comment.Contains(request.Keywords)
                || h.QcType.Contains(request.Keywords)
                );
            }
            int totalPages = 0;
            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "CreatedAt";
            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "AcceptedAt" => query.OrderByDescending(h => h.InStockTime),
                    "ProductCode" => query.OrderByDescending(h => h.ProductCode),
                    "LotNumber" => query.OrderByDescending(h => h.LotNumber),
                    "LotNumberBatch" => query.OrderByDescending(h => h.LotNumberBatch),
                    "ValidationType" => query.OrderByDescending(h => h.ValidationType),
                    "ValidationMethod" => query.OrderByDescending(h => h.ValidationMethod),
                    "ValidationItemName" => query.OrderByDescending(h => h.ValidationItemName),
                    "QcType" => query.OrderByDescending(h => h.QcType),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.CreatedAt),
                };
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "AcceptedAt" => query.OrderBy(h => h.InStockTime),
                    "ProductCode" => query.OrderBy(h => h.ProductCode),
                    "LotNumber" => query.OrderBy(h => h.LotNumber),
                    "LotNumberBatch" => query.OrderBy(h => h.LotNumberBatch),
                    "ValidationType" => query.OrderBy(h => h.ValidationType),
                    "ValidationMethod" => query.OrderBy(h => h.ValidationMethod),
                    "ValidationItemName" => query.OrderBy(h => h.ValidationItemName),
                    "QcType" => query.OrderBy(h => h.QcType),
                    "CreatedAt" => query.OrderBy(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderBy(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.CreatedAt),
                };
            }
            int totalItems = query.Count();
            totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);
            query = query.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);
        }

        public List<QcValidationDetail> GetQcDetailsByMainIdList(List<string> mainIdList)
        {
            return _dbContext.QcValidationDetails.Where(d => mainIdList.Contains(d.MainId)).ToList();
        }

        public List<QcAcceptanceDetail> GetQcAcceptanceDetailsByMainIdList(List<string> mainIdList)
        {
            return _dbContext.QcAcceptanceDetails.Where(d => mainIdList.Contains(d.MainId)).ToList();
        }


        private async Task SendMailByFlowSetting(QcFlow qcFlow, String title, String content)
        {
            var receiver = _memberService.GetMembersByUserId(qcFlow.ReviewUserId);
            if (receiver != null)
            {

                if (!string.IsNullOrEmpty(receiver.Email))
                {
                    await _emailService.SendAsync(title, content, receiver.Email);
                    _logger.LogInformation("[寄信]標題:{title},收件者:{email}", title, receiver.Email);
                }

            }
        }

        public List<QcFlowWithAgentsVo> GetQcFlowListWithAgentsByMainIdList(List<string> mainIdList)
        {
            var result = from f in _dbContext.QcFlows
                         join m in _dbContext.WarehouseMembers on f.ReviewUserId equals m.UserId
                         where mainIdList.Contains(f.MainId)
                         select new QcFlowWithAgentsVo
                         {
                             FlowId = f.FlowId,
                             CompId = f.CompId,
                             MainId = f.MainId,
                             Reason = f.Reason,
                             Status = f.Status,
                             ReviewCompId = f.ReviewCompId,
                             ReviewUserId = f.ReviewUserId,
                             ReviewUserName = f.ReviewUserName,
                             ReviewGroupId = f.ReviewGroupId,
                             ReviewGroupName = f.ReviewGroupName,
                             Answer = f.Answer,
                             Sequence = f.Sequence,
                             ReadAt = f.ReadAt,
                             SubmitAt = f.SubmitAt,
                             CreatedAt = f.CreatedAt,
                             UpdatedAt =f.UpdatedAt,
                             Agents = m.Agents,
                             AgentNames = m.AgentNames,
                         };

            var list = result.ToList();
            list.ForEach(flow =>
            {
                if (!flow.Agents.IsNullOrEmpty())
                {
                    flow.ReviewAgentIds = flow.Agents.Split(",", StringSplitOptions.None).ToList();
                    flow.ReviewAgentNames = flow.AgentNames.Split(",", StringSplitOptions.None).ToList();
                }


            });
            return list;
        }


        public List<QcFlowLog> GetQcFlowLogsByMainId(string mainId)
        {
            return _dbContext.QcFlowLogs.Where(l => l.MainId== mainId).ToList();
        }

        public List<QcValidationMain> GetQcMainsByMainId(string mainId)
        {
            return _dbContext.QcValidationMains.Where(m => m.MainId==mainId).ToList();
        }

        public List<QcFlowLog> GetQcFlowLogsByMainIdList(List<string> mainIdList)
        {
            return _dbContext.QcFlowLogs.Where(l => mainIdList.Contains(l.MainId)).ToList();
        }

        public List<QcValidationMain> GetQcMainsByMainIdList(List<string> mainIdList)
        {
            return _dbContext.QcValidationMains.Where(m => mainIdList.Contains(m.MainId)).ToList();
        }

        public List<QcFlow> GetFlowsByUserId(string userId)
        {
            return _dbContext.QcFlows.Where(f => f.ReviewUserId == userId).ToList();

        }
    }
}
