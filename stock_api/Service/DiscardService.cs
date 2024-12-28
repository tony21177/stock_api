using AutoMapper;
using Microsoft.VisualBasic;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Linq;
using System.Security.AccessControl;
using System.Transactions;

namespace stock_api.Service
{
    public class DiscardService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<StockOutService> _logger;
        private readonly WarehouseProductService _warehouseProductService;

        public DiscardService(StockDbContext dbContext, IMapper mapper, ILogger<StockOutService> logger, WarehouseProductService warehouseProductService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _warehouseProductService = warehouseProductService;
        }

        public (bool, string?) Discard(OutStockRecord outStockRecord, float applyQuantity, WarehouseMember user)
        {
            using var scope = new TransactionScope();
            try
            {
                outStockRecord.IsDiscard = true;
                outStockRecord.DiscardQuantity = applyQuantity + (outStockRecord.DiscardQuantity??0.0f) ;

                DiscardRecord newDiscardRecord = new()
                {
                    DiscardId = Guid.NewGuid().ToString(),
                    CompId = outStockRecord.CompId,
                    OutStockId = outStockRecord.OutStockId,
                    OutStockQuantity = outStockRecord.ApplyQuantity,
                    ApplyQuantity = applyQuantity,
                    OutStockTime = outStockRecord.CreatedAt.Value,
                    ProductId = outStockRecord.ProductId,
                    ProductCode = outStockRecord.ProductCode,
                    ProductName = outStockRecord.ProductName,
                    ProductSpec = outStockRecord.ProductSpec,
                    LotNumber = outStockRecord.LotNumber,
                    LotNumberBatch = outStockRecord.LotNumberBatch,
                    DiscardUserId = user.UserId,
                    DiscardUserName = user.DisplayName,
                    OutStockUserId = user.UserId,
                    OutStockUserName = user.DisplayName
                };
                _dbContext.DiscardRecords.Add(newDiscardRecord);
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[Discard]：{msg}", ex);
                return (false, ex.Message);
            }

        }


        public (List<DiscardRecord>,int) ListDiscardRecords(ListDiscardRecordsRequest request)
        {
            IQueryable<DiscardRecord> query = _dbContext.DiscardRecords;
            if (request.CompId != null)
            {
                query = query.Where(h => h.CompId == request.CompId);
            }
            if (request.StartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(request.StartDate);
                query = query.Where(h => h.CreatedAt >= startDateTime);
            }
            if (request.EndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.EndDate).Value.AddDays(1);
                query = query.Where(h => h.UpdatedAt < endDateTime);
            }
            if (request.ProductId != null)
            {
                query = query.Where(h => h.ProductId == request.ProductId);
            }
            if (request.LotNumber != null)
            {
                query = query.Where(h => h.LotNumber==request.LotNumber);
            }
            if (request.LotNumberBatch != null)
            {
                query = query.Where(h => h.LotNumberBatch == request.LotNumberBatch);
            }
            if (request.DiscardUserId != null)
            {
                query = query.Where(h => h.DiscardUserId == request.DiscardUserId);
            }
            if (request.OutStockUserId != null)
            {
                query = query.Where(h => h.OutStockUserId == request.OutStockUserId);
            }

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                query = query.Where(h => h.ProductCode.Contains(request.Keywords)
                || (h.ProductName.Contains(request.Keywords))
                || (h.ProductSpec != null && h.ProductSpec.Contains(request.Keywords))
                || (h.LotNumber != null && h.LotNumber.Contains(request.Keywords))
                || (h.LotNumberBatch != null && h.LotNumberBatch.Contains(request.Keywords))
                || (h.DiscardUserName != null && h.DiscardUserName.Contains(request.Keywords)));
            }

            int totalPages = 0;
            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "CreatedAt";
            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "ProductCode" => query.OrderByDescending(h => h.CreatedAt),
                    _ => query.OrderByDescending(h => h.CreatedAt),
                };
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "CreatedAt" => query.OrderBy(h => h.CreatedAt),
                    "ProductCode" => query.OrderBy(h => h.CreatedAt),
                    _ => query.OrderBy(h => h.CreatedAt),
                };
            }
            int totalItems = query.Count();
            totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);
            query = query.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);

        }
    }
}
