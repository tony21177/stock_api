using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.ComponentModel;
using System.Linq;
using System.Transactions;

namespace stock_api.Service
{
    public class WarehouseProductService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<WarehouseProductService> _logger;
        private FileUploadService _fileUploadService;
        private PurchaseService _purchaseService;

        public WarehouseProductService(StockDbContext dbContext, IMapper mapper, ILogger<WarehouseProductService> logger, FileUploadService fileUploadService, PurchaseService purchaseService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _fileUploadService = fileUploadService;
            _purchaseService = purchaseService;
        }

        public List<WarehouseProduct> GetAllActiveProducts()
        {
            return _dbContext.WarehouseProducts.Where(p => p.IsActive==true).ToList();
        }

        public List<WarehouseProduct> GetAllProducts(String compId)
        {
            return _dbContext.WarehouseProducts.Where(p => p.IsActive == true&&p.CompId==compId).ToList();
        }

        public WarehouseProduct? GetProductByProductId(string productId)
        {
            return _dbContext.WarehouseProducts.Where(p => p.ProductId == productId).FirstOrDefault();
        }
        public List<WarehouseProduct> GetProductByProductCodeList(List<string> productCodeList)
        {
            return _dbContext.WarehouseProducts.Where(p => productCodeList.Contains(p.ProductCode)).ToList();
        }

        public WarehouseProduct? GetProductByProductIdAndCompId(string productId, string compId)
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

        public List<WarehouseProduct> GetProductsByCompId( string compId)
        {
            return _dbContext.WarehouseProducts.Where(p =>  p.CompId == compId).ToList();
        }

