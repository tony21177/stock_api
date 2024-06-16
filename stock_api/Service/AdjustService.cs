using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
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
                            LotNumberBatch = item.LotNumberBatch??"AI"+nowDateTimeString,
                            BarCodeNumber = item.LotNumberBatch ?? "AI" + nowDateTimeString,
                            UserId = user.UserId,
                            UserName = user.DisplayName,
                            AfterQuantity = item.AfterQuantity,
                            AdjustItemId = adjustItem.AdjustItemId,
                        };
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
                            LotNumberBatch = item.LotNumberBatch ?? "AO" + nowDateTimeString,
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
                            BarCodeNumber = item.LotNumberBatch ?? "AO" + nowDateTimeString,
                            AdjustItemId = adjustItem.AdjustItemId
                        };
                        outStockRecords.Add(outStockRecord);
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
    }
}
