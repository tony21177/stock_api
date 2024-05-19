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
    public class StockOutService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<StockOutService> _logger;
        private readonly WarehouseProductService _warehouseProductService;  

        public StockOutService(StockDbContext dbContext, IMapper mapper, ILogger<StockOutService> logger,WarehouseProductService warehouseProductService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _warehouseProductService = warehouseProductService;
        }

        public bool OutStock(string outType,OutboundRequest request,InStockItemRecord inStockItem,WarehouseProduct product,WarehouseMember applyUser,string compId)
        {
            using var scope = new TransactionScope();
            try
            {

                int inStockQuantity = inStockItem.InStockQuantity;
                int existingOutQuantity = inStockItem.OutStockQuantity ?? 0;
                existingOutQuantity += request.ApplyQuantity;
                inStockItem.OutStockQuantity = existingOutQuantity;
                if (existingOutQuantity >= inStockQuantity)
                {
                    inStockItem.OutStockStatus = CommonConstants.OutStockStatus.ALL;
                }else if (existingOutQuantity < inStockQuantity)
                {
                    inStockItem.OutStockStatus = CommonConstants.OutStockStatus.PART;
                }

                var outStockId = Guid.NewGuid().ToString();
                // temp_out_stock_record
                var tempOutStockRecord = new TempOutStockRecord()
                {
                    OutStockId = outStockId,
                    AbnormalReason = request.AbnormalReason,
                    ApplyQuantity = request.ApplyQuantity,
                    IsAbnormal = request.IsAbnormal,
                    LotNumber = inStockItem.LotNumber,
                    LotNumberBatch = request.LotNumberBatch,
                    CompId = compId,
                    ProductId = inStockItem.ProductId,
                    ProductCode = inStockItem.ProductCode,
                    ProductName = inStockItem.ProductName,
                    ProductSpec = inStockItem.ProductSpec,
                    Type = outType,
                    UserId = applyUser.UserId,
                    UserName = applyUser.DisplayName,
                    OriginalQuantity = product.InStockQuantity ?? 0,
                    IsTransfer = true,
                    ExpirationDate = inStockItem.ExpirationDate
                };
                var test = (product.InStockQuantity ?? 0) - request.ApplyQuantity;

                var outStockRecord = new OutStockRecord()
                {
                    OutStockId = outStockId,
                    AbnormalReason = request.AbnormalReason,
                    ApplyQuantity = request.ApplyQuantity,
                    IsAbnormal = request.IsAbnormal,
                    LotNumber = inStockItem.LotNumber,
                    LotNumberBatch = request.LotNumberBatch,
                    CompId = compId,
                    ProductId = inStockItem.ProductId,
                    ProductCode = inStockItem.ProductCode,
                    ProductName = inStockItem.ProductName,
                    ProductSpec = inStockItem.ProductSpec,
                    Type = outType,
                    UserId = applyUser.UserId,
                    UserName = applyUser.DisplayName,
                    OriginalQuantity = product.InStockQuantity ?? 0,
                    AfterQuantity = (product.InStockQuantity ?? 0) - request.ApplyQuantity,
                    ItemId = inStockItem.ItemId,
                    BarCodeNumber = inStockItem.BarCodeNumber,
                    ExpirationDate = inStockItem.ExpirationDate
                };
               

                _dbContext.TempOutStockRecords.Add(tempOutStockRecord);
                _dbContext.OutStockRecords.Add(outStockRecord);
                var outStockRelateToInStock = new OutstockRelatetoInstock()
                {
                    OutStockId = outStockId,
                    InStockId = inStockItem.InStockId,
                    LotNumber = inStockItem.LotNumber,
                    LotNumberBatch = request.LotNumberBatch,
                    Quantity = request.ApplyQuantity,
                };
                _dbContext.OutstockRelatetoInstocks.Add(outStockRelateToInStock);
                // 更新庫存
                product.LotNumber = inStockItem.LotNumber;
                product.LotNumberBatch = request.LotNumberBatch;
                product.InStockQuantity = product.InStockQuantity - request.ApplyQuantity;
                DateOnly nowDate = DateOnly.FromDateTime(DateTime.Now);
                if (product.OpenDeadline != null)
                {
                    product.LastAbleDate = nowDate.AddDays(product.OpenDeadline.Value);
                }
                product.LastOutStockDate = nowDate;
                product.OriginalDeadline = inStockItem.ExpirationDate;


                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OutStock]：{msg}", ex);
                return false;
            }
        }


        public bool OwnerOutStock(string outType,OwnerOutboundRequest request, InStockItemRecord inStockItem, WarehouseProduct product, WarehouseMember applyUser,AcceptanceItem? toCompAcceptanceItem,string compId)
        {
            using var scope = new TransactionScope();
            try
            {

                int inStockQuantity = inStockItem.InStockQuantity;
                int existingOutQuantity = inStockItem.OutStockQuantity ?? 0;
                existingOutQuantity += request.ApplyQuantity;
                inStockItem.OutStockQuantity = existingOutQuantity;
                if (existingOutQuantity >= inStockQuantity)
                {
                    inStockItem.OutStockStatus = CommonConstants.OutStockStatus.ALL;
                }
                else if (existingOutQuantity < inStockQuantity)
                {
                    inStockItem.OutStockStatus = CommonConstants.OutStockStatus.PART;
                }

                var outStockId = Guid.NewGuid().ToString();
                // temp_out_stock_record
                var tempOutStockRecord = new TempOutStockRecord()
                {
                    OutStockId = outStockId,
                    AbnormalReason = request.AbnormalReason,
                    ApplyQuantity = request.ApplyQuantity,
                    IsAbnormal = request.IsAbnormal,
                    LotNumber = inStockItem.LotNumber,
                    LotNumberBatch = request.LotNumberBatch,
                    CompId = compId,
                    ProductId = inStockItem.ProductId,
                    ProductCode = inStockItem.ProductCode,
                    ProductName = inStockItem.ProductName,
                    ProductSpec = inStockItem.ProductSpec,
                    Type = outType,
                    UserId = applyUser.UserId,
                    UserName = applyUser.DisplayName,
                    OriginalQuantity = product.InStockQuantity ?? 0,
                    IsTransfer = true,
                    ExpirationDate = inStockItem.ExpirationDate
                };

                var outStockRecord = new OutStockRecord()
                {
                    OutStockId = outStockId,
                    AbnormalReason = request.AbnormalReason,
                    ApplyQuantity = request.ApplyQuantity,
                    IsAbnormal = request.IsAbnormal,
                    LotNumber = inStockItem.LotNumber,
                    LotNumberBatch = request.LotNumberBatch,
                    CompId = compId,
                    ProductId = inStockItem.ProductId,
                    ProductCode = inStockItem.ProductCode,
                    ProductName = inStockItem.ProductName,
                    ProductSpec = inStockItem.ProductSpec,
                    Type = outType,
                    UserId = applyUser.UserId,
                    UserName = applyUser.DisplayName,
                    OriginalQuantity = product.InStockQuantity ?? 0,
                    AfterQuantity = product.InStockQuantity ?? 0 - request.ApplyQuantity,
                    ItemId = inStockItem.ItemId,
                    BarCodeNumber = inStockItem.BarCodeNumber,
                    ExpirationDate = inStockItem.ExpirationDate
                };


                _dbContext.TempOutStockRecords.Add(tempOutStockRecord);
                _dbContext.OutStockRecords.Add(outStockRecord);
                var outStockRelateToInStock = new OutstockRelatetoInstock()
                {
                    OutStockId = outStockId,
                    InStockId = inStockItem.InStockId,
                    LotNumber = inStockItem.LotNumber,
                    LotNumberBatch = request.LotNumberBatch,
                    Quantity = request.ApplyQuantity,
                };
                _dbContext.OutstockRelatetoInstocks.Add(outStockRelateToInStock);
                // 更新庫存
                product.LotNumber = inStockItem.LotNumber;
                product.LotNumberBatch = request.LotNumberBatch;
                product.InStockQuantity -= request.ApplyQuantity;
                DateOnly nowDate = DateOnly.FromDateTime(DateTime.Now);
                if (product.OpenDeadline != null)
                {
                    product.LastAbleDate = nowDate.AddDays(product.OpenDeadline.Value);
                }
                product.LastOutStockDate = nowDate;
                product.OriginalDeadline = inStockItem.ExpirationDate;

                if(outType == CommonConstants.OutStockType.SHIFT_OUT && toCompAcceptanceItem != null)
                {
                    var toProduct = _warehouseProductService.GetProductByProductCodeAndCompId(product.ProductCode,toCompAcceptanceItem.CompId);
                    // TODO:跟Gary確認這樣轉換對不對
                    toCompAcceptanceItem.AcceptQuantity = request.ApplyQuantity* (toProduct.UnitConversion??1);
                    toCompAcceptanceItem.LotNumber = inStockItem.LotNumber;
                    toCompAcceptanceItem.LotNumberBatch = request.LotNumberBatch;
                    toCompAcceptanceItem.ExpirationDate = inStockItem.ExpirationDate;
                }

                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OutStock]：{msg}", ex);
                return false;
            }
        }

        public (List<OutStockRecord>, int TotalPages) ListStockOutRecords(ListStockOutRecordsRequest request)
        {
            IQueryable<OutStockRecord> query = _dbContext.OutStockRecords;
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
            if (request.ProductCode != null)
            {
                query = query.Where(h => h.ProductCode == request.ProductCode);
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

            query = query.Where(h => h.CompId == request.CompId);

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                var groupNameList =
                query = query.Where(h => h.LotNumberBatch.Contains(request.Keywords)
                || h.AbnormalReason.Contains(request.Keywords)
                || h.LotNumber.Contains(request.Keywords)
                || h.ProductId.Contains(request.Keywords)
                || h.ProductCode.Contains(request.Keywords)
                || h.ProductName.Contains(request.Keywords)
                || h.ProductSpec.Contains(request.Keywords)
                || h.UserId.Contains(request.Keywords)
                || h.UserName.Contains(request.Keywords)
                );
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
    }
}
