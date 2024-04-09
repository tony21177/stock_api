using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using stock_api.Controllers.Request;
using stock_api.Models;

namespace stock_api.Service
{
    public class WarehouseProductService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public WarehouseProductService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public (List<WarehouseProduct> Results, int TotalPages) SearchProduct(WarehouseProductSearchRequest searchRequest)
        {
            IQueryable<WarehouseProduct> query = _dbContext.WarehouseProducts;

            if (searchRequest.ProductCategory != null)
            {
                query = query.Where(h => h.ProductCategory==searchRequest.ProductCategory);
            }
            if (searchRequest.ProductMachine != null)
            {
                query = query.Where(h => h.ProductMachine == searchRequest.ProductMachine);
            }
            if (searchRequest.OpenDeadline != null)
            {
                query = query.Where(h => h.OpenDeadline == searchRequest.OpenDeadline);
            }
            if (searchRequest.GroupId != null)
            {
                query = query.Where(h => h.GroupIds.Contains(searchRequest.GroupId));
            }
            query = query.Where(h => h.CompId == searchRequest.CompId);

            if (!string.IsNullOrEmpty(searchRequest.Keywords))
            {
                var groupNameList = 
                query = query.Where(h => h.LotNumberBatch.Contains(searchRequest.Keywords)
                || h.LotNumber.Contains(searchRequest.Keywords)
                || h.ManufacturerName.Contains(searchRequest.Keywords)
                || h.DeadlineRule.Contains(searchRequest.Keywords)
                || h.DeliverFunction.Contains(searchRequest.Keywords)
                || h.GroupNames.Contains(searchRequest.Keywords)
                || h.Manager.Contains(searchRequest.Keywords)
                || h.OpenedSealName.Contains(searchRequest.Keywords)
                || h.PackageWay.Contains(searchRequest.Keywords)
                || h.ProductCode.Contains(searchRequest.Keywords)
                || h.ProductId.Contains(searchRequest.Keywords)
                || h.ProductModel.Contains(searchRequest.Keywords)
                || h.ProductName.Contains(searchRequest.Keywords)
                || h.ProductRemarks.Contains(searchRequest.Keywords)
                || h.ProductSpec.Contains(searchRequest.Keywords)
                || h.SavingFunction.Contains(searchRequest.Keywords)
                || h.UdibatchCode.Contains(searchRequest.Keywords)
                || h.UdicreateCode.Contains(searchRequest.Keywords)
                || h.UdiserialCode.Contains(searchRequest.Keywords)
                || h.UdiverifyDateCode.Contains(searchRequest.Keywords)
                || h.DefaultSupplierId.Contains(searchRequest.Keywords)
                || h.DefaultSupplierName.Contains(searchRequest.Keywords));
            }


            if (searchRequest.PaginationCondition.IsDescOrderBy)
            {
                query = searchRequest.PaginationCondition.OrderByField switch
                {
                    "InStockQuantity" => query.OrderByDescending(h => h.InStockQuantity),
                    "MaxSafeQuantity" => query.OrderByDescending(h => h.MaxSafeQuantity),
                    "LastAbleDate" => query.OrderByDescending(h => h.LastAbleDate),
                    "LastOutStockDate" => query.OrderByDescending(h => h.LastOutStockDate),
                    "OpenDeadline" => query.OrderByDescending(h => h.OpenDeadline),
                    "OriginalDeadline" => query.OrderByDescending(h => h.OriginalDeadline),
                    "PreDeadline" => query.OrderByDescending(h => h.PreDeadline),
                    "PreOrderDays" => query.OrderByDescending(h => h.PreOrderDays),
                    "SafeQuantity" => query.OrderByDescending(h => h.SafeQuantity),
                    "AllowReceiveDateRange" => query.OrderByDescending(h => h.AllowReceiveDateRange),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.UpdatedAt),
                };
            }
            else
            {
                query = searchRequest.PaginationCondition.OrderByField switch
                {
                    "InStockQuantity" => query.OrderByDescending(h => h.InStockQuantity),
                    "MaxSafeQuantity" => query.OrderByDescending(h => h.MaxSafeQuantity),
                    "LastAbleDate" => query.OrderByDescending(h => h.LastAbleDate),
                    "LastOutStockDate" => query.OrderByDescending(h => h.LastOutStockDate),
                    "OpenDeadline" => query.OrderByDescending(h => h.OpenDeadline),
                    "OriginalDeadline" => query.OrderByDescending(h => h.OriginalDeadline),
                    "PreDeadline" => query.OrderByDescending(h => h.PreDeadline),
                    "PreOrderDays" => query.OrderByDescending(h => h.PreOrderDays),
                    "SafeQuantity" => query.OrderByDescending(h => h.SafeQuantity),
                    "AllowReceiveDateRange" => query.OrderByDescending(h => h.AllowReceiveDateRange),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.UpdatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / searchRequest.PaginationCondition.PageSize);

            query = query.Skip((searchRequest.PaginationCondition.Page - 1) * searchRequest.PaginationCondition.PageSize).Take(searchRequest.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);

        }
    }
}
