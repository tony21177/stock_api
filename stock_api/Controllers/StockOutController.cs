using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common.Constant;
using stock_api.Common;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Service;
using stock_api.Utils;
using stock_api.Service.ValueObject;
using stock_api.Models;
using MySqlX.XDevAPI.Common;

namespace stock_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockOutController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly StockInService _stockInService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly StockOutService _stockOutService;
        private readonly IValidator<OutboundRequest> _outboundValidator;
        private readonly IValidator<OwnerOutboundRequest> _ownerOutboundValidator;
        private readonly IValidator<ListStockOutRecordsRequest> _listStockOutRecordsValidator;
        

        public StockOutController(IMapper mapper, AuthHelpers authHelpers, GroupService groupService, StockInService stockInService, WarehouseProductService warehouseProductService, PurchaseService purchaseService, StockOutService stockOutService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _warehouseProductService = warehouseProductService;
            _stockOutService = stockOutService;
            _outboundValidator = new OutboundValidator();
            _ownerOutboundValidator = new OwnerOutboundValidator();
            _listStockOutRecordsValidator = new ListStockOutRecordsValidator();
        }

        
        [HttpPost("outbound")]
        [Authorize]
        public IActionResult OutboundItems(OutboundRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            request.CompId = compId;
            var validationResult = _outboundValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            List<InStockItemRecord> inStockItemRecordsNotAllOut = _stockInService.GetInStockRecordsHistoryNotAllOut(request.ProductCode, request.LotNumber, request.LotNumberBatch, request.CompId);
            if (inStockItemRecordsNotAllOut.Count == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "未找到相對應尚未出庫的入庫紀錄"
                });
            }
            var firstInStockItemRecord = inStockItemRecordsNotAllOut.OrderBy(item=>item.CreatedAt).FirstOrDefault();
            var product = _warehouseProductService.GetProductByProductCodeAndCompId(request.ProductCode, request.CompId);
            if (product==null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "無對應的品項"
                });
            }
            var result = _stockOutService.OutStock(request, firstInStockItemRecord, product, memberAndPermissionSetting.Member);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }

        [HttpPost("owner/outbound")]
        [Authorize]
        public IActionResult OwnerOutboundItems(OwnerOutboundRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            request.CompId = compId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }


            var validationResult = _ownerOutboundValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            List<InStockItemRecord> inStockItemRecordsNotAllOut = _stockInService.GetInStockRecordsHistoryNotAllOut(request.ProductCode, request.LotNumber, request.LotNumberBatch, request.CompId);
            if (inStockItemRecordsNotAllOut.Count == 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "未找到相對應尚未出庫的入庫紀錄"
                });
            }
            var firstInStockItemRecord = inStockItemRecordsNotAllOut.OrderBy(item => item.CreatedAt).FirstOrDefault();
            var product = _warehouseProductService.GetProductByProductCodeAndCompId(request.ProductCode, request.CompId);
            if (product == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "無對應的品項"
                });
            }
            if (request.Type == CommonConstants.OutStockType.SHIFT_OUT && request.ToCompId == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "調撥出庫必須填toCompId"
                });
            }
            AcceptanceItem? toCompAcceptanceItem = null;
            if(request.Type == CommonConstants.OutStockType.SHIFT_OUT)
            {
                // 找到要調撥過去的單位還沒入庫的AcceptItem
                toCompAcceptanceItem = _stockInService.GetAcceptanceItemNotInStockByProductIdAndCompId(firstInStockItemRecord.ProductId, request.ToCompId).FirstOrDefault();
                if (toCompAcceptanceItem == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "要調撥過去的單位沒有相符的待驗收採購品項"
                    });
                }
            }

            var result = _stockOutService.OwnerOutStock(request, firstInStockItemRecord, product, memberAndPermissionSetting.Member,toCompAcceptanceItem);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }

       

        [HttpPost("records/list")]
        [Authorize]
        public IActionResult ListStockOutRecords(ListStockOutRecordsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            request.CompId = compId;
            var validationResult = _listStockOutRecordsValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            request.CompId = compId;
            var (data,totalPages) = _stockOutService.ListStockOutRecords(request);
            var distinctProductIds = data.Select(x => x.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIds, compId);

            var outStockRecordVoList = _mapper.Map<List<OutStockRecordVo>>(data);
            foreach (var item in outStockRecordVoList)
            {
                var matchedProdcut = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProdcut?.Unit;
            }


            return Ok(new CommonResponse<List<OutStockRecordVo>>
            {
                Result = true,
                Data = outStockRecordVoList,
                TotalPages = totalPages
            });
        }

        [HttpPost("owner/records/list")]
        [Authorize]
        public IActionResult ListStockOutRecordsForOwner(ListStockOutRecordsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            request.CompId=compId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var validationResult = _listStockOutRecordsValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            if (request.ToCompId != null)
            {
                request.CompId = request.ToCompId;
            }

            var (data, totalPages) = _stockOutService.ListStockOutRecords(request);

            var distinctProductIds = data.Select(x => x.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIds, request.CompId);

            var outStockRecordVoList = _mapper.Map<List<OutStockRecordVo>>(data);
            foreach (var item in outStockRecordVoList)
            {
                var matchedProdcut = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProdcut?.Unit;
            }


            return Ok(new CommonResponse<List<OutStockRecordVo>>
            {
                Result = true,
                Data = outStockRecordVoList,
                TotalPages = totalPages
            });
        }
    }
}
