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
        private readonly ILogger<AuthlayerController> _logger;
        private readonly AuthHelpers _authHelpers;
        private readonly IValidator<WarehouseProductSearchRequest> _searchProductRequestValidator;
        private readonly IValidator<UpdateProductRequest> _updateProductValidator;
        private readonly IValidator<AdminUpdateProductRequest> _adminUpdateProductValidator;

        public WarehouseProductController(AuthLayerService authLayerService, WarehouseProductService warehouseProductService, CompanyService companyService, GroupService groupService, SupplierService supplierService,
            ManufacturerService manufacturerService, IMapper mapper, ILogger<AuthlayerController> logger, AuthHelpers authHelpers)
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
            _updateProductValidator = new UpdateProductValidator(supplierService, groupService);
            _adminUpdateProductValidator = new AdminUpdateProductValidator(supplierService, groupService, manufacturerService);
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


            var response = new CommonResponse<WarehouseProduct>()
            {
                Result = true,
                Message = "",
                Data = data
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

            var data = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId, request.CompId);


            var response = new CommonResponse<WarehouseProduct>()
            {
                Result = true,
                Message = "",
                Data = data
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

        [HttpPost("uploadImage")]
        [Authorize]
        public async Task<IActionResult> UploadImage([FromForm]  UploadProductImageRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;
            if (memberAndPermissionSetting.PermissionSetting.IsItemManage == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            if (request.Image==null || request.Image.Length == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "無效的圖檔"
                });
            }
            var product = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId,compId);
            if (product == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "此品項不存在"
                });
            }

            using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            var imageBase64 = Convert.ToBase64String(imageBytes);
            bool result = _warehouseProductService.UpdateOrAddProductImage(imageBase64, request.ProductId, request.CompId);
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
                var productImage = _warehouseProductService.GetProductImage(productId,compId);
                if (product == null || string.IsNullOrEmpty(productImage.Image))
                {
                    return NotFound("Product or image not found.");
                }

                var imageBytes = Convert.FromBase64String(productImage.Image);
                return File(imageBytes, "image/jpeg"); // Assuming the image is a jpeg, you can change the MIME type as needed.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the image.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the image.");
            }
        }
    }
}
