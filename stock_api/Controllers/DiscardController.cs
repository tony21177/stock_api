using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Service;
using stock_api.Utils;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscardController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly StockOutService _stockOutService;
        private readonly MemberService _memberService;
        private readonly DiscardService _discardService;
        private readonly IValidator<ListDiscardRecordsRequest> _listDiscardRecordsRequestValidator;

        public DiscardController(IMapper mapper, AuthHelpers authHelpers, WarehouseProductService warehouseProductService,
            StockOutService stockOutService, MemberService memberService, DiscardService discardService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _warehouseProductService = warehouseProductService;
            _stockOutService = stockOutService;
            _memberService = memberService;
            _discardService = discardService;
            _listDiscardRecordsRequestValidator = new ListDiscardRecordsValidator();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Discard(DiscardRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var outStockRecord = _stockOutService.GetOutStockRecordById(request.OutStockId);
            if (outStockRecord == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "未找到出庫紀錄"
                });
            }
            if (request.ApplyQuantity > (outStockRecord.ApplyQuantity-(outStockRecord.DiscardQuantity??0.0)))
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"超過可丟棄數量{outStockRecord.ApplyQuantity - (outStockRecord.DiscardQuantity??0.0)}"
                });
            }
            if (compId != outStockRecord.CompId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var product = _warehouseProductService.GetProductByProductId(outStockRecord.ProductId);
            if (product == null||product.IsAllowDiscard==false)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該品項不可丟棄"
                });
            }

            var (result,msg) = _discardService.Discard(outStockRecord, request.ApplyQuantity, memberAndPermissionSetting.Member);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = msg
            });

        }

        [HttpPost("search")]
        [Authorize]
        public IActionResult ListDiscardRecords(ListDiscardRecordsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }

            var validationResult = _listDiscardRecordsRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var (data,totalPages) = _discardService.ListDiscardRecords(request);
            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = data,
                TotalPages = totalPages
            });
        }
    }
}
