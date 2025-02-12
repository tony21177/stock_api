using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Service;
using stock_api.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstrumentController : ControllerBase
    {
        private readonly AuthHelpers _authHelpers;
        private readonly ILogger<InstrumentController> _logger;
        private readonly InstrumentService _instrumentService;

        public InstrumentController(AuthHelpers authHelpers, ILogger<InstrumentController> logger, InstrumentService instrumentService)
        {
            _authHelpers = authHelpers;
            _logger = logger;
            _instrumentService = instrumentService;
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateInstrument(CreateInstrumentRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }
            _instrumentService.AddInstrument(request,memberAndPermissionSetting.Member);
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
        public IActionResult UpdateInstrument(UpdateInstrumentRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var existingInstrument = _instrumentService.GetById(request.InstrumentId);
            if (existingInstrument == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "儀器不存在"
                });
            }
            
            _instrumentService.UpdateInstrument(request,existingInstrument);
            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

        [HttpPost("list")]
        [Authorize]
        public IActionResult ListInstruments(ListInstrumentRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }


            var (instruments,totalPages) = _instrumentService.ListInstrument(request,null);
            var response = new CommonResponse<dynamic>()
            {
                Result = true,
                Message = "",
                Data = instruments,
                TotalPages = totalPages
            };
            return Ok(response);
        }

    }
}
