using AutoMapper;
using Microsoft.VisualBasic;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.AccessControl;
using System.Transactions;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace stock_api.Service
{
    public class StockOutService
    {
        private readonly StockDbContext _dbContext;
        private readonly IDbContextFactory<StockDbContext> _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<StockOutService> _logger;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly IMemoryCache _memoryCache;

        // Cache keys
        private const string CacheKeyLastMonthUsage = "usage_last_month";
        private const string CacheKeyThisYearAvgUsage = "usage_this_year_avg";
        private const string CacheKeyLastYearUsage = "usage_last_year";
        
        // Cache duration settings
        // AbsoluteExpiration: 12 小時後強制失效
        private static readonly TimeSpan CacheAbsoluteExpiration = TimeSpan.FromHours(12);
        // SlidingExpiration: 2 小時沒人讀取就失效
        private static readonly TimeSpan CacheSlidingExpiration = TimeSpan.FromHours(2);

        public StockOutService(StockDbContext dbContext, IMapper mapper, ILogger<StockOutService> logger, WarehouseProductService warehouseProductService, IDbContextFactory<StockDbContext> dbContextFactory, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _warehouseProductService = warehouseProductService;
            _dbContextFactory = dbContextFactory;
            _memoryCache = memoryCache;
        }

        // Batching helper size
        private const int InClauseBatchSize = 200;

        public (bool, string?, NotifyProductQuantity?) OutStock(string outType, OutboundRequest request, InStockItemRecord inStockItem, WarehouseProduct product, WarehouseMember applyUser, string compId)
        {
            using var scope = new TransactionScope();
            try
            {

                float inStockQuantity = inStockItem.InStockQuantity + inStockItem.AdjustInQuantity;
                float existingOutQuantity = inStockItem.OutStockQuantity + inStockItem.AdjustOutQuantity + inStockItem.RejectQuantity;
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
                    AfterQuantity = (product.InStockQuantity ?? 0) - request.ApplyQuantity,
                    ItemId = inStockItem.ItemId,
                    BarCodeNumber = inStockItem.BarCodeNumber,
                    ExpirationDate = inStockItem.ExpirationDate,
                    SkipQcCommnet = request.IsSkipQc ? request.SkipQcComment : "",
                    Remark = request.Remark,
                    InstrumentId = request.InstrumentId,
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

                product.InStockQuantity = product.InStockQuantity - request.ApplyQuantity;
                DateOnly nowDate = DateOnly.FromDateTime(DateTime.Now);
                if (product.OpenDeadline != null && outType == CommonConstants.OutStockType.PURCHASE_OUT)
                {
                    product.LastAbleDate = nowDate.AddDays(product.OpenDeadline.Value);
                }
                if (outType == CommonConstants.OutStockType.PURCHASE_OUT)
                {
                    product.LotNumber = inStockItem.LotNumber;
                    product.LotNumberBatch = request.LotNumberBatch;
                    product.LastOutStockDate = nowDate;
                    product.OriginalDeadline = inStockItem.ExpirationDate;
                }


                NotifyProductQuantity notifyProductQuantity = new()
                {
                    ProductCode = product.ProductCode,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    InStockQuantity = product.InStockQuantity ?? 0.0f,
                    SafeQuantity = product.SafeQuantity ?? 0.0f,
                    MaxSafeQuantity = product.MaxSafeQuantity,
                    OutStockQuantity = request.ApplyQuantity,
                    CompId = product.CompId,
                };

                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null, notifyProductQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OutStock]：{msg}", ex);
                return (false, ex.Message, null);
            }
        }


        public (bool, string?, NotifyProductQuantity?) OwnerOutStock(string outType, OwnerOutboundRequest request, InStockItemRecord inStockItem, WarehouseProduct product, WarehouseMember applyUser, AcceptanceItem? toCompAcceptanceItem, string compId)
        {
            using var scope = new TransactionScope();
            try
            {

                float inStockQuantity = inStockItem.InStockQuantity + inStockItem.AdjustInQuantity;
                float existingOutQuantity = inStockItem.OutStockQuantity + inStockItem.AdjustOutQuantity + inStockItem.RejectQuantity;
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

                if (outType == CommonConstants.OutStockType.SHIFT_OUT && toCompAcceptanceItem != null)
                {
                    var toProduct = _warehouseProductService.GetProductByProductCodeAndCompId(product.ProductCode, toCompAcceptanceItem.CompId);
                    // TODO:跟Gary確認這樣轉換對不對
                    toCompAcceptanceItem.AcceptQuantity = request.ApplyQuantity * (toProduct.UnitConversion ?? 1);
                    toCompAcceptanceItem.LotNumber = inStockItem.LotNumber;
                    toCompAcceptanceItem.LotNumberBatch = request.LotNumberBatch;
                    toCompAcceptanceItem.ExpirationDate = inStockItem.ExpirationDate;
                }

                NotifyProductQuantity notifyProductQuantity = new()
                {
                    ProductCode = product.ProductCode,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    InStockQuantity = product.InStockQuantity ?? 0.0f,
                    SafeQuantity = product.SafeQuantity ?? 0.0f,
                    MaxSafeQuantity = product.MaxSafeQuantity,
                    OutStockQuantity = request.ApplyQuantity,
                    CompId = product.CompId,
                };


                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null, notifyProductQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OutStock]：{msg}", ex);
                return (false, ex.Message, null);
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

        public List<LastMonthUsage> GetLastMonthUsages(List<string> productIdList)
        {
            return _dbContext.LastMonthUsages.Where(e => productIdList.Contains(e.ProductId)).ToList();
        }

        // Async version using DbContextFactory and ToListAsync
        public async Task<List<LastMonthUsage>> GetLastMonthUsagesAsync(List<string> productIdList)
        {
            if (productIdList == null || productIdList.Count == 0)
            {
                return new List<LastMonthUsage>();
            }

            var sw = Stopwatch.StartNew();
            var results = new List<LastMonthUsage>();
            // split into batches to avoid huge IN(...) lists
            for (int i = 0; i < productIdList.Count; i += InClauseBatchSize)
            {
                var batch = productIdList.Skip(i).Take(InClauseBatchSize).ToList();
                using var ctx = _dbContextFactory.CreateDbContext();
                var res = await ctx.LastMonthUsages.AsNoTracking().Where(e => batch.Contains(e.ProductId)).ToListAsync();
                if (res != null && res.Count > 0) results.AddRange(res);
            }
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetLastMonthUsagesAsync elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, results?.Count ?? 0);
            return results;
        }

        public List<LastMonthUsage> GetLastMonthUsages()
        {
            return _dbContext.LastMonthUsages.ToList();
        }

        public async Task<List<LastMonthUsage>> GetLastMonthUsagesAsync()
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var sw = Stopwatch.StartNew();
            var res = await ctx.LastMonthUsages.AsNoTracking().ToListAsync();
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetLastMonthUsagesAsync (all) elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, res?.Count ?? 0);
            return res;
        }

        public List<AverageMonthUsageThisYear> GetThisAverageMonthUsages(List<string> productIdList)
        {
            return _dbContext.AverageMonthUsageThisYears.Where(e => productIdList.Contains(e.ProductId)).ToList();
        }

        public async Task<List<AverageMonthUsageThisYear>> GetThisAverageMonthUsagesAsync(List<string> productIdList)
        {
            if (productIdList == null || productIdList.Count == 0)
            {
                return new List<AverageMonthUsageThisYear>();
            }
            var sw = Stopwatch.StartNew();
            var results = new List<AverageMonthUsageThisYear>();
            for (int i = 0; i < productIdList.Count; i += InClauseBatchSize)
            {
                var batch = productIdList.Skip(i).Take(InClauseBatchSize).ToList();
                using var ctx = _dbContextFactory.CreateDbContext();
                var res = await ctx.AverageMonthUsageThisYears.AsNoTracking().Where(e => batch.Contains(e.ProductId)).ToListAsync();
                if (res != null && res.Count > 0) results.AddRange(res);
            }
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetThisAverageMonthUsagesAsync elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, results?.Count ?? 0);
            return results;
        }

        public List<AverageMonthUsageThisYear> GetThisAverageMonthUsages()
        {
            return _dbContext.AverageMonthUsageThisYears.ToList();
        }

        public async Task<List<AverageMonthUsageThisYear>> GetThisAverageMonthUsagesAsync()
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var sw = Stopwatch.StartNew();
            var res = await ctx.AverageMonthUsageThisYears.AsNoTracking().ToListAsync();
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetThisAverageMonthUsagesAsync (all) elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, res?.Count ?? 0);
            return res;
        }

        public List<LastYearUsage> GetLastYearUsages()
        {
            return _dbContext.LastYearUsages.ToList();
        }

        public async Task<List<LastYearUsage>> GetLastYearUsagesAsync()
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var sw = Stopwatch.StartNew();
            var res = await ctx.LastYearUsages.AsNoTracking().ToListAsync();
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetLastYearUsagesAsync (all) elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, res?.Count ?? 0);
            return res;
        }

        public List<LastYearUsage> GetLastYearUsages(List<string> productIdList)
        {
            return _dbContext.LastYearUsages.Where(e => productIdList.Contains(e.ProductId)).ToList();
        }

        public async Task<List<LastYearUsage>> GetLastYearUsagesAsync(List<string> productIdList)
        {
            if (productIdList == null || productIdList.Count == 0)
            {
                return new List<LastYearUsage>();
            }
            var sw = Stopwatch.StartNew();
            var results = new List<LastYearUsage>();
            for (int i = 0; i < productIdList.Count; i += InClauseBatchSize)
            {
                var batch = productIdList.Skip(i).Take(InClauseBatchSize).ToList();
                using var ctx = _dbContextFactory.CreateDbContext();
                var res = await ctx.LastYearUsages.AsNoTracking().Where(e => batch.Contains(e.ProductId)).ToListAsync();
                if (res != null && res.Count > 0) results.AddRange(res);
            }
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetLastYearUsagesAsync elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, results?.Count ?? 0);
            return results;
        }

        /// <summary>
        /// Combined async query to fetch last month, this year average and last year usages in parallel and return a single list per product.
        /// Optimized version: Uses MemoryCache with 12-hour absolute expiration and 2-hour sliding expiration, SELECT ALL from each view, then filter in memory using HashSet for O(1) lookup.
        /// </summary>
        public async Task<List<CombinedUsageVo>> GetCombinedUsagesAsync(List<string> productIdList)
        {
            if (productIdList == null || productIdList.Count == 0)
            {
                return new List<CombinedUsageVo>();
            }

            var sw = Stopwatch.StartNew();

            // Build HashSet for O(1) lookup
            var productIdSet = new HashSet<string>(productIdList);

            // Try get from cache, or load from DB
            var lastMonthList = await _memoryCache.GetOrCreateAsync(CacheKeyLastMonthUsage, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheAbsoluteExpiration;
                entry.SlidingExpiration = CacheSlidingExpiration;
                using var ctx = _dbContextFactory.CreateDbContext();
                var swDb = Stopwatch.StartNew();
                var data = await ctx.LastMonthUsages.AsNoTracking().ToListAsync();
                swDb.Stop();
                _logger.LogInformation("[StockOutService] Cache MISS - LastMonthUsage loaded from DB: {ms}ms, count: {count}", swDb.ElapsedMilliseconds, data?.Count ?? 0);
                return data ?? new List<LastMonthUsage>();
            });

            var thisYearList = await _memoryCache.GetOrCreateAsync(CacheKeyThisYearAvgUsage, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheAbsoluteExpiration;
                entry.SlidingExpiration = CacheSlidingExpiration;
                using var ctx = _dbContextFactory.CreateDbContext();
                var swDb = Stopwatch.StartNew();
                var data = await ctx.AverageMonthUsageThisYears.AsNoTracking().ToListAsync();
                swDb.Stop();
                _logger.LogInformation("[StockOutService] Cache MISS - ThisYearAvgUsage loaded from DB: {ms}ms, count: {count}", swDb.ElapsedMilliseconds, data?.Count ?? 0);
                return data ?? new List<AverageMonthUsageThisYear>();
            });

            var lastYearList = await _memoryCache.GetOrCreateAsync(CacheKeyLastYearUsage, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheAbsoluteExpiration;
                entry.SlidingExpiration = CacheSlidingExpiration;
                using var ctx = _dbContextFactory.CreateDbContext();
                var swDb = Stopwatch.StartNew();
                var data = await ctx.LastYearUsages.AsNoTracking().ToListAsync();
                swDb.Stop();
                _logger.LogInformation("[StockOutService] Cache MISS - LastYearUsage loaded from DB: {ms}ms, count: {count}", swDb.ElapsedMilliseconds, data?.Count ?? 0);
                return data ?? new List<LastYearUsage>();
            });

            var swFilter = Stopwatch.StartNew();

            // Filter in memory using HashSet (O(1) per lookup)
            var lastMonthLookup = lastMonthList
                .Where(u => productIdSet.Contains(u.ProductId))
                .ToDictionary(u => u.ProductId, u => u.Quantity);
            var thisYearLookup = thisYearList
                .Where(u => productIdSet.Contains(u.ProductId))
                .ToDictionary(u => u.ProductId, u => u.AverageQuantity);
            var lastYearLookup = lastYearList
                .Where(u => productIdSet.Contains(u.ProductId))
                .ToDictionary(u => u.ProductId, u => u.Quantity);

            // Build result
            var result = new List<CombinedUsageVo>(productIdList.Count);
            foreach (var pid in productIdList)
            {
                lastMonthLookup.TryGetValue(pid, out var lm);
                thisYearLookup.TryGetValue(pid, out var ty);
                lastYearLookup.TryGetValue(pid, out var ly);

                result.Add(new CombinedUsageVo
                {
                    ProductId = pid,
                    LastMonthQuantity = lm,
                    ThisYearAverageQuantity = ty,
                    LastYearQuantity = ly,
                });
            }

            swFilter.Stop();
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetCombinedUsagesAsync(cached) elapsed: {ms}ms (filter: {filterMs}ms), productCount: {count}, lastMonth: {lm}, thisYear: {ty}, lastYear: {ly}",
                sw.ElapsedMilliseconds, swFilter.ElapsedMilliseconds, productIdList.Count, lastMonthList.Count, thisYearList.Count, lastYearList.Count);
            return result;
        }

        /// <summary>
        /// 清除 usage cache (可在出庫後呼叫，確保下次查詢時會重新載入)
        /// </summary>
        public void InvalidateUsageCache()
        {
            _memoryCache.Remove(CacheKeyLastMonthUsage);
            _memoryCache.Remove(CacheKeyThisYearAvgUsage);
            _memoryCache.Remove(CacheKeyLastYearUsage);
            _logger.LogInformation("[StockOutService] Usage cache invalidated");
        }

        public OutStockRecord? GetOutStockRecordById(string outStockId)
        {
            return _dbContext.OutStockRecords.Where(r => r.OutStockId == outStockId).FirstOrDefault();
        }

        public List<OutStockRecord> GetOutStockRecordsByLotNumberBatch(string lotNumberBatch)
        {
            return _dbContext.OutStockRecords.Where(r => r.LotNumberBatch == lotNumberBatch).ToList();
        }

        public List<OutStockRecord> GetOutStockRecordsByLotNumberList(List<string> lotNumberList)
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.OutStockRecords.Where(r => r.LotNumber != null && lotNumberList.Contains(r.LotNumber)).ToList();
            sw.Stop();
            _logger.LogInformation("[StockOutService.GetOutStockRecordsByLotNumberList] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public async Task<List<OutStockRecord>> GetOutStockRecordsByLotNumberListAsync(List<string> lotNumberList)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var sw = Stopwatch.StartNew();
            var result = await ctx.OutStockRecords.AsNoTracking().Where(r => r.LotNumber != null && lotNumberList.Contains(r.LotNumber)).ToListAsync();
            sw.Stop();
            _logger.LogInformation("[StockOutService] GetOutStockRecordsByLotNumberListAsync elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result?.Count ?? 0);
            return result;
        }

        public List<OutStockRecord> GetOutStockRecordsByLotNumberBatchList(List<string> lotNumberBatchList)
        {
            var sw = Stopwatch.StartNew();
            var result = _dbContext.OutStockRecords.Where(r => r.LotNumberBatch != null && lotNumberBatchList.Contains(r.LotNumberBatch)).ToList();
            sw.Stop();
            _logger.LogInformation("[StockOutService.GetOutStockRecordsByLotNumberBatchList] elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, result.Count);
            return result;
        }

        public List<ReturnStockRecord> GetReturnStockRecords(string compId)
        {
            return _dbContext.ReturnStockRecords.Where(r => r.CompId == compId).OrderByDescending(r => r.CreatedAt).ToList();
        }

        public (bool, string?) OwnerDirectBatchOut(OwnerDirectBatchOutboundRequest request, List<WarehouseProduct> products, WarehouseMember user)
        {

            using var scope = new TransactionScope();
            try
            {
                List<OutStockRecord> outStockRecords = new List<OutStockRecord>();
                List<OutstockRelatetoInstock> outStockRelateToInStocks = new List<OutstockRelatetoInstock>();

                // 取得所有相關產品的入庫紀錄，依照 CreatedAt 排序 (FIFO: 先進先出)
                var productIds = request.OutItems.Select(i => i.ProductId).ToList();
                var allInStockRecords = _dbContext.InStockItemRecords
                    .Where(r => productIds.Contains(r.ProductId) 
                           && r.CompId == request.CompId 
                           && r.OutStockStatus != CommonConstants.OutStockStatus.ALL)
                    .OrderBy(r => r.CreatedAt)
                    .ToList();

                foreach (var item in request.OutItems)
                {
                    var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                    
                    // 取得該產品的入庫紀錄 (已按 CreatedAt 排序，FIFO)
                    var productInStockRecords = allInStockRecords
                        .Where(r => r.ProductId == item.ProductId)
                        .ToList();

                    float remainingOutQuantity = item.OutQuantity;
                    float originalProductQuantity = matchedProduct.InStockQuantity.Value;
                    
                    // 追蹤當前庫存量，每次出庫後遞減
                    float currentProductQuantity = originalProductQuantity;
                    
                    // 用來記錄最後出庫的批號資訊 (用於更新 product)
                    string? lastLotNumber = null;
                    string? lastLotNumberBatch = null;
                    DateOnly? lastExpirationDate = null;

                    // FIFO: 依序從最早的入庫紀錄扣除出庫數量
                    foreach (var inStockRecord in productInStockRecords)
                    {
                        if (remainingOutQuantity <= 0) break;

                        // 計算該批次可用的數量
                        float availableQuantity = inStockRecord.InStockQuantity + inStockRecord.AdjustInQuantity 
                                                 - inStockRecord.OutStockQuantity - inStockRecord.AdjustOutQuantity 
                                                 - inStockRecord.RejectQuantity;

                        if (availableQuantity <= 0) continue;

                        // 計算這批要出多少
                        float quantityToOutFromThisBatch = Math.Min(remainingOutQuantity, availableQuantity);

                        // 更新 in_stock_item_record 的 OutStockQuantity
                        inStockRecord.OutStockQuantity += quantityToOutFromThisBatch;

                        // 更新 OutStockStatus
                        float totalInQuantity = inStockRecord.InStockQuantity + inStockRecord.AdjustInQuantity;
                        float totalOutQuantity = inStockRecord.OutStockQuantity + inStockRecord.AdjustOutQuantity + inStockRecord.RejectQuantity;
                        if (totalOutQuantity >= totalInQuantity)
                        {
                            inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.ALL;
                        }
                        else if (totalOutQuantity > 0)
                        {
                            inStockRecord.OutStockStatus = CommonConstants.OutStockStatus.PART;
                        }

                        // 計算該筆出庫後的庫存量
                        float afterQuantityForThisBatch = currentProductQuantity - quantityToOutFromThisBatch;

                        // 建立出庫紀錄
                        var outStockId = Guid.NewGuid().ToString();
                        OutStockRecord outStockRecord = new OutStockRecord()
                        {
                            OutStockId = outStockId,
                            ApplyQuantity = quantityToOutFromThisBatch,
                            LotNumber = inStockRecord.LotNumber,
                            LotNumberBatch = inStockRecord.LotNumberBatch,
                            CompId = request.CompId,
                            IsAbnormal = false,
                            ProductId = item.ProductId,
                            ProductCode = matchedProduct.ProductCode,
                            ProductName = matchedProduct.ProductName,
                            ProductSpec = matchedProduct.ProductSpec,
                            Type = CommonConstants.OutStockType.OWNER_DIRECT_OUT,
                            UserId = user.UserId,
                            UserName = user.DisplayName,
                            OriginalQuantity = currentProductQuantity,  // 該筆出庫前的當前庫存
                            AfterQuantity = afterQuantityForThisBatch,  // 該筆出庫後的庫存
                            BarCodeNumber = inStockRecord.BarCodeNumber,
                            ItemId = inStockRecord.ItemId,
                            ExpirationDate = inStockRecord.ExpirationDate,
                            Remark = item.Remark,
                        };
                        outStockRecords.Add(outStockRecord);

                        // 建立出庫與入庫的關聯
                        var outStockRelateToInStock = new OutstockRelatetoInstock()
                        {
                            OutStockId = outStockId,
                            InStockId = inStockRecord.InStockId,
                            LotNumber = inStockRecord.LotNumber,
                            LotNumberBatch = inStockRecord.LotNumberBatch,
                            Quantity = quantityToOutFromThisBatch,
                        };
                        outStockRelateToInStocks.Add(outStockRelateToInStock);

                        // 記錄最後出庫的批號資訊
                        lastLotNumber = inStockRecord.LotNumber;
                        lastLotNumberBatch = inStockRecord.LotNumberBatch;
                        lastExpirationDate = inStockRecord.ExpirationDate;

                        // 更新當前庫存量和剩餘待出數量
                        currentProductQuantity = afterQuantityForThisBatch;
                        remainingOutQuantity -= quantityToOutFromThisBatch;
                    }

                    // 更新庫存品項
                    matchedProduct.InStockQuantity = originalProductQuantity - item.OutQuantity;
                    if (lastLotNumber != null) matchedProduct.LotNumber = lastLotNumber;
                    if (lastLotNumberBatch != null) matchedProduct.LotNumberBatch = lastLotNumberBatch;
                    if (lastExpirationDate.HasValue) matchedProduct.OriginalDeadline = lastExpirationDate;

                    DateOnly nowDate = DateOnly.FromDateTime(DateTime.Now);
                    if (matchedProduct.OpenDeadline != null)
                    {
                        matchedProduct.LastAbleDate = nowDate.AddDays(matchedProduct.OpenDeadline.Value);
                    }
                    matchedProduct.LastOutStockDate = nowDate;
                }

                _dbContext.OutStockRecords.AddRange(outStockRecords);
                _dbContext.OutstockRelatetoInstocks.AddRange(outStockRelateToInStocks);
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);

            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OwnerDirectBatchOut]：{msg}", ex);
                return (false, ex.Message);
            }
        }

        public (bool, string?) OwnerPurchaseSubItemsBatchOut(OwnerPurchaseSubItemsOutRequest request, List<PurchaseSubItem> subItems, List<WarehouseProduct> ownerProducts, WarehouseMember user)
        {

            using var scope = new TransactionScope();
            try
            {
                List<OutStockRecord> outStockRecords = new List<OutStockRecord>();

                foreach (var item in request.PurchaseSubOutItems)
                {
                    var matchedSubItem = subItems.Where(i => i.ItemId == item.SubItemId).FirstOrDefault();
                    var matchedProduct = ownerProducts.Where(p => p.ProductCode == matchedSubItem.ProductCode).FirstOrDefault();


                    string lotNumberBatch = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    OutStockRecord outStockRecord = new OutStockRecord()
                    {
                        OutStockId = Guid.NewGuid().ToString(),
                        ApplyQuantity = item.OutQuantity,
                        LotNumberBatch = lotNumberBatch,
                        CompId = request.CompId,
                        IsAbnormal = false,
                        ProductId = matchedProduct.ProductId,
                        ProductCode = matchedProduct.ProductCode,
                        ProductName = matchedProduct.ProductName,
                        ProductSpec = matchedProduct.ProductSpec,
                        Type = CommonConstants.OutStockType.OWNER_DIRECT_OUT_BY_SUB_ITEM,
                        UserId = user.UserId,
                        UserName = user.DisplayName,
                        OriginalQuantity = matchedProduct.InStockQuantity.Value,
                        AfterQuantity = (matchedProduct.InStockQuantity.Value - item.OutQuantity),
                        BarCodeNumber = lotNumberBatch,
                        ItemId = item.SubItemId,
                    };
                    matchedProduct.LotNumberBatch = lotNumberBatch;
                    matchedProduct.InStockQuantity = outStockRecord.AfterQuantity;

                    DateOnly nowDate = DateOnly.FromDateTime(DateTime.Now);
                    if (matchedProduct.OpenDeadline != null)
                    {
                        matchedProduct.LastAbleDate = nowDate.AddDays(matchedProduct.OpenDeadline.Value);
                    }
                    matchedProduct.LastOutStockDate = nowDate;
                    outStockRecords.Add(outStockRecord);
                    matchedSubItem.OwnerProcess = CommonConstants.PurchaseSubOwnerProcessStatus.DONE_OUTSTOCK;
                }
                _dbContext.OutStockRecords.AddRange(outStockRecords);
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);

            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[OwnerPurchaseSubItemsBatchOut]：{msg}", ex);
                return (false, ex.Message);
            }
        }

        public List<OutStockItemForOpenDeadline> SearchByOpenDeadline(string compId, int? daysAfter)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // 使用單一資料庫查詢: 先取得每個產品的最新出庫記錄 (透過 GroupBy + Max)
            var latestOutStockPerProduct = _dbContext.OutStockRecords
                .Where(o => o.CompId == compId)
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    LatestCreatedAt = g.Max(o => o.CreatedAt)
                });

            // 使用 JOIN 將產品、最新出庫記錄合併查詢
            var query = from p in _dbContext.WarehouseProducts
                        where p.CompId == compId && p.OpenDeadline != null && p.OpenDeadline > 0
                        join latest in latestOutStockPerProduct on p.ProductId equals latest.ProductId
                        join o in _dbContext.OutStockRecords on new { p.ProductId, CreatedAt = latest.LatestCreatedAt } equals new { o.ProductId, CreatedAt = o.CreatedAt }
                        where o.CompId == compId
                        select new
                        {
                            Product = p,
                            OutStockRecord = o
                        };

            var queryResult = query.ToList();

            var result = new List<OutStockItemForOpenDeadline>();

            foreach (var item in queryResult)
            {
                var product = item.Product;
                var latestOutStockRecord = item.OutStockRecord;

                var openDeadlineDate = latestOutStockRecord.CreatedAt?.AddDays(product.OpenDeadline ?? 0);
                var openDeadlineDateOnly = openDeadlineDate.HasValue ? DateOnly.FromDateTime(openDeadlineDate.Value) : (DateOnly?)null;
                var remainDaysFromNow = openDeadlineDateOnly.HasValue ? (openDeadlineDateOnly.Value.DayNumber - today.DayNumber) : (int?)null;

                if (daysAfter == null || remainDaysFromNow <= daysAfter)
                {
                    result.Add(new OutStockItemForOpenDeadline
                    {
                        OutStockQuantity = latestOutStockRecord.ApplyQuantity,
                        LotNumberBatch = latestOutStockRecord.LotNumberBatch,
                        LotNumber = latestOutStockRecord.LotNumber,
                        ProductName = latestOutStockRecord.ProductName,
                        ProductId = latestOutStockRecord.ProductId,
                        ProductCode = latestOutStockRecord.ProductCode,
                        Type = latestOutStockRecord.Type,
                        OutStockDate = latestOutStockRecord.CreatedAt,
                        OpenDeadline = product.OpenDeadline,
                        OpenDeadlineDate = openDeadlineDate,
                        LastAbleDate = product.LastAbleDate,
                        RemainingDays = remainDaysFromNow,
                        GroupIds = product.GroupIds,
                        GroupNames = product.GroupNames,
                        DefaultSupplierId = product.DefaultSupplierId,
                        DefaultSupplierName = product.DefaultSupplierName,
                        PackageWay = product.PackageWay,
                    });
                }
            }
            return result;
        }
    }

    public class CombinedUsageVo
    {
        public string ProductId { get; set; }
        public double? LastMonthQuantity { get; set; }
        public double? ThisYearAverageQuantity { get; set; }
        public double? LastYearQuantity { get; set; }
    }
}
