﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog;
using stock_api.Common;
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
    public class QcController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly StockInService _stockInService;
        private readonly StockOutService _stockOutService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly PurchaseService _purchaseService;
        private readonly QcService _qcService;
        private readonly IValidator<CreateQcRequest> _createQcValidator;
        private readonly IValidator<ListMainWithDetailRequest> _listQcMainWithDetailValidator;

        public QcController(IMapper mapper, AuthHelpers authHelpers, StockInService stockInService, StockOutService stockOutService, WarehouseProductService warehouseProductService, QcService qcService, PurchaseService purchaseService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _stockOutService = stockOutService;
            _warehouseProductService = warehouseProductService;
            _qcService = qcService;
            _purchaseService = purchaseService;
            _createQcValidator = new CreateQcValidator();
            _listQcMainWithDetailValidator = new ListQcMainWithDetailValidator();
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListUnDoneQcLot()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            List<UnDoneQcLot> unDoneQcList = _qcService.ListUnDoneQcLotList(compId);
            List<string> distinctLotNumberBatchList = unDoneQcList.Where(e=>e.LotNumberBatch!=null).Select(e => e.LotNumberBatch).Distinct().ToList();
            List<string> distincLotNumberList = unDoneQcList.Where(e => e.LotNumber != null).Select(e => e.LotNumber).Distinct().ToList();
            List<InStockItemRecord> inStockItems = _stockInService.GetInStockRecordByLotNumberBatchList(distinctLotNumberBatchList, compId);
            List<OutStockRecord> outStockRecordsByLotNumber = _stockOutService.GetOutStockRecordsByLotNumberList(distincLotNumberList);
            List<OutStockRecord> outStockRecordsByLotNumberBatch = _stockOutService.GetOutStockRecordsByLotNumberBatchList(distinctLotNumberBatchList);

            Dictionary<string, string> lotNumberBatchAndItemIdMap = new Dictionary<string, string>();
            inStockItems.ForEach(i =>
            {
                lotNumberBatchAndItemIdMap.Add(i.LotNumberBatch, i.ItemId);
            });

            var purchaseDetailList = _purchaseService.GetPurchaseDetailListByItemIdList(inStockItems.Select(i => i.ItemId).ToList());
            Dictionary<String, PurchaseDetailView> itemIdAndPurchaseDetailMap = new Dictionary<String, PurchaseDetailView>();
            purchaseDetailList.ForEach(d =>
            {
                itemIdAndPurchaseDetailMap.Add(d.ItemId, d);
            });



            unDoneQcList.ForEach(lot =>
            {
                var matchedInStock = inStockItems.Where(i => i.LotNumberBatch == lot.LotNumberBatch).FirstOrDefault();
                var matchedItemId = lotNumberBatchAndItemIdMap[lot.LotNumberBatch];
                var matchedPurchaseDetail = itemIdAndPurchaseDetailMap[matchedItemId];
                //        public string PurchaseMainId { get; set; } = null!;
                //public DateTime ApplyDate { get; set; }
                //public String InStockId { get; set; } = null!;
                //public DateTime AcceptedAt { get; set; }
                //public string AcceptUserName { get; set; }
                //public string AcceptUserId { get; set; }

                //public string ProductSpec { get; set; } = null!;
                lot.PurchaseMainId = matchedPurchaseDetail.PurchaseMainId;
                lot.ApplyDate = matchedPurchaseDetail.ApplyDate;
                lot.InStockId = matchedInStock.InStockId;
                lot.AcceptedAt = matchedInStock.CreatedAt.Value;
                lot.AcceptUserId = matchedInStock.UserId;
                lot.AcceptUserName = matchedInStock.UserName;
                lot.ProductSpec = matchedPurchaseDetail.ProductSpec;
                if (outStockRecordsByLotNumber.Where(i => i.LotNumber == lot.LotNumber).FirstOrDefault() != null)
                {
                    lot.IsLotNumberOutStock = true;
                }
                if (outStockRecordsByLotNumberBatch.Where(i => i.LotNumberBatch == lot.LotNumberBatch).FirstOrDefault() != null)
                {
                    lot.IsLotNumberBatchOutStock = true;
                }
            });


            var response = new CommonResponse<List<UnDoneQcLot>>
            {
                Result = true,
                Data = unDoneQcList
            };
            return Ok(response);
        }

        [HttpPost("qcValidation")]
        [Authorize]
        public IActionResult CreateQcValidation(CreateQcRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            request.CompId = compId;

            var validationResult = _createQcValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            QcValidationMain newQcMain = _mapper.Map<QcValidationMain>(request);
            newQcMain.MainId = Guid.NewGuid().ToString();
            List<QcValidationDetail> newQcDetailList = _mapper.Map<List<QcValidationDetail>>(request.Details);
            List<InStockItemRecord> inStockItemRecordList = new List<InStockItemRecord>();
            if (!string.IsNullOrEmpty(request.LotNumber))
            {
                inStockItemRecordList= _stockInService.GetInStockRecordListByLotNumber(request.LotNumber, compId).OrderByDescending(i => i.CreatedAt).ToList();
                if (inStockItemRecordList.Count == 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "此批號沒有對應的入庫資料"
                    });
                }
            }
            if (!string.IsNullOrEmpty(request.LotNumberBatch))
            {
                var inStockItemRecord = _stockInService.GetInStockRecordByLotNumberBatch(request.LotNumberBatch, compId);
                if (inStockItemRecord==null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "此批號沒有對應的入庫資料"
                    });
                }
                inStockItemRecordList = new List<InStockItemRecord> { inStockItemRecord };
            }
            
            List<string> itemIdList = inStockItemRecordList.Select(i => i.ItemId).Distinct().ToList();
            List<PurchaseDetailView> purchaseDetailList = _purchaseService.GetPurchaseDetailListByItemIdList(itemIdList);

            newQcMain.PurchaseMainId = purchaseDetailList.Count > 0 ? purchaseDetailList[0].PurchaseMainId : null;
            newQcMain.PurchaseSubItemId = purchaseDetailList.Count > 0 ? string.Join(",", purchaseDetailList.Select(e => e.ItemId).ToList()) : null;
            newQcMain.InStockId = inStockItemRecordList[0].InStockId;
            newQcMain.InStockTime = inStockItemRecordList[0].CreatedAt.Value;
            newQcMain.InStockUserId = inStockItemRecordList[0].UserId;
            newQcMain.InStockUserName = inStockItemRecordList[0].UserName;
            newQcMain.ProductId = inStockItemRecordList[0].ProductId;
            newQcMain.ProductCode = inStockItemRecordList[0].ProductCode;
            newQcMain.ProductName = inStockItemRecordList[0].ProductName;
            newQcMain.ProductSpec = inStockItemRecordList[0].ProductSpec;
            newQcMain.LotNumber = inStockItemRecordList[0].LotNumber;
            newQcMain.LotNumberBatch = inStockItemRecordList[0].LotNumberBatch;
            newQcDetailList.ForEach(detail => detail.MainId = newQcMain.MainId);
            var (result,erroMsg) = _qcService.CreateQcValidation(newQcMain, newQcDetailList);
            var response = new CommonResponse<List<UnDoneQcLot>>
            {
                Result = result,
                Message = erroMsg
            };
            return Ok(response);
        }


        [HttpPost("mainWithDetail/list")]
        [Authorize]
        public IActionResult ListMainWithDetail([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ListMainWithDetailRequest? request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request == null)
            {
                request = new ListMainWithDetailRequest();
            }
            request.CompId = compId;

            var validationResult = _listQcMainWithDetailValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var (qcMainList,totalPages) = _qcService.ListQcMain(request);
            List<String> distinctLotNumberList = qcMainList.Where(qc=>qc.LotNumber!=null).Select(qc=>qc.LotNumber).Distinct().ToList();
            List<String> distinctLotNumberBatchList = qcMainList.Where(qc => qc.LotNumberBatch != null).Select(qc => qc.LotNumberBatch).Distinct().ToList();
            List<OutStockRecord> outStockRecordsByLotNumber = _stockOutService.GetOutStockRecordsByLotNumberList(distinctLotNumberList);
            List<OutStockRecord> outStockRecordsByLotNumberBatch = _stockOutService.GetOutStockRecordsByLotNumberBatchList(distinctLotNumberBatchList);


            var details = _qcService.GetQcDetailsByMainIdList(qcMainList.Select(m => m.MainId).ToList());

            var qcMainWithDetailList = _mapper.Map<List<QcMainWithDetail>>(qcMainList);
            qcMainWithDetailList.ForEach(m =>
            {
                var matchedDetailList = details.Where(d=>d.MainId==m.MainId).ToList();
                m.DetailList = matchedDetailList;
                if (outStockRecordsByLotNumber.Where(i => i.LotNumber == m.LotNumber).FirstOrDefault() != null)
                {
                    m.IsLotNumberOutStock = true;
                }
                if (outStockRecordsByLotNumberBatch.Where(i => i.LotNumberBatch == m.LotNumberBatch).FirstOrDefault() != null)
                {
                    m.IsLotNumberBatchOutStock = true;
                }

            });

            
            var response = new CommonResponse<List<QcMainWithDetail>>
            {
                Result = true,
                Data = qcMainWithDetailList,
                TotalPages = totalPages
            };
            return Ok(response);
        }
    }
}
