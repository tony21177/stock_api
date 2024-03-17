using AutoMapper;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service;
using stock_api.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthlayerController : Controller
    {
        private readonly AuthLayerService _authLayerService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthlayerController> _logger;
        private readonly AuthHelpers _jwtHelpers;
        public AuthlayerController(AuthLayerService authLayerService, IMapper mapper, ILogger<AuthlayerController> logger, AuthHelpers jwtHelpers)
        {
            _authLayerService = authLayerService;
            _mapper = mapper;
            _logger = logger;
            _jwtHelpers = jwtHelpers;
        }

        [HttpGet("list/{compId}")]
        //[Authorize(Roles = "1")]
        [AuthorizeRoles("1")]
        public IActionResult List(string compId)
        {
            // 以下這段不需要===>[Authorize(Roles = "1")]搭配CustomAuthorizationMiddlewareResultHandler即可

            //var authValueAndPermissionSetting = _jwtHelpers.GetMemberAndPermissionSetting(User);
            //if (authValueAndPermissionSetting == null || authValueAndPermissionSetting.Member.AuthValue != (short)1)
            //{
            //    return Unauthorized(new CommonResponse<List<WarehouseAuthlayer>>()
            //    {
            //        Result = false,
            //        Message = "沒有權限",
            //        Data = null
            //    });
            //}

            var data = _authLayerService.GetAllAuthlayers(compId);
            var response = new CommonResponse<List<WarehouseAuthlayer>>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return Ok(response);
        }

        [HttpPost("update")]
        [AuthorizeRoles("1")]
        public CommonResponse<List<WarehouseAuthlayer>> Update(List<UpdateAuthlayerRequest> updateAuthlayerListRequest)
        {
            var authlayerList = _mapper.Map<List<WarehouseAuthlayer>>(updateAuthlayerListRequest);
            var data = _authLayerService.UpdateAuthlayers(authlayerList);

            var response = new CommonResponse<List<WarehouseAuthlayer>>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return response;
        }

        [HttpPost("create")]
        [AuthorizeRoles("1")]
        public CommonResponse<WarehouseAuthlayer> Create(CreateAuthlayerRequest createAuthlayerRequest)
        {
            var newAuthLayer = _mapper.Map<WarehouseAuthlayer>(createAuthlayerRequest);
            // TODO
            var data = _authLayerService.AddAuthlayer(newAuthLayer);

            var response = new CommonResponse<WarehouseAuthlayer>()
            {
                Result = true,
                Message = "",
                Data = data
            };
            return response;
        }

        [HttpDelete("delete/{id}")]
        [AuthorizeRoles("1")]
        public IActionResult Delete(int id)
        {

            _authLayerService.DeleteAuthLayer(id);

            var response = new CommonResponse<WarehouseAuthlayer>()
            {
                Result = true,
                Message = "",
                Data = null
            };
            return Ok(response);
        }

    }
}

