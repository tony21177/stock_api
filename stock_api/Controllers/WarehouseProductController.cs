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

        public WarehouseProductController(AuthLayerService authLayerService, WarehouseProductService warehouseProductService,CompanyService companyService, GroupService groupService,SupplierService supplierService,
            ManufacturerService manufacturerService,IMapper mapper, ILogger<AuthlayerController> logger, AuthHelpers authHelpers)
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
            _updateProductValidator = new UpdateProductValidator(supplierService,groupService);
            _adminUpdateProductValidator = new AdminUpdateProductValidator(supplierService, groupService,manufacturerService);
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

            if(searchRequest.CompId!=null&& searchRequest.CompId != compId && compType != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _searchProductRequestValidator.Validate(searchRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var (data,totalPages) = _warehouseProductService.SearchProduct(searchRequest);

            var response = new CommonResponse<List<WarehouseProduct>>()
            {
                Result = true,
                Message = "",
                Data = data,
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
            var compType = memberAndPermissionSetting.CompanyWithUnit.Type;
            if (memberAndPermissionSetting.PermissionSetting.IsItemManage == false)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var validationResult =  _updateProductValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }


            var existingProduct = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId,compId);
            if (existingProduct == null)
            {
                return BadRequest(new CommonResponse<dynamic>(){
                    Result = false,
                    Message = "品項不存在"
                });
            }
            var groups = _groupService.GetGroupsByIdList(request.GroupIds);

            _warehouseProductService.UpdateProduct(request,existingProduct,groups);


            var response = new CommonResponse<WarehouseProduct>()
            {
                Result = true,
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

            _warehouseProductService.AdminUpdateProduct(request, existingProduct, supplier, manufacturer, groups);


            var response = new CommonResponse<WarehouseProduct>()
            {
                Result = true,
                Message = "",
            };
            return Ok(response);

        }
    }
}
