using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
using System.Diagnostics;

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
            if (request.Type != null)
            {
                query = query.Where(h => h.Type == request.Type);
            }
            query = query.Where(h => h.CompId == request.CompId);


            return query.ToList();
        }

        /// <summary>
        /// 在資料庫端進行分頁的查詢方法
        /// </summary>
        /// <param name="request">查詢條件</param>
        /// <returns>分頁後的資料和總頁數</returns>
        public (List<PurchaseAcceptanceItemsView> Data, int TotalPages, int TotalItems) SearchPurchaseAcceptanceItemsWithPagination(SearchPurchaseAcceptItemRequest request)
        {
            IQueryable<PurchaseAcceptanceItemsView> query = _dbContext.PurchaseAcceptanceItemsViews;

            // 套用基本過濾條件
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
            if (request.Type != null)
            {
                query = query.Where(h => h.Type == request.Type);
            }
            query = query.Where(h => h.CompId == request.CompId);

            // 過濾掉 OwnerProcess 為 NOT_AGREE 的項目
            query = query.Where(h => h.OwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE);

            // 過濾掉 OrderQuantity 為 0 的項目
            query = query.Where(h => h.OrderQuantity > 0);

            // 取得分頁後的 PurchaseMainId 列表（先分組再分頁）
            var orderByField = request.PaginationCondition.OrderByField ?? "ApplyDate";
            orderByField = StringUtils.CapitalizeFirstLetter(orderByField);
            bool isDesc = request.PaginationCondition.IsDescOrderBy;

            // 依 PurchaseMainId 分組並排序
            IQueryable<IGrouping<string, PurchaseAcceptanceItemsView>> groupedQuery = query.GroupBy(h => h.PurchaseMainId);

            // 先建立排序後的 PurchaseMainId 查詢
            IQueryable<string> orderedPurchaseMainIds;
            switch (orderByField)
            {
                case "ApplyDate":
                    orderedPurchaseMainIds = isDesc
                        ? groupedQuery.OrderByDescending(g => g.Max(x => x.ApplyDate)).Select(g => g.Key)
                        : groupedQuery.OrderBy(g => g.Min(x => x.ApplyDate)).Select(g => g.Key);
                    break;
                case "DemandDate":
                    orderedPurchaseMainIds = isDesc
                        ? groupedQuery.OrderByDescending(g => g.Max(x => x.DemandDate)).Select(g => g.Key)
                        : groupedQuery.OrderBy(g => g.Min(x => x.DemandDate)).Select(g => g.Key);
                    break;
                default:
                    orderedPurchaseMainIds = isDesc
                        ? groupedQuery.OrderByDescending(g => g.Max(x => x.ApplyDate)).Select(g => g.Key)
                        : groupedQuery.OrderBy(g => g.Min(x => x.ApplyDate)).Select(g => g.Key);
                    break;
            }

            // 計算總數
            int totalItems = orderedPurchaseMainIds.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);

            // 分頁取得 PurchaseMainId
            var pagedPurchaseMainIds = orderedPurchaseMainIds
                .Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize)
                .Take(request.PaginationCondition.PageSize)
                .ToList();

            // 只查詢分頁後的 PurchaseMainId 對應的完整資料
            var result = query
                .Where(h => pagedPurchaseMainIds.Contains(h.PurchaseMainId))
                .ToList();

            return (result, totalPages, totalItems);
        }

        public List<AcceptanceItem> GetAcceptanceItemsByAccepIdList(List<string> acceptIdList, string compId)
        {
            return _dbContext.AcceptanceItems.Where(ai => ai.CompId == compId && acceptIdList.Contains(ai.AcceptId)).ToList();
        }

        public InStockItemRecord? GetInStockRecordById(string inStockId)
        {
            return _dbContext.InStockItemRecords.Where(i => i.InStockId == inStockId).FirstOrDefault();
        }

        public (bool, string?, Qc?) UpdateAcceptItem(PurchaseMainSheet purchaseMain, PurchaseSubItem purchaseSubItem, AcceptanceItem existingAcceptanceItem, UpdateAcceptItemRequest updateAcceptItem, WarehouseProduct product, string compId, WarehouseMember acceptMember, bool isDirectOutStock)
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
                    existingAcceptanceItem.CurrentTotalQuantity = product.InStockQuantity + existingAcceptanceItem.AcceptQuantity;
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

                if (existingAcceptanceItem.AcceptQuantity != null && existingAcceptanceItem.AcceptQuantity >= existingAcceptanceItem.OrderQuantity)
                {
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                    existingAcceptanceItem.VerifyAt = DateTime.Now;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                    purchaseSubItem.InStockQuantity = existingAcceptanceItem.AcceptQuantity;
                    purchaseSubItem.ReceiveQuantity = purchaseSubItem.InStockQuantity;
                }
                else if (existingAcceptanceItem.AcceptQuantity != null && existingAcceptanceItem.AcceptQuantity > 0 && existingAcceptanceItem.AcceptQuantity < existingAcceptanceItem.OrderQuantity)
                {
                    _logger.LogInformation("[品項部分驗收] AcceptId:${acceptId},AcceptQuantity:${AcceptQuantity},OrderQuantity:${OrderQuantity}", existingAcceptanceItem.AcceptId, existingAcceptanceItem.AcceptQuantity, existingAcceptanceItem.OrderQuantity);
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                    existingAcceptanceItem.VerifyAt = DateTime.Now;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                    purchaseSubItem.InStockQuantity = existingAcceptanceItem.AcceptQuantity;
                    purchaseSubItem.ReceiveQuantity = purchaseSubItem.InStockQuantity;
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

                    var qcTestStatus = CommonConstants.QcTestStatus.NONE;
                    if (product.IsNeedAcceptProcess == true)
                    {
                        if (updateAcceptItem.LotNumber != null && product.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER)
                        {
                            if (IsThisLotNumberAlreadyQc(updateAcceptItem.LotNumber) == true)
                            {
                                qcTestStatus = CommonConstants.QcTestStatus.DONE;
                            }
                        }
                        if (lotNumberBatch != null && product.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH)
                        {
                            if (IsThisLotNumberBatchAlreadyQc(lotNumberBatch) == true)
                            {
                                qcTestStatus = CommonConstants.QcTestStatus.DONE;
                            }
                        }
                    }
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
                        QcTestStatus = qcTestStatus,
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
                                inStockItemRecord.QcTestStatus = CommonConstants.QcTestStatus.DONE;
                            }
                        }
                        if (inStockItemRecord.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH)
                        {
                            var lastInStockedRecord = _dbContext.InStockItemRecords.Where(i => i.LotNumberBatch == inStockItemRecord.LotNumberBatch).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                            if (lastInStockedRecord != null && (lastInStockedRecord.QcTestStatus == CommonConstants.QcTestStatus.DONE))
                            {
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

                    if (isDirectOutStock == false)
                    {
                        product.InStockQuantity = inStockItemRecord.AfterQuantity;
                    }

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

                    _dbContext.TempInStockItemRecords.Add(tempInStockItemRecord);
                    _dbContext.InStockItemRecords.Add(inStockItemRecord);
                }

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
                else if (otherExistingAcceptanceItems.All(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE || item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.CLOSE) && existingAcceptanceItem.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE)
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.ALL_ACCEPT;
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
                query = query.Where(h => h.CreatedAt >= DateTimeHelper.ParseDateString(request.StartDate).Value);
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

        public (List<InStockItemRecordNewLotNumberVew>, int TotalPages) ListStockInRecordsWithNewLotNumber(ListStockInRecordsWithNewLotNumberRequest request)
        {
            IQueryable<InStockItemRecordNewLotNumberVew> query = _dbContext.InStockItemRecordNewLotNumberVews;
            if (request.LotNumberBatch != null)
            {
                query = query.Where(h => h.LotNumberBatch == request.LotNumberBatch);
            }
            if (request.LotNumber != null)
            {
                query = query.Where(h => h.LotNumber == request.LotNumber);
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
            if (request.StartDate != null)
            {
                query = query.Where(h => h.InStockTime >= DateTimeHelper.ParseDateString(request.StartDate).Value);
            }
            if (request.EndDate != null)
            {
                DateTime endDateTime = DateTimeHelper.ParseDateString(request.EndDate).Value.AddDays(1);
                query = query.Where(h => h.InStockTime < endDateTime);
            }
            if (request.SupplierId != null)
            {
                query = query.Where(h => h.SupplierId == request.SupplierId);
            }
            if (request.IsNewLotNumber != null)
            {
                query = query.Where(h => h.IsNewLotNumber == request.IsNewLotNumber);
            }
            if (request.GroupId != null)
            {
                query = query.Where(h => h.GroupIds.Contains(request.GroupId));
            }

            query = query.Where(h => h.CompId == request.CompId);

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var groupNameList =
                query = query.Where(h => h.LotNumberBatch.Contains(request.Keywords)
                || h.LotNumber.Contains(request.Keywords)
                || h.ProductName.Contains(request.Keywords)
                || h.UserName.Contains(request.Keywords)
                || h.GroupNames.Contains(request.Keywords)
                );
            }
            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "CreatedAt";

            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "LotNumberBatch" => query.OrderByDescending(h => h.LotNumberBatch),
                    "LotNumber" => query.OrderByDescending(h => h.LotNumber),
                    "ExpirationDate" => query.OrderByDescending(h => h.ExpirationDate),
                    "InStockTime" => query.OrderByDescending(h => h.InStockTime),
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
                    "InStockTime" => query.OrderBy(h => h.InStockTime),
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
            var sw = Stopwatch.StartNew();
            var result = _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.ProductId == prodcutId).ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetInStockRecordsHistory] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public Dictionary<string, List<InStockItemRecord>> GetInStockRecordsHistoryByProductIdList(List<string> productIdList, string compId)
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.InStockItemRecords.Where(record => record.CompId == compId && productIdList.Contains(record.ProductId)).ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetInStockRecordsHistoryByProductIdList] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result.GroupBy(r => r.ProductId).ToDictionary(g => g.Key, g => g.ToList());
        }

        public InStockItemRecord? GetInStockRecordByLotNumberBatch(string lotNumberBatch, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumberBatch == lotNumberBatch).FirstOrDefault();
        }

        public List<InStockItemRecord> GetInStockRecordListByLotNumber(string lotNumber, string compId)
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumber == lotNumber).ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetInStockRecordListByLotNumber] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public List<InStockItemRecord> GetInStockRecordByLotNumberBatchList(List<string> lotNumberBatchList, string compId)
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.InStockItemRecords.Where(record => record.CompId == compId && lotNumberBatchList.Contains(record.LotNumberBatch)).ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetInStockRecordByLotNumberBatchList] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
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
            return _dbContext.InStockItemRecords.Where(record => record.ItemId != null && itemIdList.Contains(record.ItemId)).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemsByInIdList(List<string> idList)
        {
            return _dbContext.AcceptanceItems.Where(record => idList.Contains(record.AcceptId)).ToList();
        }

        public List<AcceptanceItem> GetAcceptanceItemsByItemIdList(List<string> itemIdList)
        {
            return _dbContext.AcceptanceItems.Where(record => itemIdList.Contains(record.ItemId)).ToList();
        }

        public List<InStockItemRecord> GetProductInStockRecordsHistoryNotAllOutExpirationFIFO(string productCode, string compId)
        {
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
                var inStockRecord = _dbContext.InStockItemRecords.Where(i => i.LotNumberBatch == outStockRecord.LotNumberBatch).FirstOrDefault();

                var inStockQuantityBefore = inStockRecord?.InStockQuantity ?? 0 + inStockRecord?.AdjustInQuantity ?? 0;
                var outStockQuantityBefore = outStockRecord.ApplyQuantity;
                var outStockRecordsAfter = _dbContext.OutStockRecords.Where(r => r.ProductId == outStockRecord.ProductId && r.CreatedAt > outStockRecord.CreatedAt).ToList();

                if (inStockRecord != null)
                {
                    inStockRecord.OutStockQuantity = inStockRecord.OutStockQuantity - returnQuantity;
                    if ((inStockRecord.InStockQuantity + inStockRecord.AdjustInQuantity) - (inStockRecord.OutStockQuantity + inStockRecord.AdjustOutQuantity) > 0)
                    {
                        inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.PART;
                    }
                    if ((inStockRecord.OutStockQuantity + inStockRecord.AdjustOutQuantity) == 0)
                    {
                        inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.NONE;
                    }
                    if (inStockRecord.InStockQuantity + inStockRecord.AdjustInQuantity - inStockRecord.OutStockQuantity - inStockRecord.AdjustOutQuantity - inStockRecord.RejectQuantity <= 0)
                    {
                        inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.ALL;
                    }
                    inStockRecord.ReturnOutStockId = outStockRecord.OutStockId;
                }

                outStockRecord.ApplyQuantity = outStockRecord.ApplyQuantity - returnQuantity;
                outStockRecord.IsReturned = true;

                product.InStockQuantity = product.InStockQuantity + returnQuantity;

                var afterQuantityBefore = outStockRecord.AfterQuantity;
                var afterQuantityAfter = product.InStockQuantity;

                string? inStockId = null;
                if (inStockRecord != null) inStockId = inStockRecord.InStockId;

                var returnStockRecord = new ReturnStockRecord()
                {
                    InStockId = inStockId,
                    OutStockId = outStockRecord.OutStockId,
                    ReturnQuantity = returnQuantity,
                    InStockQuantityBefore = inStockQuantityBefore,
                    InStockQuantityAfter = inStockRecord?.InStockQuantity ?? 0 + inStockRecord?.AdjustInQuantity ?? 0,
                    OutStockApplyQuantityBefore = outStockQuantityBefore,
                    OutStockApplyQuantityAfter = outStockRecord.ApplyQuantity,
                    AfterQuantityBefore = afterQuantityBefore,
                    AfterQuantityAfter = afterQuantityAfter,
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

        public List<NearExpiredProductVo> GetNearExpiredProductList(string compId, DateOnly compareDate, int? preDeadline)
        {
            var activeProducts = _dbContext.WarehouseProducts.Where(p => p.CompId == compId && p.IsActive == true).ToList();
            var productIds = activeProducts.Select(p => p.ProductId).ToList();
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

                foreach (var inStockItem in product.InStockItemList)
                {
                    if (inStockItem.LotNumber != null)
                    {
                        product.NearExpiredLotNumber.Add(inStockItem.LotNumber);
                    }
                    if (inStockItem.LotNumberBatch != null)
                    {
                        product.NearExpiredLotNumberBatch.Add(inStockItem.LotNumberBatch);
                    }
                }
                product.NearExpiredQuantity = product.InStockItemList.Sum(i => (i.InStockQuantity + i.AdjustInQuantity - i.OutStockQuantity - i.AdjustOutQuantity));
            }

            return nearExpireProductVoList.Where(p => p.InStockItemList.Count > 0 && p.NearExpiredQuantity > 0).ToList();
        }

        public PurchaseMainSheet GetPurchaseMainByInStockId(InStockItemRecord inStockItemRecord)
        {
            var purchaseSubItem = _dbContext.PurchaseSubItems.Where(s => s.ItemId == inStockItemRecord.ItemId).FirstOrDefault();
            var main = _dbContext.PurchaseMainSheets.Where(m => m.PurchaseMainId == purchaseSubItem.PurchaseMainId).FirstOrDefault();
            return main;
        }

        public List<AcceptanceItem> GetAcceptanceItemListByAcceptIdList(List<string> acceptIdList)
        {
            return _dbContext.AcceptanceItems.Where(a => acceptIdList.Contains(a.AcceptId)).ToList();
        }

        public List<ProductNewLotnumberView> GetProductsNewLotNumberList()
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.ProductNewLotnumberViews.ToList().Where(e => e.LotNumber != "N/A").ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetProductsNewLotNumberList] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public List<InStockItemRecordNewLotNumberVew> GetInStockItemRecordNewLotNumberViews()
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.InStockItemRecordNewLotNumberVews.ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetInStockItemRecordNewLotNumberViews] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public List<InStockItemRecordNewLotNumberVew> GetInStockItemRecordNewLotNumberViewsByProductIds(List<string> productIds)
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.InStockItemRecordNewLotNumberVews.Where(v => productIds.Contains(v.ProductId)).ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetInStockItemRecordNewLotNumberViewsByProductIds] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public List<ProductNewLotnumberbatchView> GetProductsNewLotNumberBatchList()
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.ProductNewLotnumberbatchViews.ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetProductsNewLotNumberBatchList] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public List<ProductNewLotnumberbatchView> GetProductsNewLotNumberBatchListByProductIds(List<string> productIds)
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.ProductNewLotnumberbatchViews.Where(v => productIds.Contains(v.ProductId)).ToList();
            sw.Stop();
            _logger.LogInformation("[StockInService.GetProductsNewLotNumberBatchListByProductIds] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public List<InStockItemRecord> GetAllInStockItemRecordsByCompId(string compId)
        {
            return _dbContext.InStockItemRecords.Where(i => i.CompId == compId).ToList();
        }

        public (bool, string?) UpdateInStockItem(UpdateInStockRequest request, AcceptanceItem acceptanceItem)
        {
            using var scope = new TransactionScope();
            try
            {
                var inStockRecord = _dbContext.InStockItemRecords.Where(i => i.ItemId == acceptanceItem.ItemId).FirstOrDefault();
                if (request.LotNumber != null) acceptanceItem.LotNumber = request.LotNumber;
                if (request.ExpirationDate != null) acceptanceItem.ExpirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.ExpirationDate).Value);

                if (inStockRecord != null)
                {
                    if (request.LotNumber != null) inStockRecord.LotNumber = request.LotNumber;
                    if (request.ExpirationDate != null) inStockRecord.ExpirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.ExpirationDate).Value);
                }
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

        public List<InStockItemRecord> GetInStockRecordsNotAllOutOrReject(string productId)
        {
            return _dbContext.InStockItemRecords.Where(i => i.ProductId == productId && (i.InStockQuantity + i.AdjustInQuantity - i.OutStockQuantity - i.AdjustOutQuantity - i.RejectQuantity) > 0).ToList();
        }

        public (bool, string?) DeleteInStockRecord(InStockItemRecord inStockItemRecord)
        {
            using var scope = new TransactionScope();
            try
            {
                var acceptItem = _dbContext.AcceptanceItems.Where(i => i.ItemId == inStockItemRecord.ItemId).FirstOrDefault();
                var purchaseSubItem = _dbContext.PurchaseSubItems.Where(i => i.ItemId == inStockItemRecord.ItemId).FirstOrDefault();
                var product = _dbContext.WarehouseProducts.Where(p => p.ProductId == inStockItemRecord.ProductId).FirstOrDefault();
                acceptItem.AcceptQuantity = acceptItem.AcceptQuantity - inStockItemRecord.InStockQuantity - inStockItemRecord.AdjustInQuantity;
                product.InStockQuantity = product.InStockQuantity - inStockItemRecord.InStockQuantity - inStockItemRecord.AdjustInQuantity;
                if (acceptItem.AcceptQuantity == 0)
                {
                    acceptItem.AcceptUserId = null;
                    acceptItem.LotNumber = null;
                    acceptItem.LotNumberBatch = null;
                    acceptItem.ExpirationDate = null;
                    acceptItem.PackagingStatus = null;
                    acceptItem.QcStatus = null;
                    acceptItem.Comment = null;
                    acceptItem.DeliverFunction = null;
                    acceptItem.DeliverTemperature = null;
                    acceptItem.SavingFunction = null;
                    acceptItem.SavingTemperature = null;
                    acceptItem.VerifyAt = null;
                }
                else
                {
                    var beforeInStockItemRecord = _dbContext.InStockItemRecords.Where(i => i.InStockId != inStockItemRecord.InStockId &&
                    i.ItemId == inStockItemRecord.ItemId).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                    acceptItem.AcceptUserId = beforeInStockItemRecord.UserId;
                    acceptItem.LotNumber = beforeInStockItemRecord.LotNumber;
                    acceptItem.LotNumberBatch = beforeInStockItemRecord.LotNumberBatch;
                    acceptItem.ExpirationDate = beforeInStockItemRecord.ExpirationDate;
                    acceptItem.PackagingStatus = beforeInStockItemRecord.PackagingStatus;
                    acceptItem.QcStatus = null;
                    acceptItem.Comment = beforeInStockItemRecord.Comment;
                    acceptItem.DeliverFunction = beforeInStockItemRecord.DeliverFunction;
                    acceptItem.DeliverTemperature = beforeInStockItemRecord.DeliverTemperature;
                    acceptItem.SavingFunction = beforeInStockItemRecord.SavingFunction;
                    acceptItem.SavingTemperature = beforeInStockItemRecord.SavingTemperature;
                    acceptItem.VerifyAt = beforeInStockItemRecord.CreatedAt;
                }

                if (acceptItem.AcceptQuantity != null && acceptItem.AcceptQuantity >= acceptItem.OrderQuantity)
                {
                    acceptItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                }
                else if (acceptItem.AcceptQuantity != null && acceptItem.AcceptQuantity > 0 && acceptItem.AcceptQuantity < acceptItem.OrderQuantity)
                {
                    _logger.LogInformation("[刪除部分驗收] AcceptId:${acceptId},AcceptQuantity:${AcceptQuantity},OrderQuantity:${}", acceptItem.AcceptId, acceptItem.AcceptQuantity, acceptItem.OrderQuantity);
                    acceptItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                }
                else if (acceptItem.AcceptQuantity != null && acceptItem.AcceptQuantity == 0)
                {
                    acceptItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.NONE;
                    purchaseSubItem.ReceiveStatus = CommonConstants.PurchaseSubItemReceiveStatus.NONE;
                }
                purchaseSubItem.InStockQuantity = purchaseSubItem.InStockQuantity - inStockItemRecord.InStockQuantity - inStockItemRecord.AdjustInQuantity;
                purchaseSubItem.ReceiveQuantity = purchaseSubItem.InStockQuantity;

                _dbContext.InStockItemRecords.Remove(inStockItemRecord);
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[DeleteInStockRecord]：{msg}", ex);
                return (false, ex.Message);
            }
        }

        public List<InStockItemRecord> GetInStockItemRecordsByLotNumberBatchList(string compId, List<string> lotNumberBatchList)
        {
            return _dbContext.InStockItemRecords.Where(i => lotNumberBatchList.Contains(i.LotNumberBatch) && i.CompId == compId).ToList();
        }

        public (bool, string?, string?) OwnerStockInService(OwnerStockInRequest request, WarehouseProduct product, WarehouseMember user)
        {
            using var scope = new TransactionScope();
            try
            {
                string lotNumberBatch = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                var inStockItemRecord = new InStockItemRecord()
                {
                    InStockId = Guid.NewGuid().ToString(),
                    LotNumberBatch = lotNumberBatch,
                    CompId = request.CompId,
                    OriginalQuantity = product.InStockQuantity.Value,
                    ExpirationDate = request.ExpirationDate != null ? DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.ExpirationDate).Value) : null,
                    InStockQuantity = request.Quantity,
                    ProductId = product.ProductId,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    ProductSpec = product.ProductSpec,
                    Type = CommonConstants.StockInType.OWNER_DIRECT_IN,
                    BarCodeNumber = lotNumberBatch,
                    UserId = user.UserId,
                    UserName = user.DisplayName,
                    AfterQuantity = (product.InStockQuantity.Value + request.Quantity),
                    IsNeedQc = product.IsNeedAcceptProcess,
                    QcType = product.QcType,
                    QcTestStatus = CommonConstants.QcTestStatus.NONE,
                    Comment = request.Comment,
                };

                product.InStockQuantity = inStockItemRecord.AfterQuantity;
                product.LotNumberBatch = inStockItemRecord.LotNumberBatch;
                _dbContext.InStockItemRecords.Add(inStockItemRecord);
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null, lotNumberBatch);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OwnerStockInService]：{msg}", ex);
                return (false, ex.Message, null);
            }
        }

        public bool IsThisLotNumberAlreadyQc(string lotNumber)
        {
            return _dbContext.InStockItemRecords.Where(i => i.LotNumber == lotNumber && i.QcTestStatus == CommonConstants.QcTestStatus.DONE).Any();
        }

        public bool IsThisLotNumberBatchAlreadyQc(string lotNumberBatch)
        {
            return _dbContext.InStockItemRecords.Where(i => i.LotNumberBatch == lotNumberBatch && i.QcTestStatus == CommonConstants.QcTestStatus.DONE).Any();
        }
    }
}

