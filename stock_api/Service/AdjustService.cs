using MySqlX.XDevAPI.Common;
using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Transactions;

namespace stock_api.Service
{
    public class AdjustService
    {
        private readonly StockDbContext _dbContext;
        private readonly StockInService _stockInService;
        private readonly StockOutService _stockOutService;
        private readonly ILogger<AdjustService> _logger;

        public AdjustService(StockDbContext dbContext, StockInService stockInService, StockOutService stockOutService, ILogger<AdjustService> logger)
        {
            _dbContext = dbContext;
            _stockInService = stockInService;
            _stockOutService = stockOutService;
            _logger = logger;
        }

        public (bool,string?) AdjustItems(List<AdjustItem> adjustItems,List<WarehouseProduct> products,WarehouseMember user)
        {   
            using var scope = new TransactionScope();
            try
            {
                InventoryAdjustMain main = new ()
                {
                    MainId = Guid.NewGuid().ToString(),
                    CompId = user.CompId,
                    Type = CommonConstants.AdjustType.ADJUST,
                    UserId = user.UserId,
                    CurrentStatus = CommonConstants.AdjustStatus.AGREE,
                };
                List<InventoryAdjustItem> inventoryAdjustItems = new();
                List<InStockItemRecord> inStockItemRecords = new();
                List<OutStockRecord> outStockRecords = new();
                foreach (var item in adjustItems)
                {
                    var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                    var now = DateTime.Now;
                    var nowDateTimeString = DateTimeHelper.FormatDateString(now, "yyyyMMddHHmm");
                    if (item.BeforeQuantity < item.AfterQuantity)
                    {
                        //盤盈
                        InventoryAdjustItem adjustItem = new ()
                        {
                            AdjustItemId = Guid.NewGuid().ToString(),
                            MainId = main.MainId,
                            ProductId = item.ProductId,
                            ProductCode = matchedProduct.ProductCode,
                            CompId = user.CompId,
                            BeforeQuantity = item.BeforeQuantity,
                            AfterQuantity = item.AfterQuantity,
                        };
                        inventoryAdjustItems.Add(adjustItem);
                        InStockItemRecord record = new() 
                        { 
                            InStockId = Guid.NewGuid().ToString(),
                            CompId = user.CompId,
                            OriginalQuantity = item.BeforeQuantity,
                            InStockQuantity = item.AfterQuantity - item.BeforeQuantity,
                            ProductId = item.ProductId,
                            ProductCode = matchedProduct.ProductCode,
                            ProductName = matchedProduct.ProductName,
                            ProductSpec = matchedProduct.ProductSpec,
                            Type = CommonConstants.StockInType.ADJUST,
                            LotNumber = item.LotNumber,
                            LotNumberBatch = item.LotNumberBatch??matchedProduct.ProductCode+"AI"+nowDateTimeString,
                            BarCodeNumber = item.LotNumberBatch ?? matchedProduct.ProductCode + "AI" + nowDateTimeString,
                            UserId = user.UserId,
                            UserName = user.DisplayName,
                            AfterQuantity = item.AfterQuantity,
                            AdjustItemId = adjustItem.AdjustItemId,
                        };
                        matchedProduct.InStockQuantity = item.AfterQuantity;
                        inStockItemRecords.Add(record);

                    }

                    if (item.BeforeQuantity > item.AfterQuantity)
                    {
                        //盤虧
                        InventoryAdjustItem adjustItem = new()
                        {
                            AdjustItemId = Guid.NewGuid().ToString(),
                            MainId = main.MainId,
                            ProductId = item.ProductId,
                            ProductCode = matchedProduct.ProductCode,
                            CompId = user.CompId,
                            BeforeQuantity = item.BeforeQuantity,
                            AfterQuantity = item.AfterQuantity,
                        };
                        inventoryAdjustItems.Add(adjustItem);
                        OutStockRecord outStockRecord = new()
                        {
                            OutStockId = Guid.NewGuid().ToString(),
                            ApplyQuantity = item.BeforeQuantity - item.AfterQuantity,
                            LotNumberBatch = item.LotNumberBatch ?? matchedProduct.ProductCode + "AO" + nowDateTimeString,
                            LotNumber = item.LotNumber,
                            CompId = user.CompId,
                            IsAbnormal = false,
                            ProductId = item.ProductId,
                            ProductCode = matchedProduct.ProductCode,
                            ProductName = matchedProduct.ProductName,
                            ProductSpec = matchedProduct.ProductSpec,
                            Type = CommonConstants.OutStockType.ADJUST_OUT,
                            UserId = user.UserId,
                            UserName = user.DisplayName,
                            OriginalQuantity = item.BeforeQuantity,
                            AfterQuantity = item.AfterQuantity,
                            BarCodeNumber = matchedProduct.ProductCode + "AO" + nowDateTimeString,
                            AdjustItemId = adjustItem.AdjustItemId
                        };
                        matchedProduct.InStockQuantity = item.AfterQuantity;
                        outStockRecords.Add(outStockRecord);
                    }
                    if(item.BeforeQuantity == item.AfterQuantity)
                    {
                        InventoryAdjustItem adjustItem = new()
                        {
                            AdjustItemId = Guid.NewGuid().ToString(),
                            MainId = main.MainId,
                            ProductId = item.ProductId,
                            ProductCode = matchedProduct.ProductCode,
                            CompId = user.CompId,
                            BeforeQuantity = item.BeforeQuantity,
                            AfterQuantity = item.AfterQuantity,
                        };
                        inventoryAdjustItems.Add(adjustItem);
                    }

                }
                _dbContext.InventoryAdjustMains.Add(main);
                if (inventoryAdjustItems.Count > 0)
                {
                    _dbContext.InventoryAdjustItems.AddRange(inventoryAdjustItems);
                }
                if (inStockItemRecords.Count > 0)
                {
                    _dbContext.InStockItemRecords.AddRange(inStockItemRecords);
                }
                if (outStockRecords.Count > 0)
                {
                    _dbContext.OutStockRecords.AddRange(outStockRecords);
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[AdjustItems]：{msg}", ex);
                return (false,ex.Message);
            }
        }

        public (List<AdjustMainWithItemsVo>,int) ListAdjustMainWithItemsByCondition(ListAdjustItemsRequest request)
        {
            IQueryable<AdjustMainItemListView> query = _dbContext.AdjustMainItemListViews;
            if (request.CompId != null)
            {
                query = query.Where(h => h.CompId == request.CompId);
            }
            if (request.MainId != null)
            {
                query = query.Where(h => h.MainId == request.MainId);
            }
            if (request.AdjustCompId != null)
            {
                query = query.Where(h => h.AdjustCompId == request.AdjustCompId);
            }
            if (request.Type != null)
            {
                query = query.Where(h => h.Type == request.Type);
            }
            if (request.UserId != null)
            {
                query = query.Where(h => h.UserId == request.UserId);
            }
            if (request.CurrentStatus != null)
            {
                query = query.Where(h => h.CurrentStatus == request.CurrentStatus);
            }
            if (request.StartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(request.StartDate);
                query = query.Where(h => h.CreatedAt>=startDateTime);
            }
            if (request.EndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.EndDate).Value.AddDays(1);
                query = query.Where(h => h.CreatedAt < endDateTime);
            }
            if (request.ProductId != null)
            {
                query = query.Where(h => h.ProductId == request.ProductId);
            }
            if (request.ProductCode != null)
            {
                query = query.Where(h => h.ProductCode == request.ProductCode);
            }

            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "UpdatedAt";
            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
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
                    "CreatedAt" => query.OrderBy(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderBy(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.CreatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);

            query = query.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize);
            var itemsView = query.ToList();

            Dictionary<string, List<AdjustMainItemListView>> mainIdMap = new Dictionary<string, List<AdjustMainItemListView>>();
            foreach (var mainWithItem in itemsView)
            {
                if (!mainIdMap.ContainsKey(mainWithItem.MainId))
                {
                    mainIdMap.Add(mainWithItem.MainId, new List<AdjustMainItemListView>());
                }
                var voList = mainIdMap.GetValueOrDefault(mainWithItem.MainId);
                voList?.Add(mainWithItem);
            }

            List<AdjustMainWithItemsVo> adjustMainWithItemsVoList = new ();
            foreach (var kvp in mainIdMap)
            {
                List<AdjustItemVo> Items = new List<AdjustItemVo>();
                kvp.Value.ForEach(vo =>
                {
                    var adjustItemVo = new AdjustItemVo()
                    {
                        AdjustItemId = vo.AdjustItemId,
                        ProductId = vo.ProductId,
                        ProductCode = vo.ProductCode,
                        BeforeQuantity = vo.BeforeQuantity,
                        AfterQuantity = vo.AfterQuantity,
                        ItemCreatedAt = vo.ItemCreatedAt,
                        ItemUpdatedAt = vo.ItemUpdatedAt,
                    };
                    Items.Add(adjustItemVo);
                });

                var mainVo = new AdjustMainWithItemsVo
                {
                    MainId = kvp.Key,
                    CompId = kvp.Value[0].CompId,
                    AdjustCompId = kvp.Value[0].AdjustCompId,
                    Type = kvp.Value[0].Type,
                    UserId = kvp.Value[0].UserId,
                    CurrentStatus = kvp.Value[0].CurrentStatus,
                    CreatedAt = kvp.Value[0].CreatedAt,
                    UpdatedAt = kvp.Value[0].UpdatedAt,
                };
                mainVo.Items = Items;
                adjustMainWithItemsVoList.Add(mainVo);
            }
            return (adjustMainWithItemsVoList,totalPages);

        }
    }
}
