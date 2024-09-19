using AutoMapper;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Linq;
using System.Security.AccessControl;
using System.Transactions;

namespace stock_api.Service
{
    public class StockInService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<StockInService> _logger;

        public StockInService(StockDbContext dbContext, IMapper mapper, ILogger<StockInService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public List<PurchaseAcceptanceItemsView> SearchPurchaseAcceptanceItems(SearchPurchaseAcceptItemRequest request)
        {
            IQueryable<PurchaseAcceptanceItemsView> query = _dbContext.PurchaseAcceptanceItemsViews;

            if (request.ReceiveStatus != null)
            {
                query = query.Where(h => request.ReceiveStatus == h.ReceiveStatus);
            }
            if (request.ReceiveStatusList != null)
            {
                query = query.Where(h => request.ReceiveStatusList.Contains(h.ReceiveStatus));
            }
            if (request.InStockStatusList != null)
            {
                query = query.Where(h => h.InStockStatus != null && request.InStockStatusList.Contains(h.InStockStatus));
            }
            if (request.PurchaseMainId != null)
            {
                query = query.Where(h => h.PurchaseMainId == request.PurchaseMainId);
            }
            //if (request.DemandDateStart != null)
            //{
            //    DateOnly startDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.DemandDateStart).Value);
            //    query = query.Where(h => h.DemandDate >= startDate);
            //}
            //if (request.DemandDateEnd != null)
            //{
            //    DateOnly endDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.DemandDateStart).Value).AddDays(1);
            //    query = query.Where(h => h.DemandDate < endDate);
            //}
            if (request.DemandDateStart != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(request.DemandDateStart).Value;
                query = query.Where(h => h.DemandDate.Value.ToDateTime(new TimeOnly(0, 0)) >= startDateTime);
            }
            if (request.DemandDateEnd != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.DemandDateEnd).Value.AddDays(1);
                query = query.Where(h => h.DemandDate.Value.ToDateTime(new TimeOnly(0, 0)) < endDateTime);
            }
            if (request.VerifyAtStart != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(request.VerifyAtStart).Value;
                query = query.Where(h => h.VerifyAt >= startDateTime);
            }
            if (request.VerifyAtEnd != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.VerifyAtEnd).Value.AddDays(1);
                query = query.Where(h => h.VerifyAt < endDateTime);
            }
            if (request.ApplyDateStart != null)
            {
                query = query.Where(h => h.ApplyDate >= DateTimeHelper.ParseDateString(request.ApplyDateStart).Value);
            }
            if (request.ApplyDateEnd != null)
            {
                DateTime endDateTime = DateTimeHelper.ParseDateString(request.ApplyDateEnd).Value.AddDays(1);
                query = query.Where(h => h.ApplyDate < endDateTime);
            }
            // 是要過濾SubItems的
            //if (request.GroupId != null)
            //{
            //    query = query.Where(h => h.GroupIds.Contains(request.GroupId));
            //}
            if (request.Type != null)
            {
                query = query.Where(h => h.Type == request.Type);
            }
            query = query.Where(h => h.CompId == request.CompId);


            return query.ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemsByAccepIdList(List<string> acceptIdList, string compId)
        {
            return _dbContext.AcceptanceItems.Where(ai => ai.CompId == compId && acceptIdList.Contains(ai.AcceptId)).ToList();
        }