        public List<WarehouseProduct> GetProductsByProductCodesAndCompId(List<string> productCodeList, string compId)
        {
            return _dbContext.WarehouseProducts.Where(p => productCodeList.Contains(p.ProductCode) && p.CompId == compId).ToList();
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
                query = query.Where(h => h.ProductCategory == searchRequest.ProductCategory);
            }
            if (searchRequest.ProductMachine != null)
            {
                query = query.Where(h => h.ProductMachine == searchRequest.ProductMachine);
            }
            if (searchRequest.ProductId != null)
            {
                query = query.Where(h => h.ProductId == searchRequest.ProductId);
            }
            if (searchRequest.OpenDeadline != null)
            {
                query = query.Where(h => h.OpenDeadline == searchRequest.OpenDeadline);
            }
            if (searchRequest.GroupId != null)
            {
                query = query.Where(h => h.GroupIds != null && h.GroupIds.Contains(searchRequest.GroupId));
            }
            if (searchRequest.DefaultSupplierId != null)
            {
                query = query.Where(h => h.DefaultSupplierId != null && h.DefaultSupplierId == searchRequest.DefaultSupplierId);
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

            int totalPages = 0;
            if (searchRequest.PaginationCondition.OrderByField != "insufficientQuantity")
            {
                if (searchRequest.PaginationCondition.OrderByField == null) searchRequest.PaginationCondition.OrderByField = "UpdatedAt";
                if (searchRequest.PaginationCondition.IsDescOrderBy)
                {
                    var orderByField = StringUtils.CapitalizeFirstLetter(searchRequest.PaginationCondition.OrderByField);
                    query = orderByField switch
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
                        "GroupId" => query.OrderByDescending(h => h.GroupIds),
                        "GroupName" => query.OrderByDescending(h => h.GroupNames),
                        _ => query.OrderByDescending(h => h.UpdatedAt),
                    };
                }
                else
                {
                    var orderByField = StringUtils.CapitalizeFirstLetter(searchRequest.PaginationCondition.OrderByField);
                    query = orderByField switch
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
                        "GroupId" => query.OrderBy(h => h.GroupIds),
                        "GroupName" => query.OrderBy(h => h.GroupNames),
                        _ => query.OrderBy(h => h.UpdatedAt),
                    };
                }
                int totalItems = query.Count();
                totalPages = (int)Math.Ceiling((double)totalItems / searchRequest.PaginationCondition.PageSize);
                query = query.Skip((searchRequest.PaginationCondition.Page - 1) * searchRequest.PaginationCondition.PageSize).Take(searchRequest.PaginationCondition.PageSize);
            }
            
            return (query.ToList(), totalPages);
        }


        public List<NotEnoughQuantityProduct> ListNotEnoughProducts(ListNotEnoughProductsRequest request)
        {
            var allOngoingPurchaseItems = _dbContext.PurchaseSubItems.Where(s => s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.CLOSE &&
            s.ReceiveStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE && s.CompId == request.CompId && s.OwnerProcess!=CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE).ToList();
            var allPurchaseMainIdList = allOngoingPurchaseItems.Select(p => p.PurchaseMainId).ToList();
            var allPurchaseMain = _purchaseService.GetPurchaseMainsByMainIdList(allPurchaseMainIdList);
            var allEffectivePurchaseMain = _purchaseService.GetPurchaseMainsByMainIdList(allPurchaseMainIdList)
                .Where(m => m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.REJECT && m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.CLOSE)
                .ToList();
            var allEffectivePurchaseMainId = allEffectivePurchaseMain.Select(m => m.PurchaseMainId).ToList();
            allOngoingPurchaseItems = allOngoingPurchaseItems.Where(s=>allEffectivePurchaseMainId.Contains(s.PurchaseMainId)).ToList(); 

            IQueryable<WarehouseProduct> query = _dbContext.WarehouseProducts;
            if (request.GroupId != null)
            {
                query = query.Where(h => h.GroupIds.Contains(request.GroupId));
            }

            if (request.ProductMachine != null)
            {
                query = query.Where(h => h.ProductMachine == request.ProductMachine);
            }

            if (request.SupplierId != null)
            {
                query = query.Where(h => h.DefaultSupplierId == request.SupplierId);
            }
            query = query.Where(h => h.CompId == request.CompId);
            query = query.Where(h => h.SafeQuantity.HasValue && h.InStockQuantity <= h.SafeQuantity);

            var products = query.ToList();
            var matchedProducts = _mapper.Map<List<NotEnoughQuantityProduct>>(products);

            var notEnoughProducts = matchedProducts.FindAll(p =>
            {
                var matchedSubItems = allOngoingPurchaseItems.Where(i => i.ProductId == p.ProductId).ToList();                
                var ongoingOrderQuantities = matchedSubItems.Select(i => i.Quantity).Sum();

                if (p.MaxSafeQuantity - p.InStockQuantity - ongoingOrderQuantities >= 0)
                {
                    p.InProcessingOrderQuantity = ongoingOrderQuantities??0.0f;
                    p.NeedOrderedQuantity = p.MaxSafeQuantity??0.0f - p.InStockQuantity ?? 0.0f - ongoingOrderQuantities ?? 0.0f;
                    var needOrderedQuantityUnitFloat = p.NeedOrderedQuantity * p.UnitConversion;
                    var needOrderedQuantityUnit = Math.Ceiling((decimal)needOrderedQuantityUnitFloat.Value * 100) / 100;
                    p.NeedUnorderedQuantityUnit = (float)needOrderedQuantityUnit;
                    return true;
                }
                return false;
            });
            

            return notEnoughProducts;

        }

        public bool UpdateProduct(UpdateProductRequest request, WarehouseProduct existingProduct, List<WarehouseGroup> groups)
        {
            using var scope = new TransactionScope();
            try
            {
                var groupIds = request.GroupIds;

                // 尚未驗收的AcceptanceItem也需更新udiSerialcode
                if (request.UdiserialCode != null && request.UdiserialCode != existingProduct.UdiserialCode)
                {
                    _dbContext.AcceptanceItems.Where(item => item.CompId == request.CompId && item.ProductId == existingProduct.ProductId)
                        .ExecuteUpdate(item => item.SetProperty(x => x.UdiserialCode, request.UdiserialCode));
                    _dbContext.PurchaseSubItems.Where(i => i.CompId == request.CompId && i.ProductCode == existingProduct.ProductCode).
                        ExecuteUpdate(i => i.SetProperty(x => x.UdiserialCode, request.UdiserialCode));
                }

                var updateProduct = _mapper.Map<WarehouseProduct>(request);
                updateProduct.InStockQuantity = existingProduct.InStockQuantity;
                updateProduct.MaxSafeQuantity = existingProduct.MaxSafeQuantity;
                updateProduct.OpenDeadline = existingProduct.OpenDeadline;
                if (request.PreDeadline == null)
                {
                    updateProduct.PreDeadline = existingProduct.PreDeadline;
                }
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
                else if(request.IsNeedAcceptProcess==false)
                {
                    _dbContext.InStockItemRecords.Where(i=>i.ProductId==existingProduct.ProductId)
                        .ExecuteUpdate(i => i.SetProperty(x => x.IsNeedQc, false));
                }
                updateProduct.AllowReceiveDateRange = existingProduct.AllowReceiveDateRange;
                updateProduct.TestCount = existingProduct.TestCount;
                updateProduct.UnitConversion = existingProduct.UnitConversion;
                updateProduct.IsActive = existingProduct.IsActive;
                if (request.OpenDeadline == null)
                {
                    updateProduct.OpenDeadline = existingProduct.OpenDeadline;
                }
                if (request.IsPrintSticker == null)
                {
                    updateProduct.IsPrintSticker = existingProduct.IsPrintSticker;
                }

                updateProduct.CompId = existingProduct.CompId;
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

        public bool AdminUpdateProduct(AdminUpdateProductRequest request, WarehouseProduct existingProduct, Supplier? supplier, Manufacturer? manufacturer, List<WarehouseGroup> groups)
        {
            using var scope = new TransactionScope();
            try
            {
                // 尚未驗收的AcceptanceItem也需更新udiSerialcode
                if (request.UdiserialCode != null && request.UdiserialCode != existingProduct.UdiserialCode)
                {
                    _dbContext.AcceptanceItems.Where(item => item.CompId == request.CompId && item.ProductId == existingProduct.ProductId)
                        .ExecuteUpdate(item => item.SetProperty(x => x.UdiserialCode, request.UdiserialCode));
                }

                // 尚未任何入庫的採購品項其ProductSpec也須更新
                if (request.ProductSpec != null && request.ProductSpec != existingProduct.ProductSpec)
                {
                    _dbContext.PurchaseSubItems.Where(item => item.CompId == request.CompId && item.ProductId == existingProduct.ProductId && item.ReceiveStatus
                    == CommonConstants.PurchaseReceiveStatus.NONE).ExecuteUpdate(item => item.SetProperty(x => x.ProductSpec, request.ProductSpec));
                }

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

                }else if (request.IsNeedAcceptProcess==false)
                {
                    _dbContext.InStockItemRecords.Where(i => i.ProductId == existingProduct.ProductId)
                        .ExecuteUpdate(i => i.SetProperty(x => x.IsNeedQc, false));
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
                if (request.IsActive == null)
                {
                    updateProduct.IsActive = existingProduct.IsActive;
                }
                if (request.IsPrintSticker == null)
                {
                    updateProduct.IsPrintSticker = existingProduct.IsPrintSticker;
                }
                updateProduct.CompId = existingProduct.CompId;
                _mapper.Map(updateProduct, existingProduct);
                var groupIds = request.GroupIds;
                if (groupIds != null)
                {
                    var matchedGroups = groups.Where(g => groupIds.Contains(g.GroupId)).ToList();
                    existingProduct.GroupIds = String.Join(",", matchedGroups.Select(g => g.GroupId).ToList());
                    existingProduct.GroupNames = matchedGroups.Select(g => g.GroupName).Aggregate("", (current, s) => current + (s + ","));
                }
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

        //public bool UpdateOrAddProductImage(string ImageBase64String,string ProductId,string CompId)
        //{
        //    using var scope = new TransactionScope();
        //    try
        //    {
        //        var existingProductImage = _dbContext.ProductImages.Where(i => i.ProductId == ProductId && i.CompId == CompId).FirstOrDefault();
        //        if (existingProductImage == null)
        //        {
        //            var newProductImage = new ProductImage()
        //            {
        //                ProductId = ProductId,
        //                CompId = CompId,
        //                Image = ImageBase64String
        //            };

        //            _dbContext.ProductImages.Add(newProductImage);

        //        }
        //        else
        //        {
        //            existingProductImage.Image = ImageBase64String;
        //        }

        //        _dbContext.SaveChanges();
        //        scope.Complete();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("事務失敗[UpdateOrAddProductImage]：{msg}", ex);
        //        return false;
        //    }
        //}

        public async Task<bool> UpdateOrAddProductImage(IFormFile imagesFile, string ProductId, string CompId)
        {
            using var scope = new TransactionScope();
            try
            {
                var fileDetails = await _fileUploadService.PostFilesAsync(new List<IFormFile> { imagesFile }, new List<string> { "product" });

                var existingProductImage = _dbContext.ProductImages.Where(i => i.ProductId == ProductId && i.CompId == CompId).FirstOrDefault();
                if (existingProductImage == null)
                {
                    var newProductImage = new ProductImage()
                    {
                        ProductId = ProductId,
                        CompId = CompId,
                        Image = fileDetails[0].FilePath
                    };

                    _dbContext.ProductImages.Add(newProductImage);

                }
                else
                {
                    existingProductImage.Image = fileDetails[0].FilePath;
                }

                _dbContext.SaveChanges();
                scope.Complete();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateOrAddProductImage]：{msg}", ex);
                return false;
            }
        }

        public ProductImage? GetProductImage(string ProductId, string CompId)
        {

            return _dbContext.ProductImages.FirstOrDefault(i => i.ProductId == ProductId && i.CompId == CompId);
        }


        public (bool, string?) UpdateProducts(List<ModifyProductDto> modifyProductDtos,List<WarehouseProduct> products,List<WarehouseGroup> allGroups)
        {
            using var scope = new TransactionScope();
            try
            {
                foreach (var modifyProductDto in modifyProductDtos)
                {
                    var updateProducts = products.Where(p => p.ProductCode == modifyProductDto.ProductCode).ToList();

                    if (updateProducts.Count==0)
                    {
                        _logger.LogWarning("Product with code:{productCode} not found", modifyProductDto.ProductCode);
                        continue;
                    }

                    if (modifyProductDto.DeadlineRule.HasValue)

                        updateProducts.ForEach(p=>p.DeadlineRule = modifyProductDto.DeadlineRule.Value) ;

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.DeliverRemarks))
                        updateProducts.ForEach(p => p.DeliverRemarks = modifyProductDto.DeliverRemarks);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.GroupNames))
                    {

                        foreach(var updateProduct in updateProducts)
                        {
                            var updateCompId = updateProduct.CompId;
                            var updateGroupNameList = modifyProductDto.GroupNames.Split(',').ToList();
                            var matchedUpdateGroup = allGroups.Where(g=>g.CompId==updateCompId&&g.GroupName!=null).Where(g=>updateGroupNameList.Contains(g.GroupName)).ToList();
                            updateProduct.GroupIds = string.Join(",", matchedUpdateGroup.Select(g => g.GroupId));
                            updateProduct.GroupNames = string.Join(",", matchedUpdateGroup.Select(g => g.GroupName));
                        }
                    }
                       

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.Manager))
                        updateProducts.ForEach(p => p.Manager = modifyProductDto.Manager);

                    if (modifyProductDto.MaxSafeQuantity.HasValue)
                        updateProducts.ForEach(p => p.MaxSafeQuantity = (float)modifyProductDto.MaxSafeQuantity.Value);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.OpenedSealName))
                        updateProducts.ForEach(p => p.OpenedSealName = modifyProductDto.OpenedSealName);

                    if (modifyProductDto.PreOrderDays.HasValue)
                        updateProducts.ForEach(p => p.PreOrderDays = modifyProductDto.PreOrderDays.Value);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductCategory))
                        updateProducts.ForEach(p => p.ProductCategory = modifyProductDto.ProductCategory);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductRemarks))
                        updateProducts.ForEach(p => p.ProductRemarks = modifyProductDto.ProductRemarks);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.Unit))
                        updateProducts.ForEach(p => p.Unit = modifyProductDto.Unit);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductMachine))
                        updateProducts.ForEach(p => p.ProductMachine = modifyProductDto.ProductMachine);

                    if (modifyProductDto.IsNeedAcceptProcess.HasValue)
                        updateProducts.ForEach(p => p.IsNeedAcceptProcess = modifyProductDto.IsNeedAcceptProcess);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.StockLocation))
                        updateProducts.ForEach(p => p.StockLocation = modifyProductDto.StockLocation);
                    if (modifyProductDto.IsPrintSticker.HasValue)
                        updateProducts.ForEach(p => p.IsPrintSticker = modifyProductDto.IsPrintSticker);

                }
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateProducts]：{msg}", ex);
                return (false, ex.Message);
            }
            //try
            //{
            //    var updateStatements = new List<string>();

            //    foreach (var modifyProductDto in modifyProductDtos)
            //    {
            //        var product = products.FirstOrDefault(p => p.ProductCode == modifyProductDto.ProductCode);

            //        if (product == null)
            //        {
            //            _logger.LogWarning("Product with code: {ProductCode} not found", modifyProductDto.ProductCode);
            //            continue;
            //        }

            //        var updates = new List<string>();

            //        if (modifyProductDto.DeadlineRule.HasValue)
            //            updates.Add($"DeadlineRule = {modifyProductDto.DeadlineRule.Value}");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.DeliverRemarks))
            //            updates.Add($"DeliverRemarks = '{modifyProductDto.DeliverRemarks.Replace("'", "''")}'");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.GroupNames))
            //            updates.Add($"GroupNames = '{modifyProductDto.GroupNames.Replace("'", "''")}'");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.Manager))
            //            updates.Add($"Manager = '{modifyProductDto.Manager.Replace("'", "''")}'");

            //        if (modifyProductDto.MaxSafeQuantity.HasValue)
            //            updates.Add($"MaxSafeQuantity = {modifyProductDto.MaxSafeQuantity.Value}");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.OpenedSealName))
            //            updates.Add($"OpenedSealName = '{modifyProductDto.OpenedSealName.Replace("'", "''")}'");

            //        if (modifyProductDto.PreOrderDays.HasValue)
            //            updates.Add($"PreOrderDays = {modifyProductDto.PreOrderDays.Value}");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductCategory))
            //            updates.Add($"ProductCategory = '{modifyProductDto.ProductCategory.Replace("'", "''")}'");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductRemarks))
            //            updates.Add($"ProductRemarks = '{modifyProductDto.ProductRemarks.Replace("'", "''")}'");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.Unit))
            //            updates.Add($"Unit = '{modifyProductDto.Unit.Replace("'", "''")}'");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductMachine))
            //            updates.Add($"ProductMachine = '{modifyProductDto.ProductMachine.Replace("'", "''")}'");

            //        if (modifyProductDto.IsNeedAcceptProcess.HasValue)
            //            updates.Add($"IsNeedAcceptProcess = {modifyProductDto.IsNeedAcceptProcess.Value}");

            //        if (!string.IsNullOrWhiteSpace(modifyProductDto.StockLocation))
            //            updates.Add($"StockLocation = '{modifyProductDto.StockLocation.Replace("'", "''")}'");

            //        if (updates.Any())
            //        {
            //            var updateStatement = $"UPDATE warehouse_product SET {string.Join(", ", updates)} WHERE ProductCode = '{modifyProductDto.ProductCode}'";
            //            updateStatements.Add(updateStatement);
            //        }
            //    }

            //    if (updateStatements.Any())
            //    {
            //        var batchUpdateSql = string.Join("; ", updateStatements);
            //        _dbContext.Database.ExecuteSqlRaw(batchUpdateSql);
            //    }

            //    scope.Complete();
            //    return (true, null);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError("Transaction failed [UpdateProducts]: {Message}", ex);
            //    return (false, ex.Message);
            //}
        }

        public (bool, string?) UpdateProducts(List<ModifyProductDto> modifyProductDtos, List<WarehouseProduct> products, List<WarehouseGroup> allGroups, string compId)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                foreach (var modifyProductDto in modifyProductDtos)
                {
                    var updateProducts = products.Where(p => p.ProductCode == modifyProductDto.ProductCode&&p.CompId==compId).ToList();

                    if (updateProducts.Count == 0)
                    {
                        _logger.LogWarning("Product with code:{productCode} not found", modifyProductDto.ProductCode);
                        continue;
                    }

                    if (modifyProductDto.DeadlineRule.HasValue)

                        updateProducts.ForEach(p => p.DeadlineRule = modifyProductDto.DeadlineRule.Value);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.DeliverRemarks))
                        updateProducts.ForEach(p => p.DeliverRemarks = modifyProductDto.DeliverRemarks);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.GroupNames))
                    {

                        foreach (var updateProduct in updateProducts)
                        {
                            var updateCompId = updateProduct.CompId;
                            var updateGroupNameList = modifyProductDto.GroupNames.Split(',').ToList();
                            var matchedUpdateGroup = allGroups.Where(g => g.CompId == updateCompId && g.GroupName != null).Where(g => updateGroupNameList.Contains(g.GroupName)).ToList();
                            updateProduct.GroupIds = string.Join(",", matchedUpdateGroup.Select(g => g.GroupId));
                            updateProduct.GroupNames = string.Join(",", matchedUpdateGroup.Select(g => g.GroupName));
                        }
                    }


                    if (!string.IsNullOrWhiteSpace(modifyProductDto.Manager))
                        updateProducts.ForEach(p => p.Manager = modifyProductDto.Manager);

                    if (modifyProductDto.MaxSafeQuantity.HasValue)
                        updateProducts.ForEach(p => p.MaxSafeQuantity = (float)modifyProductDto.MaxSafeQuantity.Value);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.OpenedSealName))
                        updateProducts.ForEach(p => p.OpenedSealName = modifyProductDto.OpenedSealName);

                    if (modifyProductDto.PreOrderDays.HasValue)
                        updateProducts.ForEach(p => p.PreOrderDays = modifyProductDto.PreOrderDays.Value);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductCategory))
                        updateProducts.ForEach(p => p.ProductCategory = modifyProductDto.ProductCategory);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductRemarks))
                        updateProducts.ForEach(p => p.ProductRemarks = modifyProductDto.ProductRemarks);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.Unit))
                        updateProducts.ForEach(p => p.Unit = modifyProductDto.Unit);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.ProductMachine))
                        updateProducts.ForEach(p => p.ProductMachine = modifyProductDto.ProductMachine);

                    if (modifyProductDto.IsNeedAcceptProcess.HasValue)
                        updateProducts.ForEach(p => p.IsNeedAcceptProcess = modifyProductDto.IsNeedAcceptProcess);

                    if (!string.IsNullOrWhiteSpace(modifyProductDto.StockLocation))
                        updateProducts.ForEach(p => p.StockLocation = modifyProductDto.StockLocation);
                    if (modifyProductDto.IsPrintSticker.HasValue)
                        updateProducts.ForEach(p => p.IsPrintSticker = modifyProductDto.IsPrintSticker);
                }

                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[UpdateProducts]：{msg}", ex);
                return (false, ex.Message);
            }

        }

        public (bool,string?) AddNewProduct(AddNewProductRequest request)
        {

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                string productCode ="";
                string? existingMaxProductCode = _dbContext.WarehouseProducts.Where(p => request.CompIds.Contains(p.CompId)).OrderByDescending(p => p.ProductCode).Select(p => p.ProductCode).FirstOrDefault();
                if (existingMaxProductCode != null)
                {
                    productCode = (int.Parse(existingMaxProductCode) + 1).ToString("D3");
                }
                else
                {
                    productCode = 1.ToString("D3");
                }

                foreach (var compId in request.CompIds)
                {
                    var newProduct = new WarehouseProduct();
                    newProduct.ProductCode = productCode; 
                    var comp = _dbContext.Companies.Where(c=>c.CompId==compId).FirstOrDefault();
                    newProduct.CompId = compId;
                    newProduct.CompName = comp.Name;
                    if (request.ManufacturerId != null)
                    {
                        var manufacturer = _dbContext.Manufacturers.Where(m => m.Id == request.ManufacturerId).FirstOrDefault();
                        newProduct.ManufacturerId = manufacturer.Id;
                        newProduct.ManufacturerName = manufacturer.Name;
                    }
                    if(request.DeadlineRule!=null) newProduct.DeadlineRule = request.DeadlineRule;
                    if(request.DeliverRemarks!=null) newProduct.DeliverRemarks = request.DeliverRemarks;
                    if(request.InStockQuantity!=null) newProduct.InStockQuantity = request.InStockQuantity;
                    if (request.Manager != null) newProduct.Manager = request.Manager;
                    if (request.MaxSafeQuantity != null) newProduct.MaxSafeQuantity = request.MaxSafeQuantity;
                    if (request.OpenDeadline != null) newProduct.OpenDeadline = request.OpenDeadline;
                    if (request.OpenedSealName != null) newProduct.OpenedSealName = request.OpenedSealName;
                    if (request.OriginalDeadline != null)
                    {
                        newProduct.OriginalDeadline = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.OriginalDeadline).Value);
                    }
                    if (request.PackageWay != null) newProduct.PackageWay = request.PackageWay;
                    if (request.PreDeadline != null) newProduct.PreDeadline = request.PreDeadline;
                    if (request.PreOrderDays != null) newProduct.PreOrderDays = request.PreOrderDays;
                    if (request.ProductCategory != null) newProduct.ProductCategory = request.ProductCategory;

                    

                    newProduct.ProductId = Guid.NewGuid().ToString();
                    if (request.ProductModel != null) newProduct.ProductModel = request.ProductModel;
                    if (request.ProductName != null) newProduct.ProductName = request.ProductName;
                    if (request.ProductRemarks != null) newProduct.ProductRemarks = request.ProductRemarks;
                    if (request.ProductSpec != null) newProduct.ProductSpec = request.ProductSpec;
                    if (request.SafeQuantity != null) newProduct.SafeQuantity = request.SafeQuantity;
                    if (request.UdibatchCode != null) newProduct.UdibatchCode = request.UdibatchCode;
                    if (request.UdicreateCode != null) newProduct.UdicreateCode = request.UdicreateCode;
                    if (request.UdiserialCode != null) newProduct.UdiserialCode = request.UdiserialCode;
                    if (request.UdiverifyDateCode != null) newProduct.UdiverifyDateCode = request.UdiverifyDateCode;
                    if (request.Unit != null) newProduct.Unit = request.Unit;
                    if (request.Weight != null) newProduct.Weight = request.Weight;
                    if (request.ProductMachine != null) newProduct.ProductMachine = request.ProductMachine;
                    if (request.DefaultSupplierId != null)
                    {
                        var supplier = _dbContext.Suppliers.Where(s => s.Id == request.DefaultSupplierId).FirstOrDefault();
                        newProduct.DefaultSupplierId = supplier.Id;
                        newProduct.DefaultSupplierName = supplier.Name;
                    }
                    if (request.IsNeedAcceptProcess != null) newProduct.IsNeedAcceptProcess = request.IsNeedAcceptProcess;

                    if (request.QcType != null) newProduct.QcType = request.QcType;
                    if (request.AllowReceiveDateRange != null) newProduct.AllowReceiveDateRange = request.AllowReceiveDateRange;
                    if (request.UnitConversion != null) newProduct.UnitConversion = request.UnitConversion;
                    if (request.TestCount != null) newProduct.TestCount = request.TestCount;
                    if (request.IsActive != null) newProduct.IsActive = request.IsActive;
                    if (request.IsPrintSticker != null) newProduct.IsPrintSticker = request.IsPrintSticker;
                    _dbContext.WarehouseProducts.Add(newProduct);
                }
                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[AddNewProduct]：{msg}", ex);
                return (false, ex.Message);
            }
        }

        public async Task<List<NotifyProductQuantity>> FindAllProductQuantityNotifyList()
        {
            var allProducts = GetAllActiveProducts();
            var allProductIdList = allProducts.Select(x => x.ProductId).ToList();
            List<NotifyProductQuantity> notifyProductQuantityList = new();
            var allUnDonePurchaseSubItemList = _purchaseService.GetUndonePurchaseSubItems(allProductIdList);
            allProducts.ForEach(product =>
            {
                var matchedUndoneSubItemList = allUnDonePurchaseSubItemList.Where(i => i.ProductId == product.ProductId).ToList();
                float inProcessingQrderQuantity = matchedUndoneSubItemList.Select(i => i.Quantity ?? 0.0f).DefaultIfEmpty(0.0f).Sum();
                NotifyProductQuantity notifyProductQuantity = new()
                {
                    ProductCode = product.ProductCode,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    InStockQuantity = product.InStockQuantity ?? 0.0f,
                    SafeQuantity = product.SafeQuantity ?? 0.0f,
                    MaxSafeQuantity = product.MaxSafeQuantity,
                    CompId = product.CompId,
                };
                notifyProductQuantityList.Add(notifyProductQuantity);
            });

            notifyProductQuantityList = notifyProductQuantityList.Where(p =>
            {
                float neededOrderQuantity = p.SafeQuantity - p.InProcessingQrderQuantity - p.InStockQuantity;
                if (neededOrderQuantity > 0)
                {
                    return true;
                }
                return false;
            }).ToList();
            return notifyProductQuantityList;
        }

        public async Task<List<NotifyProductQuantity>> FindAllProductQuantityNotifyList(string compId)
        {

            var allProducts = GetAllProducts(compId);
            var allProductIdList = allProducts.Select(x => x.ProductId).ToList();
            List<NotifyProductQuantity> notifyProductQuantityList = new();
            var allUnDonePurchaseSubItemList = _purchaseService.GetUndonePurchaseSubItems(allProductIdList);
            allProducts.ForEach(product =>
            {
                var matchedUndoneSubItemList = allUnDonePurchaseSubItemList.Where(i => i.ProductId == product.ProductId).ToList();
                float inProcessingQrderQuantity = matchedUndoneSubItemList.Select(i => i.Quantity ?? 0.0f).DefaultIfEmpty(0.0f).Sum();
                NotifyProductQuantity notifyProductQuantity = new()
                {
                    ProductCode = product.ProductCode,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    InStockQuantity = product.InStockQuantity ?? 0.0f,
                    SafeQuantity = product.SafeQuantity ?? 0.0f,
                    MaxSafeQuantity = product.MaxSafeQuantity,
                    CompId = product.CompId,
                };
                notifyProductQuantityList.Add(notifyProductQuantity);
            });

            notifyProductQuantityList = notifyProductQuantityList.Where(p =>
            {
                float neededOrderQuantity = p.SafeQuantity - p.InProcessingQrderQuantity - p.InStockQuantity;
                if (neededOrderQuantity > 0)
                {
                    return true;
                }
                return false;
            }).ToList();
            return notifyProductQuantityList;
        }

        public List<WarehouseProduct> GetProductsByGroupId(string groupId)
        {
            return _dbContext.WarehouseProducts.Where(p=>p.GroupIds!=null &&p.GroupIds.Contains(groupId)).ToList();
        }
    }
}
