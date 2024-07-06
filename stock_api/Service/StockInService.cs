using AutoMapper;
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
                query = query.Where(h => h.ReceiveStatus == request.ReceiveStatus);
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
            if (request.ApplyDateStart != null)
            {
                query = query.Where(h => h.ApplyDate >= DateTimeHelper.ParseDateString(request.ApplyDateStart).Value);
            }
            if (request.ApplyDateEnd != null)
            {
                DateTime endDateTime = DateTimeHelper.ParseDateString(request.ApplyDateEnd).Value.AddDays(1);
                query = query.Where(h => h.ApplyDate < endDateTime);
            }
            if (request.GroupId != null)
            {
                query = query.Where(h => h.GroupIds.Contains(request.GroupId));
            }
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



        public (bool,string?,Qc?) UpdateAccepItem(PurchaseMainSheet purchaseMain,PurchaseSubItem purchaseSubItem, AcceptanceItem existingAcceptanceItem, UpdateAcceptItemRequest updateAcceptItem, WarehouseProduct product, string compId, WarehouseMember acceptMember,bool isInStocked)
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
                if (updateAcceptItem.LotNumber != null&&existingInStockRecord==null)
                {
                    existingAcceptanceItem.LotNumber = updateAcceptItem.LotNumber;
                }
                //var now = DateTime.Now;
                //var nowDateTimeString = DateTimeHelper.FormatDateString(now, "yyyyMMddHHmm");
                //existingAcceptanceItem.LotNumberBatch = $"{product.ProductCode}{nowDateTimeString}";
                if(existingInStockRecord == null)
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
                if (existingAcceptanceItem.AcceptQuantity!=null&&existingAcceptanceItem.AcceptQuantity >= existingAcceptanceItem.OrderQuantity)
                {
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                    existingAcceptanceItem.VerifyAt = DateTime.Now;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                    purchaseSubItem.InStockQuantity = existingAcceptanceItem.AcceptQuantity;
                }
                else if(existingAcceptanceItem.AcceptQuantity != null && existingAcceptanceItem.AcceptQuantity >0&& existingAcceptanceItem.AcceptQuantity<existingAcceptanceItem.OrderQuantity)
                {
                    // 判斷是否部分驗收
                    _logger.LogInformation("[品項部分驗收] AcceptId:${acceptId},AcceptQuantity:${AcceptQuantity},OrderQuantity:${}", existingAcceptanceItem.AcceptId,existingAcceptanceItem.AcceptQuantity,existingAcceptanceItem.OrderQuantity);
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                    existingAcceptanceItem.VerifyAt = DateTime.Now;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                    purchaseSubItem.InStockQuantity = existingAcceptanceItem.AcceptQuantity;
                }
                
                var lotNumberBatch = existingAcceptanceItem.LotNumberBatch;
                if (existingInStockRecord != null)
                {
                    var inStockLotNumberBatch  = existingInStockRecord.LotNumberBatch;
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

                if (updateAcceptItem.AcceptQuantity!=null)
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
                        QcTestStatus = CommonConstants.QcTestStatus.NONE
                    };
                    if (inStockItemRecord.IsNeedQc == true)
                    {
                        if(inStockItemRecord.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER)
                        {
                            var lastInStockedRecord = _dbContext.InStockItemRecords.Where(i=>i.LotNumber==inStockItemRecord.LotNumber).OrderByDescending(i=>i.CreatedAt).FirstOrDefault(); 
                            if(lastInStockedRecord!=null&&(lastInStockedRecord.QcTestStatus==CommonConstants.QcTestStatus.PASS))
                            {
                                // 上一批同批號的已經檢驗pass,表示此批號已經QC過了,不需再QC
                                inStockItemRecord.QcTestStatus = CommonConstants.QcTestStatus.PASS;
                            }
                        }
                        if(inStockItemRecord.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH)
                        {
                            var lastInStockedRecord = _dbContext.InStockItemRecords.Where(i => i.LotNumberBatch == inStockItemRecord.LotNumber).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                            if (lastInStockedRecord != null && (lastInStockedRecord.QcTestStatus == CommonConstants.QcTestStatus.PASS))
                            {
                                // 上一批同批號的已經檢驗pass,表示此批號已經QC過了,不需再QC
                                inStockItemRecord.QcTestStatus = CommonConstants.QcTestStatus.PASS;
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
                    product.InStockQuantity = inStockItemRecord.AfterQuantity;
                    // 應該是出庫時才去更新
                    //product.LotNumber = updateAcceptItem.LotNumber;
                    //product.LotNumberBatch = existingAcceptanceItem.LotNumberBatch;
                    //
                    product.DeliverFunction = updateAcceptItem.DeliverFunction;
                    product.DeliverTemperature = updateAcceptItem.DeliverTemperature;
                    product.SavingFunction = updateAcceptItem.SavingFunction;
                    product.SavingTemperature = updateAcceptItem.SavingTemperature;

                    _dbContext.TempInStockItemRecords.Add(tempInStockItemRecord);
                    _dbContext.InStockItemRecords.Add(inStockItemRecord);
                }
                //更新採購主單
                List<AcceptanceItem> allExistingAcceptanceItems = _dbContext.AcceptanceItems.Where(i=>i.PurchaseMainId == purchaseMain.PurchaseMainId && i.CompId==compId).ToList();
                List<AcceptanceItem> otherExistingAcceptanceItems = allExistingAcceptanceItems.Where(i=>i.AcceptId!= existingAcceptanceItem.AcceptId).ToList();
                var acceptIds = otherExistingAcceptanceItems.Select(i=>i.AcceptId).ToList();
                var inStockStatusList = otherExistingAcceptanceItems.Select(i=>i.InStockStatus).ToList();

                Log.Information("other acceptId list:${acceptIds}",string.Join(",", acceptIds));
                Log.Information("other inStockStatusList list:${inStockStatusList}", string.Join(",", inStockStatusList));
                Log.Information("current update acceptId:${acceptId} ", existingAcceptanceItem.AcceptId);
                Log.Information("current update acceptId:${inStockStatus}", existingAcceptanceItem.InStockStatus);

                if (otherExistingAcceptanceItems.All(item => item.InStockStatus==CommonConstants.PurchaseSubItemReceiveStatus.DONE )&&existingAcceptanceItem.InStockStatus== CommonConstants.PurchaseSubItemReceiveStatus.DONE)
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.ALL_ACCEPT;
                }
                else if (otherExistingAcceptanceItems.Any(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.PART || existingAcceptanceItem.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.PART))
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.PART_ACCEPT;
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return (true,null,qc);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateAccepItem]：{msg}", ex);
                return (false,ex.Message,null);
            }

        }

        public List<AcceptanceItem> AcceptanceItemsByUdiSerialCode(string udiserialCode,string compId)
        {
            return _dbContext.AcceptanceItems.Where(i=>i.UdiserialCode==udiserialCode&&i.CompId==compId).ToList();
        }

        public AcceptanceItem? GetAcceptanceItemByAcceptId(string acceptId)
        {
            return _dbContext.AcceptanceItems.Where(i => i.AcceptId == acceptId).FirstOrDefault();
        }
        public List<AcceptanceItem> GetAcceptanceItemByAcceptIdList(List<string> acceptIdList)
        {
            return _dbContext.AcceptanceItems.Where(i => acceptIdList.Contains( i.AcceptId)).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemByLotAndProductId(string lotNumber,string lotNumberBatch,string productId,string compId)
        {
            return _dbContext.AcceptanceItems.Where(i => i.LotNumber==lotNumber&&i.LotNumberBatch==lotNumberBatch&&i.ProductId==productId&&i.CompId==compId).OrderBy(i=>i.CreatedAt).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemNotAllInStockByProductCodeAndCompId(string productCode, string compId)
        {
            return _dbContext.AcceptanceItems.Where(i => i.ProductCode == productCode && i.CompId == compId && i.InStockStatus!=CommonConstants.PurchaseSubItemReceiveStatus.DONE).OrderBy(i => i.CreatedAt).ToList();
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

            if (request.PaginationCondition.IsDescOrderBy)
            {
                query = request.PaginationCondition.OrderByField switch
                {
                    "lotNumberBatch" => query.OrderByDescending(h => h.LotNumberBatch),
                    "lotNumber" => query.OrderByDescending(h => h.LotNumber),
                    "expirationDate" => query.OrderByDescending(h => h.ExpirationDate),
                    "createdAt" => query.OrderByDescending(h => h.CreatedAt),
                    "updatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.UpdatedAt),
                };
            }
            else
            {
                query = request.PaginationCondition.OrderByField switch
                {
                    "lotNumberBatch" => query.OrderBy(h => h.LotNumberBatch),
                    "lotNumber" => query.OrderBy(h => h.LotNumber),
                    "expirationDate" => query.OrderBy(h => h.ExpirationDate),
                    "createdAt" => query.OrderBy(h => h.CreatedAt),
                    "updatedAt" => query.OrderBy(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.UpdatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);

            query = query.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);

        }

        public List<InStockItemRecord> GetInStockRecordsHistory(string prodcutId,string compId)
        {
            return _dbContext.InStockItemRecords.Where(record=>record.CompId==compId&&record.ProductId==prodcutId).ToList();
        }

        public InStockItemRecord? GetInStockRecordByLotNumberBatch(string lotNumberBatch, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumberBatch == lotNumberBatch).FirstOrDefault();
        }

        public List<InStockItemRecord> GetInStockRecordsHistoryByLotNumberBatch(string lotNumberBatch, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumberBatch == lotNumberBatch).ToList();
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
            && record.OutStockStatus != CommonConstants.OutStockStatus.ALL).OrderBy(record => record.ExpirationDate).ThenBy(record=>record.CreatedAt).ToList();
        }

        public List<InStockItemRecord> GetProductInStockRecordsByAcceptId(string itemId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.ItemId == itemId).OrderBy(r => r.CreatedAt).ToList();
        }
        
        public List<string> GetDuplicateBatchList(List<string> lotNumberBatchList) {
            return _dbContext.InStockItemRecords.Where(record=>lotNumberBatchList.Contains(record.LotNumberBatch)).Select(i=>i.LotNumberBatch).ToList();
        }

        public (List<InStockItemRecord>,List<OutStockRecord>) GetAllInAndOutRecordByProductCodeList(List<string> productCodeList,string compId)
        {
            var inStockRecords = _dbContext.InStockItemRecords.Where(r => productCodeList.Contains(r.ProductCode) && r.CompId == compId).ToList();

            var outStockRecords = _dbContext.OutStockRecords.Where(r => productCodeList.Contains(r.ProductCode) && r.CompId == compId).ToList();
            return (inStockRecords, outStockRecords);
        }

    }
}
