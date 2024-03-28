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
    public class ManufacturerController : ControllerBase
    {
        private readonly AuthLayerService _authLayerService;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly ManufacturerService _manufacturerService;
        private readonly IValidator<CreateManufacturerRequest> _createManufacturerValidator;
        private readonly IValidator<UpdateManufacturerRequest> _updateManufacturerValidator;

        public ManufacturerController(AuthLayerService authLayerService, IMapper mapper, AuthHelpers authHelpers, ManufacturerService manufacturerService)
        {
            _authLayerService = authLayerService;
            _mapper = mapper;
            _authHelpers = authHelpers;
            _manufacturerService = manufacturerService;
            _createManufacturerValidator = new CreateManufacturerValidator();
            _updateManufacturerValidator= new UpdateManufacturerValidator();
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListAll()
        {
            var memberAndPermissionSettingList = _manufacturerService.GetAllManufacturer();

            var response = new CommonResponse<List<Manufacturer>>()
            {
                Result = true,
                Message = "",
                Data = memberAndPermissionSettingList
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(string id)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(id);

            var response = new CommonResponse<Manufacturer>()
            {
                Result = true,
                Message = "",
                Data = manufacturer
            };
            return Ok(response);
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateManufacturer(CreateManufacturerRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.Owner)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var newManufacturer = _mapper.Map<Manufacturer>(request);
            _manufacturerService.AddManufacturer(newManufacturer);

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
        public IActionResult UpdateCompany(UpdateManufacturerRequest  request)
        {
            var validationResult = _updateManufacturerValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.Owner)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse);
            }

            var existingManufacturer = _manufacturerService.GetManufacturerById(request.Id);
            if (existingManufacturer == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "製造商不存在"
                });
            }

            _manufacturerService.UpdateManufacturer(request, existingManufacturer);

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
        public IActionResult InActiveManufacturer(string id)
        {

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.Owner)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse);
            }
            var existingManufacturer = _manufacturerService.GetManufacturerById(id);
            if (existingManufacturer == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "製造商不存在"
                });
            }

            _manufacturerService.UpdateManufacturer(new UpdateManufacturerRequest()
            {
               Id=id,
                IsActive = false,
            }, existingManufacturer);

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
