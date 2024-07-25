using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common;
using stock_api.Common.Constant;
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
    public class SupplierTraceController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly SupplierService _supplierService;
        private readonly SupplierTraceService _supplierTraceService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly StockInService _stockInService;
        private readonly IValidator<ManualCreateSupplierTraceLogRequest> _manualCreateSupplierTraceLogValidator;
        private readonly IValidator<ManualUpdateSupplierTraceLogRequest> _manualUpdateSupplierTraceLogValidator;
        private readonly IValidator<ListSupplierTraceLogRequest> _listSupplierTraceLogValidator;
        private readonly IValidator<ReportListSupplierTraceLogRequest> _reportListSupplierTraceLogValidator;

        public SupplierTraceController(IMapper mapper, AuthHelpers authHelpers, SupplierService supplierService, SupplierTraceService supplierTraceService,WarehouseProductService warehouseProductService,StockInService stockInService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _supplierService = supplierService;
            _supplierTraceService = supplierTraceService;
            _warehouseProductService = warehouseProductService;
            _manualCreateSupplierTraceLogValidator = new ManualCreateTraceLogValidator(supplierService);
            _manualUpdateSupplierTraceLogValidator = new ManualUpdateTraceLogValidator(supplierService);
            _listSupplierTraceLogValidator = new ListSupplierTraceLogValidator(supplierService);
            _reportListSupplierTraceLogValidator = new ReportListSupplierTraceLogValidator(supplierService);
            _stockInService = stockInService;
        }

        [HttpPost("manualCreate")]
        [Authorize]
        public IActionResult ManualCreate(ManualCreateSupplierTraceLogRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;

            var validationResult = _manualCreateSupplierTraceLogValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var newSupplierTraceLog = _mapper.Map<SupplierTraceLog>(request);
            if (request.ProductId != null)
            {
                var product = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId, compId);
                if (product == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "該品項不存在"
                    });
                }
                newSupplierTraceLog.ProductId = product.ProductId;
                newSupplierTraceLog.ProductName = product.ProductName;
            }

            var supplier = _supplierService.GetSupplierById(request.SupplierId);
            newSupplierTraceLog.SourceId = request.SourceId;
            newSupplierTraceLog.SupplierName = supplier.Name;
            newSupplierTraceLog.UserId = memberAndPermissionSetting.Member.UserId;
            newSupplierTraceLog.UserName = memberAndPermissionSetting.Member.DisplayName;
            newSupplierTraceLog.SourceType = request.SourceType;
            _supplierTraceService.CreateSupplierTrace(newSupplierTraceLog);
            return Ok(new CommonResponse<dynamic>
            {
                Result = true
            });
        }

        [HttpPost("update")]
        [Authorize]
        public IActionResult Update(ManualUpdateSupplierTraceLogRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;

            var validationResult = _manualUpdateSupplierTraceLogValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var existingLog = _supplierTraceService.GetById(request.Id);
            if (existingLog==null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "不存在"
                });
            }

            var updateSupplierTraceLog = _mapper.Map<SupplierTraceLog>(request);
            if (request.ProductId != null)
            {
                var product = _warehouseProductService.GetProductByProductIdAndCompId(request.ProductId, compId);
                if (product == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "該品項不存在"
                    });
                }
                updateSupplierTraceLog.ProductId = product.ProductId;
                updateSupplierTraceLog.ProductName = product.ProductName;
            }


            if (request.SupplierId != null)
            {
                var supplier = _supplierService.GetSupplierById(request.SupplierId.Value);
                updateSupplierTraceLog.SupplierId = request.SupplierId.Value;
                updateSupplierTraceLog.SupplierName = supplier.Name;
            }
            updateSupplierTraceLog.UserId = memberAndPermissionSetting.Member.UserId;
            updateSupplierTraceLog.UserName = memberAndPermissionSetting.Member.DisplayName;
            updateSupplierTraceLog.CompId = compId;


            _supplierTraceService.UpdateSupplierTrace(updateSupplierTraceLog, existingLog);
            return Ok(new CommonResponse<dynamic>
            {
                Result = true
            });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var existingLog = _supplierTraceService.GetById(id);
            if (existingLog == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "不存在"
                });
            }
            if (existingLog.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            _supplierTraceService.DeleteSupplierTrace(id);
            return Ok(new CommonResponse<dynamic>
            {
                Result = true
            });
        }


        [HttpPost("list")]
        [Authorize]
        public IActionResult List(ListSupplierTraceLogRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;

            var validationResult = _listSupplierTraceLogValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

           

            var (data,totalPages) = _supplierTraceService.ListSupplierTraceLog(request);
            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = data,
                TotalPages = totalPages
            });
        }

        [HttpPost("report/abnormal/list")]
        [Authorize]
        public IActionResult ReportList(ReportListSupplierTraceLogRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;

            var validationResult = _reportListSupplierTraceLogValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }



            var (supplierTraceLogList, totalPages) = _supplierTraceService.ReportListSupplierTraceLog(request);
            List < SupplierTraceLogWithInStock > supplierTraceLogWithInStockList= _mapper.Map<List<SupplierTraceLogWithInStock>>(supplierTraceLogList);


            List<string> inStockIdList = supplierTraceLogList.Where(l=>l.SourceType==CommonConstants.SourceType.IN_STOCK&&l.SourceId!=null).Select(l=>l.SourceId).Distinct().ToList();
            if (inStockIdList != null && inStockIdList.Count > 0)
            {
                var acceptanceItems = _stockInService.GetAcceptanceItemsByInIdList(inStockIdList);
                var distinctItemIdLIst = acceptanceItems.Select(i=>i.ItemId).Distinct().ToList();
                var inStockedItems = _stockInService.GetInStockRecordsByItemIdList(distinctItemIdLIst).OrderBy(i=>i.CreatedAt).ToList();

                foreach (var log in supplierTraceLogWithInStockList)
                {
                    if (log.SourceType == CommonConstants.SourceType.IN_STOCK && log.SourceId != null)
                    {
                        var matchedAcceptanceItem = acceptanceItems.Where(i=>i.AcceptId==log.SourceId).FirstOrDefault();
                        if (matchedAcceptanceItem!=null)
                        {
                            var matchedInStockItemList = inStockedItems.Where(i=>i.ItemId==matchedAcceptanceItem.ItemId).ToList();
                            log.InStockItems = matchedInStockItemList;
                        }
                    }
                }

            }

            return Ok(new CommonResponse<List<SupplierTraceLogWithInStock>>
            {
                Result = true,
                Data = supplierTraceLogWithInStockList,
                TotalPages = totalPages
            });
        }

        [HttpPost("report/inStockListWithAbnormal")]
        [Authorize]
        public IActionResult InStockReportListWithAbnormal(ReportListSupplierTraceLogRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;

            var validationResult = _reportListSupplierTraceLogValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            ListStockInRecordsRequest listRequest = new ListStockInRecordsRequest { 
                CompId = request.CompId,
                SupplierId = request.SupplierId,
                StartDate =request.StartDate,
                EndDate =request.EndDate,
                PaginationCondition = request.PaginationCondition,
            };
            var (inStockItemList, totalPages) = _stockInService.ListStockInRecords(listRequest);
            List<InStockItemListWithAbnormal> inStockItemListWithAbnormal = _mapper.Map<List<InStockItemListWithAbnormal>>(inStockItemList);

            List<string> itemIdList = inStockItemList.Select(i=>i.ItemId).Distinct().ToList();
            var acceptanceItems = _stockInService.GetAcceptanceItemsByItemIdList(itemIdList);
            List<string> acceptIdList = acceptanceItems.Select(a=>a.AcceptId).ToList();
            List<SupplierTraceLog> supplierTraceLogs = _supplierTraceService.GetInStockAbnormalList(acceptIdList);

            foreach (var inStockItemWithAbnormal in inStockItemListWithAbnormal)
            {
                if(inStockItemWithAbnormal.InStockId == "a1a7b3e4-3e11-4d49-8a9c-ac5dedeea24f")
                {
                    var matchedAcceptItem2 = acceptanceItems.Where(a => a.ItemId == inStockItemWithAbnormal.ItemId).FirstOrDefault();
                    var matchedSupplierTraceLot2 = supplierTraceLogs.Where(s => s.SourceType == CommonConstants.SourceType.IN_STOCK && s.SourceId != null && s.SourceId == matchedAcceptItem2.AcceptId).FirstOrDefault();
                    inStockItemWithAbnormal.SupplierTraceLog = matchedSupplierTraceLot2;
                    continue;
                }
                var matchedAcceptItem = acceptanceItems.Where(a => a.ItemId == inStockItemWithAbnormal.ItemId).FirstOrDefault();
                var matchedSupplierTraceLot = supplierTraceLogs.Where(s => s.SourceType == CommonConstants.SourceType.IN_STOCK && s.SourceId != null && s.SourceId == matchedAcceptItem.AcceptId).FirstOrDefault();
                inStockItemWithAbnormal.SupplierTraceLog = matchedSupplierTraceLot;
            }

            if (request.IsAbnormal == true)
            {
                inStockItemListWithAbnormal = inStockItemListWithAbnormal.Where(i=>i.SupplierTraceLog!=null).ToList();
            }
            if (request.IsAbnormal == false)
            {
                inStockItemListWithAbnormal = inStockItemListWithAbnormal.Where(i => i.SupplierTraceLog == null).ToList();
            }

            return Ok(new CommonResponse<List<InStockItemListWithAbnormal>>
            {
                Result = true,
                Data = inStockItemListWithAbnormal,
                TotalPages = totalPages
            });
        }
    }
}
