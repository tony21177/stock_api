using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly AuthLayerService _authLayerService;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly SupplierService _supplierService;
        private readonly CompanyService _companyService;
        private readonly IValidator<CreateSupplierRequest> _createSupplierValidator;
        private readonly IValidator<UpdateSupplierRequest> _updateSupplierValidator;

        public SupplierController(AuthLayerService authLayerService, IMapper mapper, AuthHelpers authHelpers, SupplierService supplierService, CompanyService companyService)
        {
            _authLayerService = authLayerService;
            _mapper = mapper;
            _authHelpers = authHelpers;
            _supplierService = supplierService;
            _companyService = companyService;
            _createSupplierValidator = new CreateSupplierValidator(companyService);
            _updateSupplierValidator = new UpdateSupplierValidator(companyService);
            _companyService = companyService;
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListAll()
        {
            // var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            // var memberAndPermissionSettingList = _supplierService.GetAllSupplierByCompId(memberAndPermissionSetting.CompanyWithUnit.CompId).OrderByDescending(e=>e.CreatedAt).ToList();
            var data = _supplierService.GetAllSupplier().OrderByDescending(e => e.CreatedAt).ToList();

            var response = new CommonResponse<List<Supplier>>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("owner/list")]
        [Authorize]
        public IActionResult ListAllForOwner()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit == null || memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var data = _supplierService.GetAllSupplier().OrderByDescending(e => e.CreatedAt).ToList();
            var response = new CommonResponse<List<Supplier>>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {

            var supplier = _supplierService.GetSupplierById(id);

            var response = new CommonResponse<Supplier>()
            {
                Result = true,
                Message = "",
                Data = supplier
            };
            return Ok(response);
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateSupplier(CreateSupplierRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var validationResult = _createSupplierValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var newSupplier = _mapper.Map<Supplier>(request);
            _supplierService.AddSupplier(newSupplier);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult UpdateSupplier(UpdateSupplierRequest  request)
        {
            var validationResult = _updateSupplierValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var existingSupplier = _supplierService.GetSupplierById(request.Id);
            if (existingSupplier == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "供應商不存在"
                });
            }

            _supplierService.UpdateSupplier(request, existingSupplier);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult InActiveSupplier(int id)
        {

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            var existingSupplier = _supplierService.GetSupplierById(id);
            if (existingSupplier == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "供應商不存在"
                });
            }

            _supplierService.UpdateSupplier(new UpdateSupplierRequest()
            {
               Id=id,
                IsActive = false,
            }, existingSupplier);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }
    }
}
