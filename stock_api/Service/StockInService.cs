using AutoMapper;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
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


        public bool UpdateAccepItems(PurchaseMainSheet purchaseMain,List<AcceptanceItem> existingAcceptanceItems, List<UpdateAcceptItemRequest> updateAcceptItems, List<WarehouseProduct> products, string compId, WarehouseMember acceptMember)
        {
            using var scope = new TransactionScope();
            try
            {
                foreach (var existItem in existingAcceptanceItems)
                {
                    var matchedUpdateAcceptItem = updateAcceptItems.Where(u => u.AcceptId == existItem.AcceptId).FirstOrDefault();
                    var matchedProduct = products.Where(p => p.ProductId == existItem.ProductId && p.CompId == compId).FirstOrDefault();
                    if (matchedUpdateAcceptItem != null)
                    {
                        if (matchedUpdateAcceptItem.AcceptQuantity.HasValue)
                        {
                            existItem.AcceptQuantity = matchedUpdateAcceptItem.AcceptQuantity.Value;
                        }
                        if (matchedUpdateAcceptItem.AcceptUserId != null)
                        {
                            existItem.AcceptUserId = matchedUpdateAcceptItem.AcceptUserId;
                        }
                        if (matchedUpdateAcceptItem.LotNumber != null)
                        {
                            existItem.LotNumber = matchedUpdateAcceptItem.LotNumber;
                        }
                        //var now = DateTime.Now;
                        //var nowDateTimeString = DateTimeHelper.FormatDateString(now, "yyyyMMddHHmm");

                        //existItem.LotNumberBatch = $"{matchedProduct.ProductCode}{nowDateTimeString}";
                        existItem.LotNumberBatch = existItem.LotNumberBatchSeq.ToString("D12");
                        if (matchedUpdateAcceptItem.ExpirationDate != null)
                        {
                            existItem.ExpirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(matchedUpdateAcceptItem.ExpirationDate).Value);
                        }
                        if (matchedUpdateAcceptItem.PackagingStatus != null)
                        {
                            existItem.PackagingStatus = matchedUpdateAcceptItem.PackagingStatus;
                        }
                        if (matchedUpdateAcceptItem.QcStatus != null)
                        {
                            existItem.QcStatus = matchedUpdateAcceptItem.QcStatus;
                        }
                        if (existItem.AcceptQuantity != null && existItem.QcStatus != CommonConstants.QcStatus.FAIL)
                        {
                            existItem.CurrentTotalQuantity = matchedProduct.InStockQuantity + existItem.AcceptQuantity; // 驗收入庫後，當下該品項的總庫存數量
                        }

                        if (matchedUpdateAcceptItem.Comment != null)
                        {
                            existItem.Comment = matchedUpdateAcceptItem.Comment;
                        }
                        if (matchedUpdateAcceptItem.QcComment != null)
                        {
                            existItem.QcComment = matchedUpdateAcceptItem.QcComment;
                        }

                        if (existItem.AcceptQuantity != null && existItem.QcStatus != CommonConstants.QcStatus.FAIL)
                        {
                            var tempInStockItemRecord = new TempInStockItemRecord()
                            {
                                InStockId = Guid.NewGuid().ToString(),
                                LotNumberBatch = existItem.LotNumberBatch,
                                LotNumber = matchedUpdateAcceptItem.LotNumber,
                                CompId = compId,
                                OriginalQuantity = matchedProduct.InStockQuantity.Value,
                                ExpirationDate = existItem.ExpirationDate,
                                InStockQuantity = existItem.AcceptQuantity.Value,
                                ProductId = matchedProduct.ProductId,
                                ProductName = matchedProduct.ProductName,
                                ProductSpec = matchedProduct.ProductSpec,
                                Type = CommonConstants.StockInType.PURCHASE,
                                UserId = acceptMember.UserId,
                                UserName = acceptMember.DisplayName,
                                IsTransfer = true,
                                InventoryId = existItem.PurchaseMainId
                            };

                            var inStockItemRecord = new InStockItemRecord()
                            {
                                InStockId = Guid.NewGuid().ToString(),
                                LotNumberBatch = existItem.LotNumberBatch,
                                LotNumber = matchedUpdateAcceptItem.LotNumber,
                                CompId = compId,
                                OriginalQuantity = matchedProduct.InStockQuantity.Value,
                                ExpirationDate = existItem.ExpirationDate,
                                ItemId = existItem.ItemId,
                                InStockQuantity = existItem.AcceptQuantity.Value,
                                ProductId = matchedProduct.ProductId,
                                ProductCode = matchedProduct.ProductCode,
                                ProductName = matchedProduct.ProductName,
                                ProductSpec = matchedProduct.ProductSpec,
                                Type = CommonConstants.StockInType.PURCHASE,
                                BarCodeNumber = "", //TODO
                                UserId = acceptMember.UserId,
                                UserName = acceptMember.DisplayName,
                                AfterQuantity = matchedProduct.InStockQuantity.Value + existItem.AcceptQuantity.Value,
                            };
                            //
                            matchedProduct.InStockQuantity = inStockItemRecord.AfterQuantity;
                            matchedProduct.LotNumber = matchedUpdateAcceptItem.LotNumber;
                            matchedProduct.LotNumberBatch = existItem.LotNumberBatch;

                            _dbContext.TempInStockItemRecords.Add(tempInStockItemRecord);
                            _dbContext.InStockItemRecords.Add(inStockItemRecord);
                        }
                    };
                }
                if(existingAcceptanceItems.All(item=>item.QcStatus!=null && item.QcStatus != CommonConstants.QcStatus.FAIL))
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.ALL_ACCEPT;
                }else if (existingAcceptanceItems.Any(item => item.QcStatus != null && item.QcStatus != CommonConstants.QcStatus.FAIL))
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.PART_ACCEPT;
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateAccepItems]：{msg}", ex);
                return false;
            }

        }

        public (bool,string?) UpdateAccepItem(PurchaseMainSheet purchaseMain, AcceptanceItem existingAcceptanceItem, UpdateAcceptItemRequest updateAcceptItem, WarehouseProduct product, string compId, WarehouseMember acceptMember,bool isInStocked)
        {
            using var scope = new TransactionScope();
            try
            {
                
                if (updateAcceptItem.AcceptQuantity.HasValue)
                {
                    float existingAcceptQty = existingAcceptanceItem.AcceptQuantity ?? 0;
                    existingAcceptanceItem.AcceptQuantity = updateAcceptItem.AcceptQuantity.Value + existingAcceptQty;
                }
                if (updateAcceptItem.AcceptUserId != null)
                {
                    existingAcceptanceItem.AcceptUserId = updateAcceptItem.AcceptUserId;
                }
                if (updateAcceptItem.LotNumber != null)
                {
                    existingAcceptanceItem.LotNumber = updateAcceptItem.LotNumber;
                }
                //var now = DateTime.Now;
                //var nowDateTimeString = DateTimeHelper.FormatDateString(now, "yyyyMMddHHmm");
                //existingAcceptanceItem.LotNumberBatch = $"{product.ProductCode}{nowDateTimeString}";
                existingAcceptanceItem.LotNumberBatch = existingAcceptanceItem.LotNumberBatchSeq.ToString("D12");
                if (updateAcceptItem.ExpirationDate != null)
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
                if (existingAcceptanceItem.AcceptQuantity >= existingAcceptanceItem.OrderQuantity)
                {
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.DONE;
                }
                // 判斷是否部分驗收
                if (existingAcceptanceItem.AcceptQuantity >0&& existingAcceptanceItem.AcceptQuantity<existingAcceptanceItem.OrderQuantity)
                {
                    existingAcceptanceItem.InStockStatus = CommonConstants.PurchaseSubItemReceiveStatus.PART;
                }

                if (updateAcceptItem.AcceptQuantity!=null)
                {
                    var tempInStockItemRecord = new TempInStockItemRecord()
                    {
                        InStockId = Guid.NewGuid().ToString(),
                        LotNumberBatch = existingAcceptanceItem.LotNumberBatch,
                        LotNumber = updateAcceptItem.LotNumber,
                        CompId = compId,
                        OriginalQuantity = product.InStockQuantity.Value,
                        ExpirationDate = existingAcceptanceItem.ExpirationDate,
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
                        DeliverFunction = existingAcceptanceItem.DeliverFunction,
                        DeliverTemperature = existingAcceptanceItem.DeliverTemperature,
                        SavingFunction = existingAcceptanceItem.SavingFunction,
                        SavingTemperature = existingAcceptanceItem.SavingTemperature,
                    };

                    var inStockItemRecord = new InStockItemRecord()
                    {
                        InStockId = Guid.NewGuid().ToString(),
                        LotNumberBatch = existingAcceptanceItem.LotNumberBatch,
                        LotNumber = updateAcceptItem.LotNumber,
                        CompId = compId,
                        OriginalQuantity = product.InStockQuantity.Value,
                        ExpirationDate = existingAcceptanceItem.ExpirationDate,
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
                        DeliverFunction = existingAcceptanceItem.DeliverFunction,
                        DeliverTemperature = existingAcceptanceItem.DeliverTemperature,
                        SavingFunction = existingAcceptanceItem.SavingFunction,
                        SavingTemperature = existingAcceptanceItem.SavingTemperature,
                    };
                    //更新庫存品項
                    product.InStockQuantity = inStockItemRecord.AfterQuantity;
                    // 應該是出庫時才去更新
                    //product.LotNumber = updateAcceptItem.LotNumber;
                    //product.LotNumberBatch = existingAcceptanceItem.LotNumberBatch;
                    //
                    product.DeliverFunction = existingAcceptanceItem.DeliverFunction;
                    product.DeliverTemperature = existingAcceptanceItem.DeliverTemperature;
                    product.SavingFunction = existingAcceptanceItem.SavingFunction;
                    product.SavingTemperature = existingAcceptanceItem.SavingTemperature;

                    _dbContext.TempInStockItemRecords.Add(tempInStockItemRecord);
                    _dbContext.InStockItemRecords.Add(inStockItemRecord);
                }
                //更新採購主單
                List<AcceptanceItem> existingAcceptanceItems = _dbContext.AcceptanceItems.Where(i=>i.PurchaseMainId == purchaseMain.PurchaseMainId && i.CompId==compId).ToList();

                if (existingAcceptanceItems.All(item => item.InStockStatus==CommonConstants.PurchaseSubItemReceiveStatus.DONE ))
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.ALL_ACCEPT;
                }
                else if (existingAcceptanceItems.Any(item => item.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.PART))
                {
                    purchaseMain.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.PART_ACCEPT;
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return (true,null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateAccepItem]：{msg}", ex);
                return (false,ex.Message);
            }

        }

        public List<AcceptanceItem> acceptanceItemsByUdiSerialCode(string udiserialCode,string compId)
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

        public List<InStockItemRecord> GetInStockRecordsHistoryByLotNumberBatch(string lotNumberBatch, string compId)
        {
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumberBatch == lotNumberBatch).ToList();
        }

        //public List<InStockItemRecord> GetProductInStockRecordsHistoryNotAllOutFIFO(string productCode, string compId)
        //{
        //    return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.ProductCode == productCode
        //    &&record.OutStockStatus!=CommonConstants.OutStockStatus.ALL).OrderBy(record=>record.CreatedAt).ToList();
        //}


        public List<InStockItemRecord> GetProductInStockRecordsHistoryNotAllOutExpirationFIFO(string lotNumberBatch, string compId)
        {
            // 先挑效期最早的相同的再依據FIFO
            return _dbContext.InStockItemRecords.Where(record => record.CompId == compId && record.LotNumberBatch == lotNumberBatch
            && record.OutStockStatus != CommonConstants.OutStockStatus.ALL).OrderBy(record => record.ExpirationDate).ThenBy(record=>record.CreatedAt).ToList();
        }
    }
}