        public (bool, string?, Qc?) UpdateAcceptItem(PurchaseMainSheet purchaseMain, PurchaseSubItem purchaseSubItem, AcceptanceItem existingAcceptanceItem, UpdateAcceptItemRequest updateAcceptItem, WarehouseProduct product, string compId, WarehouseMember acceptMember,bool isDirectOutStock)
        {
            using var scope = new TransactionScope();
            try
            {
                Qc? qc = new();
                var existingInStockRecord = _dbContext.InStockItemRecords.Where(i => i.ItemId == existingAcceptanceItem.ItemId).OrderByDescending(i => i.CreatedAt).FirstOrDefault();

                if (updateAcceptItem.AcceptQuantity.HasValue)
                {
                    float existingAcceptQty = existingAcceptanceItem.AcceptQuantity ?? 0;
                    existingAcceptanceItem.AcceptQuantity = updateAcceptItem.AcceptQuantity.Value + existingAcceptQty;
                }
                if (updateAcceptItem.AcceptUserId != null)
                {
                    existingAcceptanceItem.AcceptUserId = updateAcceptItem.AcceptUserId;
                }
                if (updateAcceptItem.LotNumber != null && existingInStockRecord == null)
                {
                    existingAcceptanceItem.LotNumber = updateAcceptItem.LotNumber;
                }
                //var now = DateTime.Now;
                //var nowDateTimeString = DateTimeHelper.FormatDateString(now, "yyyyMMddHHmm");
                //existingAcceptanceItem.LotNumberBatch = $"{product.ProductCode}{nowDateTimeString}";
                if (existingInStockRecord == null)
                {
                    existingAcceptanceItem.LotNumberBatch = existingAcceptanceItem.LotNumberBatchSeq.ToString("D12");
                }
                if (updateAcceptItem.ExpirationDate != null && existingInStockRecord == null)
                {
                    existingAcceptanceItem.ExpirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(updateAcceptItem.ExpirationDate).Value);
                }
                if (updateAcceptItem.PackagingStatus != null)
                {
                    existingAcceptanceItem.PackagingStatus = updateAcceptItem.PackagingStatus;
                }
                if (updateAcceptItem.QcStatus != null)
                {
                    existingAcceptanceItem.QcStatus = updateAcceptItem.QcStatus;
                }
                if (existingAcceptanceItem.AcceptQuantity != null && existingAcceptanceItem.QcStatus != CommonConstants.QcStatus.FAIL)
                {
                    existingAcceptanceItem.CurrentTotalQuantity = product.InStockQuantity + existingAcceptanceItem.AcceptQuantity; // 驗收入庫後，當下該品項的總庫存數量
                }

                if (updateAcceptItem.Comment != null)
                {
                    existingAcceptanceItem.Comment = updateAcceptItem.Comment;
                }
                if (updateAcceptItem.DeliverFunction != null)
                {
                    existingAcceptanceItem.DeliverFunction = updateAcceptItem.DeliverFunction;
                }
                if (updateAcceptItem.DeliverTemperature != null)
                {
                    existingAcceptanceItem.DeliverTemperature = updateAcceptItem.DeliverTemperature;
                }
                if (updateAcceptItem.SavingFunction != null)
                {
                    existingAcceptanceItem.SavingFunction = updateAcceptItem.SavingFunction;
                }
                if (updateAcceptItem.SavingTemperature != null)
                {
                    existingAcceptanceItem.SavingTemperature = updateAcceptItem.SavingTemperature;
                }

                // 判斷是否全部驗收完
                if (existingAcceptanceItem.AcceptQuantity != null && existingAcceptanceItem.AcceptQuantity >= existingAcceptanceItem.OrderQuantity)
                {
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                    existingAcceptanceItem.VerifyAt = DateTime.Now;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                    purchaseSubItem.InStockQuantity = existingAcceptanceItem.AcceptQuantity;
                }
                else if (existingAcceptanceItem.AcceptQuantity != null && existingAcceptanceItem.AcceptQuantity > 0 && existingAcceptanceItem.AcceptQuantity < existingAcceptanceItem.OrderQuantity)
                {
                    // 判斷是否部分驗收
                    _logger.LogInformation("[品項部分驗收] AcceptId:${acceptId},AcceptQuantity:${AcceptQuantity},OrderQuantity:${}", existingAcceptanceItem.AcceptId, existingAcceptanceItem.AcceptQuantity, existingAcceptanceItem.OrderQuantity);
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                    existingAcceptanceItem.VerifyAt = DateTime.Now;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                    purchaseSubItem.InStockQuantity = existingAcceptanceItem.AcceptQuantity;
                }

                var lotNumberBatch = existingAcceptanceItem.LotNumberBatch;
                if (existingInStockRecord != null)
                {
                    var inStockLotNumberBatch = existingInStockRecord.LotNumberBatch;
                    if (inStockLotNumberBatch.Contains('-'))
                    {
                        var batchSeq = inStockLotNumberBatch.Split("-", StringSplitOptions.None)[1];
                        var nextBatchSeq = batchSeq + 1;
                        lotNumberBatch = lotNumberBatch + "-" + nextBatchSeq;
                    }
                    else
                    {
                        lotNumberBatch += "-2";
                    }
                }

                if (updateAcceptItem.AcceptQuantity != null)
                {
                    

                    var tempInStockItemRecord = new TempInStockItemRecord()
                    {
                        InStockId = Guid.NewGuid().ToString(),
                        LotNumberBatch = lotNumberBatch,
                        LotNumber = updateAcceptItem.LotNumber,
                        CompId = compId,
                        OriginalQuantity = product.InStockQuantity.Value,
                        ExpirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(updateAcceptItem.ExpirationDate).Value),
                        InStockQuantity = updateAcceptItem.AcceptQuantity.Value,
                        ProductId = product.ProductId,
                        ProductCode = product.ProductCode,
                        ProductName = product.ProductName,
                        ProductSpec = product.ProductSpec,
                        Type = CommonConstants.StockInType.PURCHASE,
                        UserId = acceptMember.UserId,
                        UserName = acceptMember.DisplayName,
                        IsTransfer = true,
                        InventoryId = existingAcceptanceItem.PurchaseMainId,
                        DeliverFunction = updateAcceptItem.DeliverFunction,
                        DeliverTemperature = updateAcceptItem.DeliverTemperature,
                        SavingFunction = updateAcceptItem.SavingFunction,
                        SavingTemperature = updateAcceptItem.SavingTemperature,
                    };

                    var inStockItemRecord = new InStockItemRecord()
                    {
                        InStockId = Guid.NewGuid().ToString(),
                        LotNumberBatch = lotNumberBatch,
                        LotNumber = updateAcceptItem.LotNumber,
                        CompId = compId,
                        OriginalQuantity = product.InStockQuantity.Value,
                        ExpirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(updateAcceptItem.ExpirationDate).Value),
                        ItemId = existingAcceptanceItem.ItemId,
                        InStockQuantity = updateAcceptItem.AcceptQuantity.Value,
                        ProductId = product.ProductId,
                        ProductCode = product.ProductCode,
                        ProductName = product.ProductName,
                        ProductSpec = product.ProductSpec,
                        Type = CommonConstants.StockInType.PURCHASE,
                        BarCodeNumber = existingAcceptanceItem.LotNumberBatch,
                        UserId = acceptMember.UserId,
                        UserName = acceptMember.DisplayName,
                        AfterQuantity = product.InStockQuantity.Value + updateAcceptItem.AcceptQuantity.Value,
                        DeliverFunction = updateAcceptItem.DeliverFunction,
                        DeliverTemperature = updateAcceptItem.DeliverTemperature,
                        SavingFunction = updateAcceptItem.SavingFunction,
                        SavingTemperature = updateAcceptItem.SavingTemperature,
                        IsNeedQc = product.IsNeedAcceptProcess,
                        QcType = product.QcType,
                        QcTestStatus = CommonConstants.QcTestStatus.NONE,
                        PackagingStatus = updateAcceptItem.PackagingStatus,
                        Comment = updateAcceptItem.Comment,
                        QcComment = updateAcceptItem.QcComment,
                    };

