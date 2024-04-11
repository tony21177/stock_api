using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using System.Runtime.CompilerServices;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly AuthLayerService _authLayerService;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly CompanyService _companyService;
        private readonly IValidator<CreateCompanyRequest> _createCompanyRequestValidator;
        private readonly IValidator<UpdateCompanyRequest> _updateCompanyRequestValidator;

        public CompanyController(AuthLayerService authLayerService, IMapper mapper, AuthHelpers authHelpers, CompanyService companyService)
        {
            _authLayerService = authLayerService;
            _mapper = mapper;
            _authHelpers = authHelpers;
            this._companyService = companyService;
            _createCompanyRequestValidator = new CreateCompanyValidator();
            _updateCompanyRequestValidator = new UpdateCompanyValidator();
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListAllCompany()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var unitId = memberAndPermissionSetting.CompanyWithUnit.UnitId;
            var data = _companyService.GetAllCompanyWithUnitByUnitId(unitId);
            var sortedData = data.OrderByDescending(dto => dto.CreatedAt).ToList();

            var response = new CommonResponse<List<CompanyWithUnitVo>>()
            {
                Result = true,
                Message = "",
                Data = sortedData
            };
            return Ok(response);
        }
        [HttpGet("owner/list")]
        [Authorize]
        public IActionResult ListAllCompanyForOwner()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            if (memberAndPermissionSetting.CompanyWithUnit==null||memberAndPermissionSetting.CompanyWithUnit.Type!=CommonConstants.CompanyType.OWNER) {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var data = _companyService.GetAllCompanyWithUnit();

            List<CompanyWithUnitVo> sortedData = data
            .OrderByDescending(item => item.Type == CommonConstants.CompanyType.OWNER)
            .ThenByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Type)
            .ToList();

            var response = new CommonResponse<List<CompanyWithUnitVo>>()
            {
                Result = true,
                Message = "",
                Data = sortedData
            };
            return Ok(response);
        }

        [HttpPost("create")]
        [AuthorizeRoles("1")]
        public IActionResult CreateCompany(CreateCompanyRequest request)
        {
            var validationResult = _createCompanyRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var unitId = memberAndPermissionSetting.CompanyWithUnit.UnitId;
            var unitName = memberAndPermissionSetting.CompanyWithUnit.UnitName;

            var newCompany = _mapper.Map<Company>(request);
            _companyService.AddCompany(newCompany,unitId, unitName);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [AuthorizeRoles("1")]
        public IActionResult UpdateCompany(UpdateCompanyRequest request)
        {
            var validationResult = _updateCompanyRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var unitId = memberAndPermissionSetting.CompanyWithUnit.UnitId;
            var unitName = memberAndPermissionSetting.CompanyWithUnit.UnitName;

            var existingCompany = _companyService.GetCompanyByCompId(request.CompId);
            if (existingCompany == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result=false,
                    Message = "組織不存在"
                });
            }

            _companyService.UpdateCompany(request, existingCompany);

            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpDelete("{compId}")]
        [AuthorizeRoles("1")]
        public IActionResult InActiveCompany(string compId)
        {

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);

            var existingCompany = _companyService.GetCompanyByCompId(compId);
            if (existingCompany == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "組織不存在"
                });
            }

            _companyService.UpdateCompany(new UpdateCompanyRequest()
            {
                CompId = compId,IsActive = false,
            }, existingCompany);

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
