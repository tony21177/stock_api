using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using stock_api.Controllers.Request;
using stock_api.Models;
using System.Linq;
using System.Transactions;

namespace stock_api.Service
{
    public class WarehouseProductService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<WarehouseProductService> _logger;

        public WarehouseProductService(StockDbContext dbContext, IMapper mapper, ILogger<WarehouseProductService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public WarehouseProduct? GetProductByProductId(string productId)
        {
            return _dbContext.WarehouseProducts.Where(p => p.ProductId == productId).FirstOrDefault();
        }
        public List<WarehouseProduct> GetProductByProductCodeList(List<string> productCodeList)
        {
            return _dbContext.WarehouseProducts.Where(p => productCodeList.Contains(p.ProductCode)).ToList();
        }

        public WarehouseProduct? GetProductByProductIdAndCompId(string productId,string compId)
        {
            return _dbContext.WarehouseProducts.Where(p => p.ProductId == productId && p.CompId == compId).FirstOrDefault();
        }

        public WarehouseProduct? GetProductByProductCodeAndCompId(string productCode, string compId)
        {
            return _dbContext.WarehouseProducts.Where(p => p.ProductCode == productCode && p.CompId == compId).FirstOrDefault();
        }

        public List<WarehouseProduct> GetProductsByProductIdsAndCompId(List<string> productIdList, string compId)
        {
            return _dbContext.WarehouseProducts.Where(p => productIdList.Contains(p.ProductId) && p.CompId == compId).ToList();
        }

        public List<WarehouseProduct> GetProductsByProductIds(List<string> productIdList)
        {
            return _dbContext.WarehouseProducts.Where(p => productIdList.Contains(p.ProductId)).ToList();
        }

        public List<WarehouseProductCommon> GetCommonProductsByProductCodes(List<string> productCodeList)
        {
            return _dbContext.WarehouseProductCommons.Where(p => productCodeList.Contains(p.ProductCode)).ToList();
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
                    "ProductCode" => query.OrderByDescending(h => h.ProductCode),
                    _ => query.OrderByDescending(h => h.UpdatedAt),
                };
            }
            else
            {
                query = searchRequest.PaginationCondition.OrderByField switch
                {
                    "InStockQuantity" => query.OrderBy(h => h.InStockQuantity),
                    "MaxSafeQuantity" => query.OrderBy(h => h.MaxSafeQuantity),
                    "LastAbleDate" => query.OrderBy(h => h.LastAbleDate),
                    "LastOutStockDate" => query.OrderBy(h => h.LastOutStockDate),
                    "OpenDeadline" => query.OrderBy(h => h.OpenDeadline),
                    "OriginalDeadline" => query.OrderBy(h => h.OriginalDeadline),
                    "PreDeadline" => query.OrderBy(h => h.PreDeadline),
                    "PreOrderDays" => query.OrderBy(h => h.PreOrderDays),
                    "SafeQuantity" => query.OrderBy(h => h.SafeQuantity),
                    "AllowReceiveDateRange" => query.OrderBy(h => h.AllowReceiveDateRange),
                    "CreatedAt" => query.OrderBy(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderBy(h => h.UpdatedAt),
                    "ProductCode" => query.OrderBy(h => h.ProductCode),
                    _ => query.OrderBy(h => h.UpdatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / searchRequest.PaginationCondition.PageSize);

            query = query.Skip((searchRequest.PaginationCondition.Page - 1) * searchRequest.PaginationCondition.PageSize).Take(searchRequest.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);

        }


        public List<WarehouseProduct> ListNotEnoughProducts(ListNotEnoughProductsRequest request)
        {
            IQueryable<WarehouseProduct> query = _dbContext.WarehouseProducts;
            if (request.GroupId != null)
            {
                query = query.Where(h => h.GroupIds.Contains(request.GroupId));
            }

            if (request.ProductMachine != null)
            {
                query = query.Where(h => h.ProductMachine==request.ProductMachine);
            }

            if (request.SupplierId != null)
            {
                query = query.Where(h => h.DefaultSupplierId == request.SupplierId);
            }
            query = query.Where(h => h.CompId == request.CompId);
            query = query.Where(h => h.SafeQuantity.HasValue && h.InStockQuantity < h.SafeQuantity);
            return query.ToList();

        }

        public bool UpdateProduct(UpdateProductRequest request,WarehouseProduct existingProduct,List<WarehouseGroup> groups)
        {
            using var scope = new TransactionScope();
            try
            {
                var groupIds = request.GroupIds;

                // 尚未驗收的AcceptanceItem也需更新udiSerialcode
                if (request.UdiserialCode != null && request.UdiserialCode != existingProduct.UdiserialCode)
                {
                    _dbContext.AcceptanceItems.Where(item => item.QcStatus == null&&item.CompId==request.CompId&&item.ProductId== existingProduct.ProductId)
                        .ExecuteUpdate(item => item.SetProperty(x => x.UdiserialCode, request.UdiserialCode));
                }

                var updateProduct = _mapper.Map<WarehouseProduct>(request);
                updateProduct.InStockQuantity = existingProduct.InStockQuantity;
                updateProduct.MaxSafeQuantity = existingProduct.MaxSafeQuantity;
                updateProduct.OpenDeadline = existingProduct.OpenDeadline;
                updateProduct.PreDeadline = existingProduct.PreDeadline;
                if (request.PreOrderDays == null)
                {
                    updateProduct.PreOrderDays = existingProduct.PreOrderDays;
                }
                updateProduct.SafeQuantity = existingProduct.SafeQuantity;
                updateProduct.DefaultSupplierId = existingProduct.DefaultSupplierId;
                if (request.IsNeedAcceptProcess == null)
                {
                    updateProduct.IsNeedAcceptProcess = existingProduct.IsNeedAcceptProcess;
                }
                updateProduct.AllowReceiveDateRange = existingProduct.AllowReceiveDateRange;
                updateProduct.TestCount = existingProduct.TestCount;
                updateProduct.UnitConversion = existingProduct.UnitConversion;
                updateProduct.IsActive = existingProduct.IsActive;
                _mapper.Map(updateProduct, existingProduct);
                if (groups.Count > 0)
                {
                    var matchedGroups = groups.Where(g => groupIds.Contains(g.GroupId)).ToList();
                    existingProduct.GroupNames = matchedGroups.Select(g => g.GroupName).Aggregate("", (current, s) => current + (s + ","));
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateProduct]：{msg}", ex);
                return false;
            }

            
        }

        public bool AdminUpdateProduct(AdminUpdateProductRequest request, WarehouseProduct existingProduct, Supplier? supplier,Manufacturer? manufacturer, List<WarehouseGroup> groups)
        {
            using var scope = new TransactionScope();
            try
            {
                // 尚未驗收的AcceptanceItem也需更新udiSerialcode
                if (request.UdiserialCode != null && request.UdiserialCode != existingProduct.UdiserialCode)
                {
                    _dbContext.AcceptanceItems.Where(item => item.QcStatus == null && item.CompId == request.CompId && item.ProductId == existingProduct.ProductId)
                        .ExecuteUpdate(item => item.SetProperty(x => x.UdiserialCode, request.UdiserialCode));
                }

                var groupIds = request.GroupIds;
                var matchedGroups = groups.Where(g => groupIds.Contains(g.GroupId)).ToList();
                var updateProduct = new WarehouseProduct()
                {
                    CompId = existingProduct.CompId,
                    ProductId = existingProduct.ProductId,
                };

                _mapper.Map(request, updateProduct);
                updateProduct.InStockQuantity = existingProduct.InStockQuantity;
                if (request.MaxSafeQuantity == null)
                {
                    updateProduct.MaxSafeQuantity = existingProduct.MaxSafeQuantity;
                }
                if (request.OpenDeadline == null)
                {
                    updateProduct.OpenDeadline = existingProduct.OpenDeadline;
                }
                if (request.PreDeadline == null)
                {
                    updateProduct.PreDeadline = existingProduct.PreDeadline;
                }
                if (request.PreOrderDays == null)
                {
                    updateProduct.PreOrderDays = existingProduct.PreOrderDays;
                }
                if (request.SafeQuantity == null)
                {
                    updateProduct.SafeQuantity = existingProduct.SafeQuantity;
                }
                if (request.DefaultSupplierId == null)
                {
                    updateProduct.DefaultSupplierId = existingProduct.DefaultSupplierId;
                }
                if (request.IsNeedAcceptProcess == null)
                {
                    updateProduct.IsNeedAcceptProcess = existingProduct.IsNeedAcceptProcess;
                }
                if (request.AllowReceiveDateRange == null)
                {
                    updateProduct.AllowReceiveDateRange = existingProduct.AllowReceiveDateRange;
                }
                if (request.UnitConversion == null)
                {
                    updateProduct.UnitConversion = existingProduct.UnitConversion;
                }
                if (request.TestCount == null)
                {
                    updateProduct.TestCount = existingProduct.TestCount;
                }
                updateProduct.IsActive = existingProduct.IsActive;

                _mapper.Map(updateProduct, existingProduct);
                existingProduct.GroupNames = matchedGroups.Select(g => g.GroupName).Aggregate("", (current, s) => current + (s + ","));
                if (supplier != null)
                {
                    existingProduct.DefaultSupplierName = supplier.Name;
                }
                if (manufacturer != null)
                {
                    existingProduct.ManufacturerName = manufacturer.Name;
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[AdminUpdateProduct]：{msg}", ex);
                return false;
            }

            
        }

    }
}