                    if (isDirectOutStock)
                    {

                        var outStockId = Guid.NewGuid().ToString();
                        var outStockRecord = new OutStockRecord()
                        {
                            OutStockId = outStockId,
                            //AbnormalReason = request.AbnormalReason,
                            ApplyQuantity = updateAcceptItem.AcceptQuantity.Value,
                            LotNumber = updateAcceptItem.LotNumber,
                            LotNumberBatch = lotNumberBatch,
                            CompId = compId,
                            ExpirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(updateAcceptItem.ExpirationDate).Value),
                            IsAbnormal = false,
                            ProductId = product.ProductId,
                            ProductCode = product.ProductCode,
                            ProductName = product.ProductName,
                            ProductSpec = product.ProductSpec,
                            Type = CommonConstants.OutStockType.PURCHASE_OUT,
                            UserId = acceptMember.UserId,
                            UserName = acceptMember.DisplayName,
                            OriginalQuantity = product.InStockQuantity.Value,
                            AfterQuantity = product.InStockQuantity.Value,
                            ItemId = existingAcceptanceItem.ItemId,
                            BarCodeNumber = existingAcceptanceItem.LotNumberBatch,
                        };
                        _dbContext.OutStockRecords.Add(outStockRecord);

                        inStockItemRecord.OutStockStatus = CommonConstants.OutStockStatus.ALL;
                        inStockItemRecord.OutStockQuantity = updateAcceptItem.AcceptQuantity.Value;
                        var outStockRelateToInStock = new OutstockRelatetoInstock()
                        {
                            OutStockId = outStockId,
                            InStockId = inStockItemRecord.InStockId,
                            LotNumber = inStockItemRecord.LotNumber,
                            LotNumberBatch = lotNumberBatch,
                            Quantity = updateAcceptItem.AcceptQuantity.Value,
                        };
                        _dbContext.OutstockRelatetoInstocks.Add(outStockRelateToInStock);


                    }


                    if (inStockItemRecord.IsNeedQc == true)
                    {
                        if (inStockItemRecord.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER)
                        {
                            var lastInStockedRecord = _dbContext.InStockItemRecords.Where(i => i.LotNumber == inStockItemRecord.LotNumber).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                            if (lastInStockedRecord != null && (lastInStockedRecord.QcTestStatus == CommonConstants.QcTestStatus.DONE))
                            {
                                // 上一批同批號的已經檢驗pass,表示此批號已經QC過了,不需再QC
                                inStockItemRecord.QcTestStatus = CommonConstants.QcTestStatus.DONE;
                            }
                        }
                        if (inStockItemRecord.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH)
                        {
                            var lastInStockedRecord = _dbContext.InStockItemRecords.Where(i => i.LotNumberBatch == inStockItemRecord.LotNumberBatch).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                            if (lastInStockedRecord != null && (lastInStockedRecord.QcTestStatus == CommonConstants.QcTestStatus.DONE))
                            {
                                // 上一批同批次的已經檢驗pass,表示此批號已經QC過了,不需再QC
                                inStockItemRecord.QcTestStatus = CommonConstants.QcTestStatus.DONE;
                            }
                        }

                        qc.QcType = inStockItemRecord.QcType;
                        qc.IsNeedQc = inStockItemRecord.IsNeedQc.Value;
                        Lot lot = new()
                        {
                            LotNumber = inStockItemRecord.LotNumber,
                            LotNumberBatch = inStockItemRecord.LotNumberBatch,
                            ProductCode = inStockItemRecord.ProductCode,
                            ProductName = inStockItemRecord.ProductName,
                        };
                        qc.Lot = lot;

                    }


                    //更新庫存品項
                    if (isDirectOutStock == false) //入庫直接出庫等於庫存數量不變
                    {
                        product.InStockQuantity = inStockItemRecord.AfterQuantity;
                    }

                    //入庫直接出庫
                    if (isDirectOutStock == true) 
                    {
                        product.LotNumber = updateAcceptItem.LotNumber;
                        product.LotNumberBatch = existingAcceptanceItem.LotNumberBatch;
                        DateOnly nowDate = DateOnly.FromDateTime(DateTime.Now);
                        if (product.OpenDeadline != null)
                        {
                            product.LastAbleDate = nowDate.AddDays(product.OpenDeadline.Value);
                        }
                        product.LastOutStockDate = nowDate;
                        product.OriginalDeadline = inStockItemRecord.ExpirationDate;
                    }
                    // 2024/09/19通常由金萬林設定
                    //product.DeliverFunction = updateAcceptItem.DeliverFunction;
                    //product.DeliverTemperature = updateAcceptItem.DeliverTemperature;
                    //product.SavingFunction = updateAcceptItem.SavingFunction;
                    //product.SavingTemperature = updateAcceptItem.SavingTemperature;
                    


                    _dbContext.TempInStockItemRecords.Add(tempInStockItemRecord);
                    _dbContext.InStockItemRecords.Add(inStockItemRecord);
                }
                //更新採購主單
                List<AcceptanceItem> allExistingAcceptanceItems = _dbContext.AcceptanceItems.Where(i => i.PurchaseMainId == purchaseMain.PurchaseMainId && i.CompId == compId).ToList();
                List<AcceptanceItem> otherExistingAcceptanceItems = allExistingAcceptanceItems.Where(i => i.AcceptId != existingAcceptanceItem.AcceptId).ToList();
                var acceptIds = otherExistingAcceptanceItems.Select(i => i.AcceptId).ToList();
                var inStockStatusList = otherExistingAcceptanceItems.Select(i => i.InStockStatus).ToList();

                Log.Information("other acceptId list:${acceptIds}", string.Join(",", acceptIds));
                Log.Information("other inStockStatusList list:${inStockStatusList}", string.Join(",", inStockStatusList));
                Log.Information("current update acceptId:${acceptId} ", existingAcceptanceItem.AcceptId);
                Log.Information("current update acceptId:${inStockStatus}", existingAcceptanceItem.InStockStatus);

                if (otherExistingAcceptanceItems.All(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE) && existingAcceptanceItem.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE)
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.ALL_ACCEPT;
                }
                else if (otherExistingAcceptanceItems.Any(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.PART || existingAcceptanceItem.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.PART))
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.PART_ACCEPT;
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null, qc);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateAccepItem]：{msg}", ex);
                return (false, ex.Message, null);
            }

        }

        public List<AcceptanceItem> AcceptanceItemsByUdiSerialCode(string udiserialCode, string compId)
        {
            return _dbContext.AcceptanceItems.Where(i => i.UdiserialCode == udiserialCode && i.CompId == compId).ToList();
        }

        public AcceptanceItem? GetAcceptanceItemByAcceptId(string acceptId)
        {
            return _dbContext.AcceptanceItems.Where(i => i.AcceptId == acceptId).FirstOrDefault();
        }
        public List<AcceptanceItem> GetAcceptanceItemByAcceptIdList(List<string> acceptIdList)
        {
            return _dbContext.AcceptanceItems.Where(i => acceptIdList.Contains(i.AcceptId)).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemByLotAndProductId(string lotNumber, string lotNumberBatch, string productId, string compId)
        {
            return _dbContext.AcceptanceItems.Where(i => i.LotNumber == lotNumber && i.LotNumberBatch == lotNumberBatch && i.ProductId == productId && i.CompId == compId).OrderBy(i => i.CreatedAt).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemNotAllInStockByProductCodeAndCompId(string productCode, string compId)
        {
            return _dbContext.AcceptanceItems.Where(i => i.ProductCode == productCode && i.CompId == compId && i.InStockStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE).OrderBy(i => i.CreatedAt).ToList();
        }

        public (List<InStockItemRecord>, int TotalPages) ListStockInRecords(ListStockInRecordsRequest request)
        {
            IQueryable<InStockItemRecord> query = _dbContext.InStockItemRecords;
            if (request.LotNumberBatch != null)
            {
                query = query.Where(h => h.LotNumberBatch == request.LotNumberBatch);
            }
            if (request.LotNumber != null)
            {
                query = query.Where(h => h.LotNumber == request.LotNumber);
            }
            if (request.ItemId != null)
            {
                query = query.Where(h => h.ItemId == request.ItemId);
            }
            if (request.ProductId != null)
            {
                query = query.Where(h => h.ProductId == request.ProductId);
            }
            if (request.ProductName != null)
            {
                query = query.Where(h => h.ProductName == request.ProductName);
            }
            if (request.UserId != null)
            {
                query = query.Where(h => h.UserId == request.UserId);
            }
            if (request.Type != null)
            {
                query = query.Where(h => h.Type == request.Type);
            }
            if (request.OutStockStatusList != null && request.OutStockStatusList.Count > 0)
            {
                query = query.Where(h => request.OutStockStatusList.Contains(h.OutStockStatus));
            }
            if (request.ProductCode != null)
            {
                query = query.Where(h => h.ProductCode == request.ProductCode);
            }
            if (request.StartDate != null)
            {
                query = query.Where(h =>  h.CreatedAt >= DateTimeHelper.ParseDateString(request.StartDate).Value);
            }
            if (request.EndDate != null)
            {
                DateTime endDateTime = DateTimeHelper.ParseDateString(request.EndDate).Value.AddDays(1);
                query = query.Where(h => h.CreatedAt < endDateTime);
            }
            if (request.SupplierId != null)
            {
                query = query.Where(h => h.SupplierId == request.SupplierId);
            }

            query = query.Where(h => h.CompId == request.CompId);

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var groupNameList =
                query = query.Where(h => h.LotNumberBatch.Contains(request.Keywords)
                || h.LotNumber.Contains(request.Keywords)
                || h.DeliverFunction.Contains(request.Keywords)
                || h.ProductId.Contains(request.Keywords)
                || h.ProductCode.Contains(request.ProductCode)
                || h.ProductName.Contains(request.Keywords)
                || h.ProductSpec.Contains(request.Keywords)
                || h.UserId.Contains(request.Keywords)
                || h.UserName.Contains(request.Keywords)
                || h.DeliverFunction.Contains(request.Keywords)
                || h.SavingFunction.Contains(request.Keywords));
            }
            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "UpdatedAt";

            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "LotNumberBatch" => query.OrderByDescending(h => h.LotNumberBatch),
                    "LotNumber" => query.OrderByDescending(h => h.LotNumber),
                    "ExpirationDate" => query.OrderByDescending(h => h.ExpirationDate),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.UpdatedAt),
                };
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "LotNumberBatch" => query.OrderBy(h => h.LotNumberBatch),
                    "LotNumber" => query.OrderBy(h => h.LotNumber),
                    "ExpirationDate" => query.OrderBy(h => h.ExpirationDate),
                    "CreatedAt" => query.OrderBy(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderBy(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.UpdatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);

            query = query.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);

        }

        public List<InStockItemRecord> GetInStockRecordsHistory(string prodcutId, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.ProductId == prodcutId).ToList();
        }

        public InStockItemRecord? GetInStockRecordByLotNumberBatch(string lotNumberBatch, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumberBatch == lotNumberBatch).FirstOrDefault();
        }

        public List<InStockItemRecord> GetInStockRecordListByLotNumber(string lotNumber, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumber == lotNumber).ToList();
        }

        public List<InStockItemRecord> GetInStockRecordByLotNumberBatchList(List<string> lotNumberBatchList, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && lotNumberBatchList.Contains(record.LotNumberBatch)).ToList();
        }

        public List<InStockItemRecord> GetInStockRecordsHistoryByLotNumberBatch(string lotNumberBatch, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumberBatch == lotNumberBatch).ToList();
        }

        public List<InStockItemRecord> GetInStockRecordsByInStockIdList(List<string> inStockIdList)
        {
            return _dbContext.InStockItemRecords.Where(record => inStockIdList.Contains(record.InStockId)).ToList();
        }
        public List<InStockItemRecord> GetInStockRecordsByItemIdList(List<string> itemIdList)
        {
            return _dbContext.InStockItemRecords.Where(record => record.ItemId!=null&&itemIdList.Contains(record.ItemId)).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemsByInIdList(List<string> idList)
        {
            return _dbContext.AcceptanceItems.Where(record => idList.Contains(record.AcceptId)).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemsByItemIdList(List<string> itemIdList)
        {
            return _dbContext.AcceptanceItems.Where(record => itemIdList.Contains(record.ItemId)).ToList();
        }

        //public List<InStockItemRecord> GetProductInStockRecordsHistoryNotAllOutFIFO(string productCode, string compId)
        //{
        //    return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.ProductCode == productCode
        //    &&record.OutStockStatus!=CommonConstants.OutStockStatus.ALL).OrderBy(record=>record.CreatedAt).ToList();
        //}


        public List<InStockItemRecord> GetProductInStockRecordsHistoryNotAllOutExpirationFIFO(string productCode, string compId)
        {
            // 先挑效期最早的相同的再依據FIFO
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.ProductCode == productCode
            && record.OutStockStatus != CommonConstants.OutStockStatus.ALL).OrderBy(record => record.ExpirationDate).ThenBy(record => record.CreatedAt).ToList();
        }

        public List<InStockItemRecord> GetProductInStockRecordsByAcceptId(string itemId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.ItemId == itemId).OrderBy(r => r.CreatedAt).ToList();
        }

        public List<string> GetDuplicateBatchList(List<string> lotNumberBatchList)
        {
            return _dbContext.InStockItemRecords.Where(record => lotNumberBatchList.Contains(record.LotNumberBatch)).Select(i => i.LotNumberBatch).ToList();
        }

        public (List<InStockItemRecord>, List<OutStockRecord>) GetAllInAndOutRecordByProductCodeList(List<string> productCodeList, string compId)
        {
            var inStockRecords = _dbContext.InStockItemRecords.Where(r => productCodeList.Contains(r.ProductCode) && r.CompId == compId).ToList();

            var outStockRecords = _dbContext.OutStockRecords.Where(r => productCodeList.Contains(r.ProductCode) && r.CompId == compId).ToList();
            return (inStockRecords, outStockRecords);
        }

        public (bool, string?) Return(OutStockRecord outStockRecord, WarehouseProduct product, WarehouseMember user, float returnQuantity)
        {
            using var scope = new TransactionScope();
            try
            {
                var inStockRecord = _dbContext.InStockItemRecords.Where(i=>i.LotNumberBatch==outStockRecord.LotNumberBatch).FirstOrDefault();

                var inStockQuantityBefore = inStockRecord.InStockQuantity;
                var outStockQuantityBefore = outStockRecord.ApplyQuantity;


                inStockRecord.InStockQuantity = inStockRecord.InStockQuantity + returnQuantity;
                inStockRecord.OutStockQuantity = inStockRecord.OutStockQuantity - returnQuantity;
                if(inStockRecord.OutStockQuantity - inStockRecord.OutStockQuantity > 0)
                {
                    inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.PART;
                }
                if(inStockRecord.OutStockQuantity == 0)
                {
                    inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.NONE;
                }
                if(inStockRecord.InStockQuantity - inStockRecord.OutStockQuantity == 0)
                {
                    inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.ALL;
                }
                inStockRecord.ReturnOutStockId = outStockRecord.OutStockId;

                outStockRecord.ApplyQuantity = outStockRecord.ApplyQuantity - returnQuantity;
                outStockRecord.IsReturned = true;

                product.InStockQuantity = product.InStockQuantity + returnQuantity;

                var returnStockRecord = new ReturnStockRecord()
                {
                    InStockId = inStockRecord.InStockId,
                    OutStockId = outStockRecord.OutStockId,
                    InStockQuantityBefore = inStockQuantityBefore,
                    InStockQuantityAfter = inStockRecord.InStockQuantity,
                    OutStockApplyQuantityBefore = outStockQuantityBefore,
                    OutStockApplyQuantityAfter = outStockRecord.ApplyQuantity,
                    LotNumberBatch = outStockRecord.LotNumberBatch,
                    LotNumber = outStockRecord.LotNumber,
                    CompId = outStockRecord.CompId, 
                    ProductId = product.ProductId,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    UserId = user.UserId,
                    UserName = user.DisplayName,
                };
                _dbContext.ReturnStockRecords.Add(returnStockRecord);
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);

            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[Return]：{msg}", ex);
                return (false, ex.Message);
            }
        }


        //public (bool, string?) Return(OutStockRecord outStockRecord, WarehouseProduct product, WarehouseMember user,float returnQuantity)
        //{
        //    using var scope = new TransactionScope();
        //    try
        //    {
        //        var sameLotNumberBatchLastOutStockRecord = _dbContext.OutStockRecords.Where(r => r.LotNumberBatch == outStockRecord.LotNumberBatch&&r.IsReturned==true)
        //            .OrderByDescending(i => i.CreatedAt).FirstOrDefault();
        //        InStockItemRecord? lastSameInstock = null; 
        //        if (sameLotNumberBatchLastOutStockRecord != null)
        //        {
        //            var lastOutStockRecordId = sameLotNumberBatchLastOutStockRecord.OutStockId;
        //            lastSameInstock = _dbContext.InStockItemRecords.Where(i=>i.ReturnOutStockId!=null&&i.ReturnOutStockId==lastOutStockRecordId).FirstOrDefault();
        //        }


        //        outStockRecord.IsReturned = true;
        //        var inStockLotNumberBatch = outStockRecord.LotNumberBatch + "-R1";
        //        if (lastSameInstock != null && lastSameInstock.LotNumberBatch.Contains("-R"))
        //        {
        //            var lotNumberBatchSplited = lastSameInstock.LotNumberBatch.Split("-R");
        //            var nowSeqString = lotNumberBatchSplited[1];
        //            var nowSeqInt = int.Parse(nowSeqString);
        //            inStockLotNumberBatch = sameLotNumberBatchLastOutStockRecord.LotNumberBatch.Split("-R")[0] + "-R" + (nowSeqInt + 1).ToString();
        //        }
        //        float afterQuantity = (product.InStockQuantity ?? 0) + outStockRecord.ApplyQuantity;

        //        var inStockRecord = new InStockItemRecord
        //        {
        //            InStockId = Guid.NewGuid().ToString(),
        //            LotNumberBatch = inStockLotNumberBatch,
        //            LotNumber = outStockRecord.LotNumber,
        //            CompId = outStockRecord.CompId,
        //            OriginalQuantity = product.InStockQuantity ?? 0.0f,
        //            ExpirationDate = outStockRecord.ExpirationDate,
        //            ItemId = outStockRecord.ItemId,
        //            InStockQuantity = outStockRecord.ApplyQuantity,
        //            ProductId = outStockRecord.ProductId,
        //            ProductCode = outStockRecord.ProductCode,
        //            ProductName = outStockRecord.ProductName,
        //            ProductSpec = outStockRecord.ProductSpec,
        //            Type = CommonConstants.StockInType.RETURN,
        //            BarCodeNumber = outStockRecord.BarCodeNumber,
        //            UserId = user.UserId,
        //            UserName = user.DisplayName,
        //            AfterQuantity = afterQuantity,
        //            QcType = product.QcType,
        //            ReturnOutStockId = outStockRecord.OutStockId
        //        };
        //        _dbContext.InStockItemRecords.Add( inStockRecord );
        //        product.InStockQuantity = afterQuantity;
        //        _dbContext.SaveChanges();
        //        scope.Complete();
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("事務失敗[Return]：{msg}", ex);
        //        return (false, ex.Message);
        //    }
        //}

        public List<ReturnStockRecord> ListReturnRecords(ListReturnRecordsRequest request)
        {
            IQueryable<ReturnStockRecord> query = _dbContext.ReturnStockRecords;
            
            if (request.StartDate != null)
            {
                query = query.Where(h => h.CreatedAt >= DateTimeHelper.ParseDateString(request.StartDate).Value);
            }
            if (request.EndDate != null)
            {
                DateTime endDateTime = DateTimeHelper.ParseDateString(request.EndDate).Value.AddDays(1);
                query = query.Where(h => h.CreatedAt < endDateTime);
            }
            if (request.ProductId != null)
            {
                query = query.Where(h => h.ProductId == request.ProductId);
            }
            if (request.LotNumberBatch != null)
            {
                query = query.Where(h => h.LotNumberBatch == request.LotNumberBatch);
            }
            if (request.LotNumber != null)
            {
                query = query.Where(h => h.LotNumber == request.LotNumber);
            }
            if (request.OutStockId != null)
            {
                query = query.Where(h => h.OutStockId == request.OutStockId);
            }
            query = query.Where(h => h.CompId == request.CompId);

            return query.ToList();
        }



        public List<NearExpiredProductVo> GetNearExpiredProductList(string compId,DateOnly compareDate,int? preDeadline)
        {
            var activeProducts = _dbContext.WarehouseProducts.Where(p => p.CompId == compId && p.IsActive == true).ToList();
            var productIds = activeProducts.Select(p=>p.ProductId).ToList();
            var allUnAllOutInStockItemList = _dbContext.InStockItemRecords.Where(i => productIds.Contains(i.ProductId) && i.OutStockStatus != CommonConstants.OutStockStatus.ALL).ToList();
            var nearExpireProductVoList = _mapper.Map<List<NearExpiredProductVo>>(activeProducts);
            
            foreach (var product in nearExpireProductVoList)
            {
                var matchedAllUnAllOutInStockItemList = allUnAllOutInStockItemList.Where(i => i.ProductId == product.ProductId).ToList();
                if (product.PreDeadline == null) continue;
                if (preDeadline != null)
                {
                    var nearExpiredInStockItemList = matchedAllUnAllOutInStockItemList.Where(i => i.ExpirationDate != null && i.ExpirationDate.Value.AddDays(-preDeadline.Value) <= compareDate).OrderBy(i => i.ExpirationDate).ToList();
                    product.InStockItemList = nearExpiredInStockItemList;
                }
                else
                {
                    var nearExpiredInStockItemList = matchedAllUnAllOutInStockItemList.Where(i => i.ExpirationDate != null && i.ExpirationDate.Value.AddDays(-product.PreDeadline.Value) <= compareDate).OrderBy(i => i.ExpirationDate).ToList();
                    product.InStockItemList = nearExpiredInStockItemList;
                }
            }

            return nearExpireProductVoList.Where(p=>p.InStockItemList.Count>0).ToList();
        }

        public PurchaseMainSheet GetPurchaseMainByInStockId(InStockItemRecord inStockItemRecord)
        {
            var purchaseSubItem = _dbContext.PurchaseSubItems.Where(s=>s.ItemId==inStockItemRecord.ItemId).FirstOrDefault();
            var main = _dbContext.PurchaseMainSheets.Where(m=>m.PurchaseMainId==purchaseSubItem.PurchaseMainId).FirstOrDefault();
            return main;
        }

        public List<AcceptanceItem> GetAcceptanceItemListByAcceptIdList(List<string> acceptIdList)
        {
            return _dbContext.AcceptanceItems.Where(a => acceptIdList.Contains(a.AcceptId)).ToList();   
        }
    }
}
