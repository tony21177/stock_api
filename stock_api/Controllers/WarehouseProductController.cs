using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common.Constant;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Service;
using stock_api.Utils;
using stock_api.Models;
using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Controllers.Validator;
using stock_api.Auth;
using stock_api.Service.ValueObject;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using MySqlX.XDevAPI.Common;
using AutoMapper.Execution;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Threading.Tasks;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseProductController : ControllerBase
    {

        private readonly AuthLayerService _authLayerService;
        private readonly CompanyService _companyService;
        private readonly GroupService _groupService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly SupplierService _supplierService;
        private readonly ManufacturerService _manufacturerService;
        private readonly IMapper _mapper;
        private readonly ILogger<WarehouseProductController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly IValidator<WarehouseProductSearchRequest> _searchProductRequestValidator;
        private readonly IValidator<UpdateProductRequest> _updateProductValidator;
        private readonly IValidator<AdminUpdateProductRequest> _adminUpdateProductValidator;
        private readonly IValidator<AddNewProductRequest> _addNewProductRequestValidator;
        private readonly IValidator<UpdateProductToCompRequest> _updateProductToCompRequestValidator;
        private readonly FileUploadService _fileUploadService;
        private readonly StockInService _stockInService;
        private readonly PurchaseService _purchaseService;
        private readonly StockOutService _stockOutService;
        private readonly InstrumentService _instrumentService;

        public WarehouseProductController(AuthLayerService authLayerService, WarehouseProductService warehouseProductService, CompanyService companyService, GroupService groupService, SupplierService supplierService,
            ManufacturerService manufacturerService, IMapper mapper, ILogger<WarehouseProductController> logger, AuthHelpers authHelpers, FileUploadService fileUploadService, StockInService stockInService, PurchaseService purchaseService
            ,StockOutService stockOutService,InstrumentService instrumentService)
        {
            _authLayerService = authLayerService;
            _warehouseProductService = warehouseProductService;
            _groupService = groupService;
            _supplierService = supplierService;
            _manufacturerService = manufacturerService;
            _mapper = mapper;
            _logger = logger;
            _authHelpers = authHelpers;
            _companyService = companyService;
            _searchProductRequestValidator = new SearchProductRequestValidator(companyService, groupService);
            _updateProductValidator = new UpdateProductValidator(supplierService, groupService,instrumentService);
            _adminUpdateProductValidator = new AdminUpdateProductValidator(supplierService, groupService, manufacturerService,instrumentService);
            _addNewProductRequestValidator = new AddNewProductValidator(supplierService, manufacturerService, companyService);
            _updateProductToCompRequestValidator = new UpdateProductToCompValidator(companyService, warehouseProductService);
            _fileUploadService = fileUploadService;
            _stockInService = stockInService;
            _purchaseService = purchaseService;
            _stockOutService = stockOutService;
            _instrumentService = instrumentService;
        }

        [HttpPost("search")]
        [Authorize]
        public async Task<IActionResult> SearchWarehouseProduct(WarehouseProductSearchRequest searchRequest)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var compType = memberAndPermissionSetting.CompanyWithUnit.Type;

            if (searchRequest.CompId == null)
            {
                searchRequest.CompId = compId;
            }

            if (searchRequest.CompId != null && searchRequest.CompId != compId && compType != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            
            // 對於 search API，跳過不必要的資料庫驗證
            // CompId 已經在 JWT 驗證過，且 GroupId 為 null 時不需要驗證
            if (searchRequest.GroupId != null)
            {
                var validationResult = _searchProductRequestValidator.Validate(searchRequest);
                if (!validationResult.IsValid)
                {
                    return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
                }
            }

            var (data, totalPages) = _warehouseProductService.SearchProduct(searchRequest);
            var warehouseProductVoList = _mapper.Map<List<WarehouseProductVo>>(data);
            _logger.LogInformation("[SearchWarehouseProduct] SearchProduct + Mapping 完成: {elapsed}ms, count: {count}", sw.ElapsedMilliseconds, warehouseProductVoList?.Count ?? 0);

            var distictProductCodeList = warehouseProductVoList.Select(x => x.ProductCode).Distinct().ToList();
            var productsInAnotherComp = _warehouseProductService.GetProductByProductCodeList(distictProductCodeList);

            // 使用 Dictionary 優化查找效能 (O(n) -> O(1))
            var productsByCodeLookup = productsInAnotherComp
                .GroupBy(p => p.ProductCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (compType == CommonConstants.CompanyType.OWNER && productsInAnotherComp.Count > 0)
            {
                foreach (var item in warehouseProductVoList)
                {
                    if (productsByCodeLookup.TryGetValue(item.ProductCode, out var matchedProducts))
                    {
                        var matchedProdcutInAnotherComp = matchedProducts.FirstOrDefault(p => p.CompId != compId);
                        if (matchedProdcutInAnotherComp != null)
                        {
                            item.AnotherUnit = matchedProdcutInAnotherComp.Unit;
                            item.AnotherUnitConversion = matchedProdcutInAnotherComp.UnitConversion;
                        }
                    }
                }
            }

            var allProductIsList = warehouseProductVoList.Select(p => p.ProductId).ToList();

            // 並行執行查詢
            var swParallel = System.Diagnostics.Stopwatch.StartNew();
            var taskPurchaseSubItems = _purchaseService.GetNotDonePurchaseSubItemsByProductIdListOptimizedAsync(allProductIsList);
            var taskCombinedUsages = _stockOutService.GetCombinedUsagesAsync(allProductIsList);

            await Task.WhenAll(taskPurchaseSubItems, taskCombinedUsages);
            swParallel.Stop();
            _logger.LogInformation("[SearchWarehouseProduct] Parallel queries: {elapsed}ms", swParallel.ElapsedMilliseconds);

            var allSubItems = taskPurchaseSubItems.Result ?? new List<PurchaseSubItem>();
            var combinedUsages = taskCombinedUsages.Result ?? new List<CombinedUsageVo>();

            // Post-processing: 使用 Dictionary 優化所有查找
            var swPostProc = System.Diagnostics.Stopwatch.StartNew();
            
            // 建立所有必要的 Lookup (一次性)
            var subItemsByProductId = allSubItems.ToLookup(s => s.ProductId);
            var combinedUsageLookup = combinedUsages.ToDictionary(u => u.ProductId);
            var existCompIdsByProductCode = productsInAnotherComp
                .Where(p => p.IsActive == true)
                .ToLookup(p => p.ProductCode, p => p.CompId);

            // 單一迴圈處理所有計算與指派
            foreach (var p in warehouseProductVoList)
            {
                // 計算 NeedOrderedQuantity
                var matchedOngoingSubItems = subItemsByProductId[p.ProductId];
                float inProcessingOrderQuantity = 0;
                foreach (var s in matchedOngoingSubItems)
                {
                    inProcessingOrderQuantity += (s.Quantity ?? 0) - (s.InStockQuantity ?? 0.0f);
                }
                
                var needOrderedQuantity = (p.MaxSafeQuantity ?? 0) - (p.InStockQuantity ?? 0) - inProcessingOrderQuantity;
                var needOrderedQuantityUnitFloat = needOrderedQuantity * (p.UnitConversion ?? 1);
                var needOrderedQuantityUnit = Math.Ceiling((decimal)needOrderedQuantityUnitFloat * 100) / 100;
                
                p.InProcessingOrderQuantity = inProcessingOrderQuantity;
                p.NeedOrderedQuantity = needOrderedQuantity;
                p.NeedOrderedQuantityUnit = (float)needOrderedQuantityUnit;

                // 指派 Usage 資料
                if (combinedUsageLookup.TryGetValue(p.ProductId, out var usage))
                {
                    p.LastMonthUsageQuantity = usage.LastMonthQuantity ?? 0;
                    p.ThisYearAverageMonthUsageQuantity = usage.ThisYearAverageQuantity ?? 0;
                    p.LastYearUsageQuantity = usage.LastYearQuantity ?? 0;
                }

                // 指派 ExistCompIds
                p.ExistCompIds = existCompIdsByProductCode[p.ProductCode].Distinct().ToList();
            }

            swPostProc.Stop();
            sw.Stop();
            _logger.LogInformation("[SearchWarehouseProduct] Post-processing: {postMs}ms, Total: {totalMs}ms", swPostProc.ElapsedMilliseconds, sw.ElapsedMilliseconds);

            var response = new CommonResponse<List<WarehouseProductVo>>()
            {
                Result = true,
                Message = "",
                Data = warehouseProductVoList,
                TotalPages = totalPages
            };
            return Ok(response);
        }

        
        [HttpPost("searchForUnderSafeQuantity")]
        [Authorize]
        public async Task<IActionResult> SearchUnderSafeQuantityWarehouseProduct(WarehouseProductSearchRequest searchRequest)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var compType = memberAndPermissionSetting.CompanyWithUnit.Type;

            if (searchRequest.CompId == null)
            {
                searchRequest.CompId = compId;
            }

            if (searchRequest.CompId != null && searchRequest.CompId != compId && compType != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _searchProductRequestValidator.Validate(searchRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            searchRequest.PaginationCondition.PageSize = 100000;
            var (data, totalPages) = _warehouseProductService.SearchProduct(searchRequest);
            var warehouseProductVoList = _mapper.Map<List<WarehouseProductVo>>(data);
            _logger.LogInformation("[SearchUnderSafeQuantity] SearchProduct + Mapping: {elapsed}ms, count: {count}", sw.ElapsedMilliseconds, warehouseProductVoList?.Count ?? 0);

            var allProductIdList = warehouseProductVoList.Select(p => p.ProductId).ToList();
            var distictProductCodeList = warehouseProductVoList.Select(x => x.ProductCode).Distinct().ToList();

            // 平行執行所有獨立查詢 (包含 GetProductByProductCodeList)
            var swParallel = System.Diagnostics.Stopwatch.StartNew();
            var taskSubItems = _purchaseService.GetNotDonePurchaseSubItemsByProductIdListOptimizedAsync(allProductIdList);
            var taskCombinedUsages = _stockOutService.GetCombinedUsagesAsync(allProductIdList); // 使用快取版本
            var taskProductsInAnotherComp = Task.Run(() => _warehouseProductService.GetProductByProductCodeList(distictProductCodeList));

            await Task.WhenAll(taskSubItems, taskCombinedUsages, taskProductsInAnotherComp);
            swParallel.Stop();
            _logger.LogInformation("[SearchUnderSafeQuantity] Parallel queries: {elapsed}ms", swParallel.ElapsedMilliseconds);

            var allSubItems = taskSubItems.Result ?? new List<PurchaseSubItem>();
            var combinedUsages = taskCombinedUsages.Result ?? new List<CombinedUsageVo>();
            var productsInAnotherComp = taskProductsInAnotherComp.Result ?? new List<WarehouseProduct>();

            // 使用 Dictionary 優化 OWNER 查找 (O(n) -> O(1))
            if (compType == CommonConstants.CompanyType.OWNER && productsInAnotherComp.Count > 0)
            {
                var productsByCodeLookup = productsInAnotherComp
                    .GroupBy(p => p.ProductCode)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in warehouseProductVoList)
                {
                    if (productsByCodeLookup.TryGetValue(item.ProductCode, out var matchedProducts))
                    {
                        var matchedProdcutInAnotherComp = matchedProducts.FirstOrDefault(p => p.CompId != compId);
                        if (matchedProdcutInAnotherComp != null)
                        {
                            item.AnotherUnit = matchedProdcutInAnotherComp.Unit;
                            item.AnotherUnitConversion = matchedProdcutInAnotherComp.UnitConversion;
                        }
                    }
                }
            }

            // 建立 Lookup (一次性)
            var subItemsByProductId = allSubItems.ToLookup(s => s.ProductId);
            var usageLookup = combinedUsages.ToDictionary(u => u.ProductId, u => u);

            // 單一迴圈處理所有計算
            var swPostProc = System.Diagnostics.Stopwatch.StartNew();
            foreach (var p in warehouseProductVoList)
            {
                // 使用 Lookup O(1) 查找
                var matchedOngoingSubItems = subItemsByProductId[p.ProductId];
                float inProcessingOrderQuantity = 0;
                foreach (var s in matchedOngoingSubItems)
                {
                    inProcessingOrderQuantity += (s.Quantity ?? 0) - (s.InStockQuantity ?? 0.0f);
                }

                var needOrderedQuantity = (p.MaxSafeQuantity ?? 0) - (p.InStockQuantity ?? 0) - inProcessingOrderQuantity;
                p.InProcessingOrderQuantity = inProcessingOrderQuantity;
                p.NeedOrderedQuantity = needOrderedQuantity;

                // 使用 Dictionary O(1) 查找 (改用 CombinedUsageVo)
                if (usageLookup.TryGetValue(p.ProductId, out var matchedUsage))
                {
                    p.ThisYearAverageMonthUsageQuantity = matchedUsage.ThisYearAverageQuantity ?? 0.0;
                }
            }
            swPostProc.Stop();
            _logger.LogInformation("[SearchUnderSafeQuantity] Post-processing: {elapsed}ms", swPostProc.ElapsedMilliseconds);

            // 過濾低於安全庫存的產品
            var result = warehouseProductVoList.Where(p => p.InStockQuantity + p.InProcessingOrderQuantity < p.SafeQuantity).ToList();

            // 當過濾欄位為 NeedOrderedQuantityUnit 需要特別處理
            if (searchRequest.PaginationCondition.OrderByField == "insufficientQuantity")
            {
                if (searchRequest.PaginationCondition.IsDescOrderBy)
                {
                    result = result.OrderByDescending(p => p.NeedOrderedQuantityUnit).ToList();
                }
                else
                {
                    result = result.OrderBy(p => p.NeedOrderedQuantityUnit).ToList();
                }
                int totalItems = result.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / searchRequest.PaginationCondition.PageSize);
                result = result.Skip((searchRequest.PaginationCondition.Page - 1) * searchRequest.PaginationCondition.PageSize).Take(searchRequest.PaginationCondition.PageSize).ToList();
            }

            sw.Stop();
            _logger.LogInformation("[SearchUnderSafeQuantity] TOTAL: {elapsed}ms, resultCount: {count}", sw.ElapsedMilliseconds, result.Count);

            var response = new CommonResponse<List<WarehouseProductVo>>()
            {
                Result = true,
                Message = "",
                Data = result,
                TotalPages = totalPages
            };
            return Ok(response);
        }

        [HttpPost("searchForAdjust")]
        [Authorize]
        public IActionResult SearchWarehouseProductForAdjust(WarehouseProductSearchRequest searchRequest)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var compType = memberAndPermissionSetting.CompanyWithUnit.Type;

            if (searchRequest.CompId == null)
            {
                searchRequest.CompId = compId;
            }

            if (searchRequest.CompId != null && searchRequest.CompId != compId && compType != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _searchProductRequestValidator.Validate(searchRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var (data, totalPages) = _warehouseProductService.SearchProduct(searchRequest);
            var warehouseProductVoList = _mapper.Map<List<WarehouseProductVo>>(data);
            var distictProductCodeList = warehouseProductVoList.Select(x => x.ProductCode).Distinct().ToList();
            var productsInAnotherComp = _warehouseProductService.GetProductByProductCodeList(distictProductCodeList);

            if (compType == CommonConstants.CompanyType.OWNER && productsInAnotherComp.Count > 0)
            {
                foreach (var item in warehouseProductVoList)
                {
                    var matchedProdcutsInAnotherComp = productsInAnotherComp.Where(p => p.ProductCode.Contains(item.ProductCode) && p.CompId != compId).ToList();
                    if (matchedProdcutsInAnotherComp.Count > 0)
                    {
                        // 因為金萬林此product的unit在不同醫院單位都會設成一樣,故只取第一筆
                        var matchedProdcutInAnotherComp = matchedProdcutsInAnotherComp[0];
                        item.AnotherUnit = matchedProdcutInAnotherComp.Unit;
                        item.AnotherUnitConversion = matchedProdcutInAnotherComp.UnitConversion;
                    }
                }
            }
            List<string> allProductCodeList = warehouseProductVoList.Where(p => p.ProductCode != null).Select(p => p.ProductCode).Distinct().ToList();
            var (allInStockRecords, allOutStockRecords) = _stockInService.GetAllInAndOutRecordByProductCodeList(allProductCodeList, compId);

            warehouseProductVoList.ForEach(vo =>
            {
                var matchedInStockRecords = allInStockRecords.Where(r => r.ProductCode == vo.ProductCode).OrderByDescending(r => r.UpdatedAt).ToList();
                var matchedOutStockRecords = allOutStockRecords.Where(r => r.ProductCode == vo.ProductCode).OrderByDescending(r => r.UpdatedAt).ToList();
                vo.InStockRecords = matchedInStockRecords;
                vo.OutStockRecords = matchedOutStockRecords;
            });

            // 當過濾欄位為NeedOrderedQuantityUnit 需要特別處理
            if (searchRequest.PaginationCondition.OrderByField == "insufficientQuantity")
            {
                if (searchRequest.PaginationCondition.IsDescOrderBy)
                {
                    warehouseProductVoList = warehouseProductVoList.OrderByDescending(p => p.NeedOrderedQuantityUnit).ToList();
                }
                else
                {
                    warehouseProductVoList = warehouseProductVoList.OrderBy(p => p.NeedOrderedQuantityUnit).ToList();
                }
                int totalItems = warehouseProductVoList.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / searchRequest.PaginationCondition.PageSize);
                warehouseProductVoList = warehouseProductVoList.Skip((searchRequest.PaginationCondition.Page - 1) * searchRequest.PaginationCondition.PageSize).Take(searchRequest.PaginationCondition.PageSize).ToList();
            }

            var response = new CommonResponse<List<WarehouseProductVo>>()
            {
                Result = true,
                Message = "",
                Data = warehouseProductVoList,
                TotalPages = totalPages
            };
            return Ok(response);

        }

        [HttpPost("adminDetail")]
        [Authorize]
        public IActionResult GetWarehouseProductAdminDetail(ProductDetailRequest request)
        {
            var data = _warehouseProductService.GetProductByProductId(request.ProductId);

            var productWithInstrument = _mapper.Map<WarehouseProductWithInstruments>(data);

            var product = _warehouseProductService.GetProductByProductId(productWithInstrument.ProductId);
            var productWithInstruments = _mapper.Map<WarehouseProductWithInstruments>(product);
            var productInstruments = _warehouseProductService.GetProductInstrumentsByProductId(product.ProductId);
            productWithInstruments.InstrumentIdList = productInstruments.Select(pi => pi.InstrumentId).ToList();
            productWithInstruments.InstrumentNameList = productInstruments.Select(pi => pi.InstrumentName).ToList();


            var response = new CommonResponse<WarehouseProductWithInstruments>()
            {
                Result = true,
                Message = "",
                Data = productWithInstruments
            };
            return Ok(response);

        }


        [HttpPost("detailByCode")]
        [Authorize]
        public IActionResult GetWarehouseProductDetailByCode(ProductDetailByCodeRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }

            var data = _warehouseProductService.GetProductByProductCodeAndCompId(request.ProductCode, request.CompId);


            var response = new CommonResponse<WarehouseProduct>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return Ok(response);

        }

        [HttpPost("detail")]
        [Authorize]
        public IActionResult GetWarehouseProductDetail(ProductDetailRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var compType = memberAndPermissionSetting.CompanyWithUnit.Type;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }

            if (request.CompId != null && request.CompId != compId && compType != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var product = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId, request.CompId);
            var productWithInstruments = _mapper.Map<WarehouseProductWithInstruments>(product);
            var productInstruments = _warehouseProductService.GetProductInstrumentsByProductId(product.ProductId);
            productWithInstruments.InstrumentIdList = productInstruments.Select(pi => pi.InstrumentId).ToList();
            productWithInstruments.InstrumentNameList = productInstruments.Select(pi => pi.InstrumentName).ToList();



            var response = new CommonResponse<WarehouseProductWithInstruments>()
            {
                Result = true,
                Message = "",
                Data = productWithInstruments
            };
            return Ok(response);

        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult UpdateProduct(UpdateProductRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;
            var compType = memberAndPermissionSetting.CompanyWithUnit.Type;
            if (memberAndPermissionSetting.PermissionSetting.IsItemManage == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var validationResult = _updateProductValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var existingProduct = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId, compId);
            if (existingProduct == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "品項不存在"
                });
            }
            var groups = _groupService.GetGroupsByIdList(request.GroupIds);

            var result = _warehouseProductService.UpdateProduct(request, existingProduct, groups);


            var response = new CommonResponse<WarehouseProduct>()
            {
                Result = result,
                Message = "",
            };
            return Ok(response);

        }

        [HttpPost("adminUpdate")]
        [AuthorizeRoles("1")]
        public IActionResult AdminUpdateProduct(AdminUpdateProductRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;
            if (memberAndPermissionSetting.PermissionSetting.IsItemManage == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var validationResult = _adminUpdateProductValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var existingProduct = _warehouseProductService.GetProductByProductId(request.ProductId);
            if (existingProduct == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "品項不存在"
                });
            }
            var groups = _groupService.GetGroupsByIdList(request.GroupIds);
            Supplier? supplier = null;
            if (request.DefaultSupplierId != null)
            {
                supplier = _supplierService.GetSupplierById(request.DefaultSupplierId.Value);
            }
            Manufacturer? manufacturer = null;
            if (request.ManufacturerId != null)
            {
                manufacturer = _manufacturerService.GetManufacturerById(request.ManufacturerId);
            }

            var result = _warehouseProductService.AdminUpdateProduct(request, existingProduct, supplier, manufacturer, groups);


            var response = new CommonResponse<WarehouseProduct>()
            {
                Result = result,
                Message = "",
            };
            return Ok(response);

        }

        //[HttpPost("uploadImage")]
        //[Authorize]
        //public async Task<IActionResult> UploadImage([FromForm]  UploadProductImageRequest request)
        //{
        //    var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
        //    var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
        //    request.CompId = compId;
        //    if (memberAndPermissionSetting.PermissionSetting.IsItemManage == false)
        //    {
        //        return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
        //    }

        //    if (request.Image==null || request.Image.Length == 0)
        //    {
        //        return BadRequest(new CommonResponse<dynamic>
        //        {
        //            Result = false,
        //            Message = "無效的圖檔"
        //        });
        //    }
        //    var product = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId,compId);
        //    if (product == null)
        //    {
        //        return BadRequest(new CommonResponse<dynamic>
        //        {
        //            Result = false,
        //            Message = "此品項不存在"
        //        });
        //    }

        //    using var memoryStream = new MemoryStream();
        //    await request.Image.CopyToAsync(memoryStream);
        //    var imageBytes = memoryStream.ToArray();
        //    var imageBase64 = Convert.ToBase64String(imageBytes);
        //    bool result = _warehouseProductService.UpdateOrAddProductImage(imageBase64, request.ProductId, request.CompId);
        //    return Ok(new CommonResponse<dynamic>()
        //    {
        //        Result = result
        //    }); ;

        //}

        [HttpPost("uploadImage")]
        [Authorize]
        public async Task<IActionResult> UploadImage(UploadProductImageRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;
            if (memberAndPermissionSetting.PermissionSetting.IsItemManage == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            if (string.IsNullOrEmpty(request.Image))
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "無效的圖檔"
                });
            }
            var product = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId, compId);
            if (product == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此品項不存在"
                });
            }

            var data = new Regex(@"^data:image\/[a-zA-Z]+;base64,").Replace(request.Image, string.Empty);
            var bytes = Convert.FromBase64String(data);
            using var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, product.ProductId, product.ProductId)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            bool result = await _warehouseProductService.UpdateOrAddProductImage(file, request.ProductId, request.CompId);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result
            }); ;

        }

        [HttpGet("image/{productId}")]
        [Authorize]
        public async Task<IActionResult> GetProductImage(string productId)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (memberAndPermissionSetting.PermissionSetting.IsItemManage == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var product = _warehouseProductService.GetProductByProductIdAndCompId(productId, compId);
            if (product == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此品項不存在"
                });
            }

            try
            {
                //var productImage = _warehouseProductService.GetProductImage(productId,compId);
                //if (product == null || productImage ==null|| productImage.Image==null)
                //{
                //    return NotFound("Product or image not found.");
                //}

                //var fileStream = _fileUploadService.Download(productImage.Image, productImage.ProductId, "image/png");
                //return fileStream;
                var productImage = _warehouseProductService.GetProductImage(productId, compId);
                if (product == null || productImage == null || productImage.Image == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "品項或圖片找不到"
                    });
                }
                var imageByteArray = _fileUploadService.DownloadToByte(productImage.Image);
                var data = Convert.ToBase64String(imageByteArray);
                return Ok(new CommonResponse<dynamic>
                {
                    Result = false,
                    Data = data
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the image.");
                return StatusCode(StatusCodes.Status404NotFound, "An error occurred while retrieving the image.");
            }
        }

        [HttpPost("updateAllCompProduct")]
        [Authorize]
        public IActionResult BatchUpdateAllCompProduct(BatchUpdateProducts request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var allDistinctProducts = request.ModifyProductDtoList.Select(x => x.ProductCode).Distinct().ToList();
            var allProducts = _warehouseProductService.GetProductByProductCodeList(allDistinctProducts);
            var allDistinctGroupNames = request.ModifyProductDtoList.Select(i => i.GroupNames).Where(i => i != null).SelectMany(i => i.Split(','))
                .Distinct().ToList();
            var allGroups = _groupService.GetGroupsByGroupNameList(allDistinctGroupNames);

            var (result, errorMsg) = _warehouseProductService.UpdateProducts(request.ModifyProductDtoList, allProducts, allGroups);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = errorMsg
            });
        }

        [HttpPost("updateOwnCompProduct")]
        [Authorize]
        public IActionResult BatchUpdateOwnCompProduct(BatchUpdateProducts request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var allDistinctProducts = request.ModifyProductDtoList.Select(x => x.ProductCode).Distinct().ToList();
            var allProducts = _warehouseProductService.GetProductsByProductCodesAndCompId(allDistinctProducts, compId);
            var allDistinctGroupNames = request.ModifyProductDtoList.Select(i => i.GroupNames).Where(i => i != null).SelectMany(i => i.Split(','))
               .Distinct().ToList();
            var allGroups = _groupService.GetGroupsByGroupNameListAndCompId(allDistinctGroupNames, compId);

            var (result, errorMsg) = _warehouseProductService.UpdateProducts(request.ModifyProductDtoList, allProducts, allGroups, compId);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = errorMsg
            });
        }

        [HttpPost("addNewProduct")]
        [Authorize]
        public IActionResult AddNewProduct(AddNewProductRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _addNewProductRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {

                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            request.CompIds.Add(compId);
            request.CompIds = request.CompIds.Distinct().ToList();

            var (result, errorMsg) = _warehouseProductService.AddNewProduct(request);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = errorMsg
            });
        }

        [HttpPost("unDonePurchaseItems")]
        [Authorize]
        public IActionResult GetUndonePurchaseItems(GetUndonePurchaseRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId ??= compId;
            List<PurchaseItemListView> purchaseSubItems = _purchaseService.GetUndonePurchaseSubItems(request.CompId, request.ProductId);
            List<string> mainIdList = purchaseSubItems.Select(s => s.PurchaseMainId).Distinct().ToList();
            List<PurchaseMainSheet> purchaseMainSheets = _purchaseService.GetPurchaseMainsByMainIdList(mainIdList);
            List<UnDonePurchaseSubItem> unDonePurchaseSubItems = _mapper.Map<List<UnDonePurchaseSubItem>>(purchaseSubItems);
            List<WarehouseProduct> products = _warehouseProductService.GetProductsByProductIdsAndCompId(unDonePurchaseSubItems.Select(s=>s.ProductId).ToList(), request.CompId);
            unDonePurchaseSubItems.ForEach(unDonePurchaseSubItem =>
            {
                var matchedPurchaseMain = purchaseMainSheets.Where(m => m.PurchaseMainId == unDonePurchaseSubItem.PurchaseMainId).FirstOrDefault();
                var matchedProduct = products.Where(m => m.ProductId==unDonePurchaseSubItem.ProductId).FirstOrDefault();
                unDonePurchaseSubItem.PurchaseMain = matchedPurchaseMain;
                unDonePurchaseSubItem.Unit = matchedProduct.Unit;
            });
            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = unDonePurchaseSubItems
            });

        }

        [HttpPost("owner/updateProductToComp")]
        [Authorize]
        public IActionResult UpdateProductToComp(UpdateProductToCompRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.FromCompId == null)
            {
                request.FromCompId = compId;
            }
            var validationResult = _updateProductToCompRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var product = _warehouseProductService.GetProductByProductCodeAndCompId(request.ProductCode,request.FromCompId);
            _warehouseProductService.UpdateProductToCompIds(request.ToCompIds,product,request.IsActive);

            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
            });
        }

        [HttpPost("searchProductLotQuantity")]
        [Authorize]
        public IActionResult SearchProductLotQuantity(SearchProductLotQuantityRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }
            
            var stockInRecordList = _stockInService.GetInStockRecordsNotAllOutOrReject(request.ProductId);
            var products = _warehouseProductService.GetProductsByCompId( request.CompId).Where(p=>stockInRecordList.Select(e=>e.ProductId).Contains(p.ProductId)).ToList();


            var inStockLots = _mapper.Map<List<InStockLots>>(stockInRecordList);
            foreach (var item in inStockLots)
            {
                var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.RemainingQuantity = item.InStockQuantity + item.AdjustInQuantity - item.OutStockQuantity- item.RejectQuantity - item.AdjustOutQuantity;
                if (item.LotNumberBatch.Contains(":A")) item.RemainingQuantity = 0; // :A的批號結餘量算院批號內
                item.ProductUnit = matchedProduct?.Unit;
                item.GroupName = matchedProduct?.GroupNames;
                item.OpenDeadline = matchedProduct?.OpenDeadline;
                item.ProductModel = matchedProduct?.ProductModel;
                item.SavingTemperature = matchedProduct?.SavingTemperature;
                item.SavingFunction = matchedProduct?.SavingFunction;
            }

            inStockLots = inStockLots.Where(x => x.RemainingQuantity != 0).ToList();

            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = inStockLots
            });
        }
    }
}
