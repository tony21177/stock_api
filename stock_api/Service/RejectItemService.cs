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
    public class RejectItemService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<StockOutService> _logger;
        private readonly WarehouseProductService _warehouseProductService;

        public RejectItemService(StockDbContext dbContext, IMapper mapper, ILogger<StockOutService> logger, WarehouseProductService warehouseProductService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _warehouseProductService = warehouseProductService;
        }

        public (bool, string?) RejectItem(InStockItemRecord inStockItemRecord,PurchaseSubItem? purchaseSubItem,WarehouseProduct product, WarehouseMember user,RejectItemRequest request)
        {
            using var scope = new TransactionScope();
            try
            {
                var rejectId = Guid.NewGuid().ToString();
                DateOnly rejectDate = DateOnly.FromDateTime(DateTime.Now);
                if (request.RejectDate != null)
                {
                    rejectDate = DateOnly.FromDateTime( DateTimeHelper.ParseDateString(request.RejectDate).Value);
                }
                var rejectQuantity = inStockItemRecord.InStockQuantity;
                if (request.RejectQuantity != null)
                {
                    rejectQuantity = request.RejectQuantity.Value;
                }


                RejectItemRecord newRejectItemRecord = new()
                {
                    RejectId = rejectId,
                    PurchaseMainId = purchaseSubItem?.PurchaseMainId,
                    SubItemId = purchaseSubItem?.ItemId,
                    InStockId = inStockItemRecord.InStockId,
                    LotNumberBatch = inStockItemRecord.LotNumberBatch,
                    LotNumber = inStockItemRecord.LotNumber ?? string.Empty, // Fix for CS8601
                    CompId = inStockItemRecord.CompId,
                    StockQuantityBefore = product.InStockQuantity ?? 0.0f, // Fix for CS1003 and CS1525
                    StockQuantityAfter = (product.InStockQuantity ?? 0.0f) - inStockItemRecord.InStockQuantity, // Fix for CS1003 and CS1525
                    InStockQuantity = inStockItemRecord.InStockQuantity,
                    RejectQuantity = rejectQuantity,
                    ProductId = inStockItemRecord.ProductId,
                    ProductCode = inStockItemRecord.ProductCode,
                    ProductName = inStockItemRecord.ProductName,
                    ProductSpec = inStockItemRecord.ProductSpec,
                    RejectUserId = user.UserId,
                    RejectUserName = user.DisplayName,
                    InStockUserId = inStockItemRecord.UserId,
                    InStockUserName = inStockItemRecord.UserName,
                    SupplierId = inStockItemRecord.SupplierId,
                    SupplierName = inStockItemRecord.SupplierName,
                    RejectDate = rejectDate, 
                    RejectReason = request.RejectReason,
                };
                inStockItemRecord.RejectQuantity = rejectQuantity;
                if(inStockItemRecord.InStockQuantity+inStockItemRecord.AdjustInQuantity-inStockItemRecord.OutStockQuantity-inStockItemRecord.AdjustInQuantity - rejectQuantity == 0)
                {
                    inStockItemRecord.OutStockStatus = CommonConstants.OutStockStatus.ALL;
                }
                product.InStockQuantity -= rejectQuantity;

                _dbContext.RejectItemRecords.Add(newRejectItemRecord);

                SupplierTraceLog newSupplierTraceLog = new SupplierTraceLog()
                {
                    CompId = inStockItemRecord.CompId,
                    AbnormalType = CommonConstants.AbnormalType.REJECT_ABNORMAL,
                    SupplierId = inStockItemRecord.SupplierId,
                    SupplierName = inStockItemRecord.SupplierName,
                    AbnormalContent = request.RejectReason,
                    UserId = user.UserId,
                    UserName = user.DisplayName,
                    SourceId = inStockItemRecord.InStockId,
                    SourceType = CommonConstants.SourceType.IN_STOCK,
                    AbnormalDate = rejectDate.ToDateTime(TimeOnly.MinValue),
                    ProductId = inStockItemRecord.ProductId,
                    ProductName = inStockItemRecord.ProductName,
                    PurchaseMainId = purchaseSubItem?.PurchaseMainId,
                    LotNumber = inStockItemRecord.LotNumber,
                    LotNumberBatch = inStockItemRecord.LotNumberBatch,
                };
                _dbContext.SupplierTraceLogs.Add(newSupplierTraceLog);

                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[RejectItem]：{msg}", ex);
                return (false, ex.Message);
            }

        }


        public (List<RejectItemRecord>,int) ListRejectItemRecords(ListRejectRecordsRequest request)
        {
            IQueryable<RejectItemRecord> query = _dbContext.RejectItemRecords;
            if (request.CompId != null)
            {
                query = query.Where(h => h.CompId == request.CompId);
            }
            if (request.StartDate != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(request.StartDate);
                var startDate = DateOnly.FromDateTime(startDateTime.Value);
                query = query.Where(h => h.RejectDate >= startDate);
            }
            if (request.EndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.EndDate).Value.AddDays(1);
                var endDate = DateOnly.FromDateTime(endDateTime);
                query = query.Where(h => h.RejectDate < endDate);
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
            if (request.RejectUserId != null)
            {
                query = query.Where(h => h.RejectUserId == request.RejectUserId);
            }
            if (request.InStockUserId != null)
            {
                query = query.Where(h => h.InStockUserId == request.InStockUserId);
            }

            if (!string.IsNullOrEmpty(request.Keywords))
            {
                query = query.Where(h => h.ProductCode.Contains(request.Keywords)
                || (h.ProductName.Contains(request.Keywords))
                || (h.ProductSpec != null && h.ProductSpec.Contains(request.Keywords))
                || (h.LotNumber != null && h.LotNumber.Contains(request.Keywords))
                || (h.LotNumberBatch != null && h.LotNumberBatch.Contains(request.Keywords))
                || (h.RejectUserName != null && h.RejectUserName.Contains(request.Keywords)));
            }

            int totalPages = 0;
            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "RejectDate";
            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "RejectDate" => query.OrderByDescending(h => h.RejectDate),
                    "ProductCode" => query.OrderByDescending(h => h.CreatedAt),
                    _ => query.OrderByDescending(h => h.CreatedAt),
                };
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "RejectDate" => query.OrderBy(h => h.RejectDate),
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
