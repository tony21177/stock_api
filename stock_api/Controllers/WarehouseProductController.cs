﻿using AutoMapper;
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
        public IActionResult SearchWarehouseProduct(WarehouseProductSearchRequest searchRequest)
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

            var allProductIsList = warehouseProductVoList.Select(p => p.ProductId).ToList();
            var allSubItems = _purchaseService.GetNotDonePurchaseSubItemByProductIdList(allProductIsList);
            var allPurchaseMainIdList = allSubItems.Select(p => p.PurchaseMainId).ToList();
            var allPurchaseMain = _purchaseService.GetPurchaseMainsByMainIdList(allPurchaseMainIdList);
            var allEffectivePurchaseMain = _purchaseService.GetPurchaseMainsByMainIdList(allPurchaseMainIdList)
                .Where(m => m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.REJECT && m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.CLOSE)
                .ToList();
            var allEffectivePurchaseMainId = allEffectivePurchaseMain.Select(m => m.PurchaseMainId).ToList();

            warehouseProductVoList.ForEach(p =>
            {
                var matchedOngoingSubItems = allSubItems.Where(s => s.ProductId == p.ProductId&& allEffectivePurchaseMainId.Contains(s.PurchaseMainId)).ToList();
                var inProcessingOrderQuantity = matchedOngoingSubItems
                    .Select(s => (s.Quantity - (s.InStockQuantity ?? 0.0f)))
                    .Sum();
                var needOrderedQuantity = p.MaxSafeQuantity ?? 0 - p.InStockQuantity ?? 0 - inProcessingOrderQuantity;
                var needOrderedQuantityUnitFloat = needOrderedQuantity * p.UnitConversion;
                var needOrderedQuantityUnit = Math.Ceiling((decimal)needOrderedQuantityUnitFloat.Value*100) / 100;  
                p.InProcessingOrderQuantity = inProcessingOrderQuantity??0.0f ;
                p.NeedOrderedQuantity = needOrderedQuantity ?? 0.0f;
                p.NeedOrderedQuantityUnit = (float)needOrderedQuantityUnit;
            });

            // 當過濾欄位為NeedOrderedQuantityUnit 需要特別處理
            if (searchRequest.PaginationCondition.OrderByField == "insufficientQuantity")
            {
                if (searchRequest.PaginationCondition.IsDescOrderBy)
                {
                    warehouseProductVoList = warehouseProductVoList.OrderByDescending(p=>p.NeedOrderedQuantityUnit).ToList();
                }
                else
                {
                    warehouseProductVoList = warehouseProductVoList.OrderBy(p => p.NeedOrderedQuantityUnit).ToList();
                }
                int totalItems = warehouseProductVoList.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / searchRequest.PaginationCondition.PageSize);
                warehouseProductVoList = warehouseProductVoList.Skip((searchRequest.PaginationCondition.Page - 1) * searchRequest.PaginationCondition.PageSize).Take(searchRequest.PaginationCondition.PageSize).ToList();
            }
            var allProductIdListForUsage = warehouseProductVoList.Select(p => p.ProductId).ToList();
            var productsLastMonthUsage = _stockOutService.GetLastMonthUsages();
            var productsLastYearUsage = _stockOutService.GetLastYearUsages();

            var productsThisYearAverageMonthUsage = _stockOutService.GetThisAverageMonthUsages();
             

            var allProducts = _warehouseProductService.GetAllProducts();

            warehouseProductVoList.ForEach(p =>
            {
                var matchedLastMonthUsage = productsLastMonthUsage.Where(u => u.ProductId == p.ProductId).FirstOrDefault();
                if (matchedLastMonthUsage != null) p.LastMonthUsageQuantity = matchedLastMonthUsage?.Quantity;
                var matchedLastYearUsage = productsLastYearUsage.Where(u => u.ProductId == p.ProductId).FirstOrDefault();
                if (matchedLastYearUsage != null) p.LastYearUsageQuantity = matchedLastYearUsage?.Quantity;
                var matchedThisYearAverageMonthUsage = productsThisYearAverageMonthUsage.Where(u => u.ProductId == p.ProductId).FirstOrDefault();
                if(matchedThisYearAverageMonthUsage!=null) p.ThisYearAverageMonthUsageQuantity = matchedThisYearAverageMonthUsage?.AverageQuantity;

                var matchedProductOfSameCodeList = allProducts.Where(product=> product.ProductCode==p.ProductCode&&product.IsActive==true).ToList();
                var distinctCompIds = matchedProductOfSameCodeList.Select(product => product.CompId).Distinct().ToList();
                p.ExistCompIds = distinctCompIds;
            });


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
        public IActionResult SearchUnderSafeQuantityWarehouseProduct(WarehouseProductSearchRequest searchRequest)
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

            searchRequest.PaginationCondition.PageSize = 100000;
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

            var allProductIsList = warehouseProductVoList.Select(p => p.ProductId).ToList();
            var allSubItems = _purchaseService.GetNotDonePurchaseSubItemByProductIdList(allProductIsList);
            var allPurchaseMainIdList = allSubItems.Select(p => p.PurchaseMainId).ToList();
            var allPurchaseMain = _purchaseService.GetPurchaseMainsByMainIdList(allPurchaseMainIdList);
            var allEffectivePurchaseMain = _purchaseService.GetPurchaseMainsByMainIdList(allPurchaseMainIdList)
                .Where(m => m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.REJECT && m.CurrentStatus != CommonConstants.PurchaseCurrentStatus.CLOSE)
                .ToList();
            var allEffectivePurchaseMainId = allEffectivePurchaseMain.Select(m => m.PurchaseMainId).ToList();
            var productsThisYearAverageMonthUsage = _stockOutService.GetThisAverageMonthUsages();

            warehouseProductVoList.ForEach(p =>
            {
                var matchedOngoingSubItems = allSubItems.Where(s => s.ProductId == p.ProductId&& allEffectivePurchaseMainId.Contains(s.PurchaseMainId)).ToList();
                var inProcessingOrderQuantity = matchedOngoingSubItems.Select(s => s.Quantity - s.InStockQuantity).DefaultIfEmpty(0).Sum();
                var needOrderedQuantity = p.MaxSafeQuantity ?? 0 - p.InStockQuantity ?? 0 - inProcessingOrderQuantity;
                p.InProcessingOrderQuantity = inProcessingOrderQuantity ?? 0;
                p.NeedOrderedQuantity = needOrderedQuantity ?? 0;
                var matchedProductThisYearAverageMonthUsage = productsThisYearAverageMonthUsage.Where(e => e.ProductId == p.ProductId).FirstOrDefault();
                if (matchedProductThisYearAverageMonthUsage != null)
                {
                    p.ThisYearAverageMonthUsageQuantity = matchedProductThisYearAverageMonthUsage.AverageQuantity ?? 0.0;
                }


            });

            var result = warehouseProductVoList.Where(p => p.InStockQuantity + p.InProcessingOrderQuantity < p.SafeQuantity).ToList();

            List<string> allProductCodeList = result.Where(p => p.ProductCode != null).Select(p => p.ProductCode).Distinct().ToList();
            // var (allInStockRecords, allOutStockRecords) = _stockInService.GetAllInAndOutRecordByProductCodeList(allProductCodeList, compId);
            // result.ForEach(vo =>
            //  {
            //      var matchedInStockRecords = allInStockRecords.Where(r => r.ProductCode == vo.ProductCode).OrderByDescending(r => r.UpdatedAt).ToList();
            //      var matchedOutStockRecords = allOutStockRecords.Where(r => r.ProductCode == vo.ProductCode).OrderByDescending(r => r.UpdatedAt).ToList();
            //      vo.InStockRecords = matchedInStockRecords;
            //      vo.OutStockRecords = matchedOutStockRecords;
            //  });

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
            return Ok(new CommonResponse<dynamic>()
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
            var inStockLots = _mapper.Map<List<InStockLots>>(stockInRecordList);
            foreach (var item in inStockLots)
            {
                item.RemainingQuantity = item.InStockQuantity - item.OutStockQuantity??0.0f - item.RejectQuantity??0.0f;
            }



            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = inStockLots
            });
        }
    }
}
