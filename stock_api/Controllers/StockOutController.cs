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
using MySqlX.XDevAPI.CRUD;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using stock_api.Controllers.Dto;
using stock_api.Common.Utils;

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
        private readonly PurchaseService _purchaseService;
        private readonly IValidator<OutboundRequest> _outboundValidator;
        private readonly IValidator<BatchOutboundRequest> _batchOutboundValidator;
        private readonly IValidator<OwnerOutboundRequest> _ownerOutboundValidator;
        private readonly IValidator<BatchOwnerOutboundRequest> _batchOwnerOutboundValidator;
        private readonly IValidator<ListStockOutRecordsRequest> _listStockOutRecordsValidator;
        private readonly EmailService _emailService;
        private readonly MemberService _memberService;
        private readonly DiscardService _discardService;
        private readonly InstrumentService _instrumentService;


        public StockOutController(IMapper mapper, AuthHelpers authHelpers, GroupService groupService, StockInService stockInService,
            WarehouseProductService warehouseProductService, PurchaseService purchaseService,
            StockOutService stockOutService,EmailService emailService, MemberService memberService,DiscardService discardService,
            InstrumentService instrumentService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _warehouseProductService = warehouseProductService;
            _stockOutService = stockOutService;
            _purchaseService = purchaseService;
            _outboundValidator = new OutboundValidator();
            _batchOutboundValidator = new BatchOutboundValidator();
            _ownerOutboundValidator = new OwnerOutboundValidator();
            _batchOwnerOutboundValidator = new BatchOwnerOutboundValidator();
            _listStockOutRecordsValidator = new ListStockOutRecordsValidator();
            _emailService = emailService;
            _memberService = memberService;
            _discardService = discardService;
            _instrumentService = instrumentService;
        }


        [HttpPost("outbound")]
        [Authorize]
        public IActionResult OutboundItems(OutboundRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            var validationResult = _outboundValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            if (request.Type == CommonConstants.OutStockType.PURCHASE_OUT)
            {
                var inStockRecord = _stockInService.GetInStockRecordByLotNumberBatch(request.LotNumberBatch, compId);
                if (inStockRecord == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "未找到該品項尚未出庫的入庫紀錄"
                    });
                }
                var productCode = inStockRecord.ProductCode;
                request.ProductCode = productCode;

                List<InStockItemRecord> inStockItemRecordsNotAllOutExDateFIFO = _stockInService.GetProductInStockRecordsHistoryNotAllOutExpirationFIFO(productCode, compId);

                if (inStockItemRecordsNotAllOutExDateFIFO.Count == 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "未找到該品項尚未出庫的入庫紀錄"
                    });
                }
                InStockItemRecord? requestLot = null;
                if (inStockItemRecordsNotAllOutExDateFIFO.Count > 0)
                {
                    requestLot = inStockItemRecordsNotAllOutExDateFIFO.FirstOrDefault(item => item.LotNumberBatch == request.LotNumberBatch);
                    if (requestLot == null)
                    {
                        return BadRequest(new CommonResponse<dynamic>
                        {
                            Result = false,
                            Message = "未找到相對應尚未出庫的入庫紀錄"
                        });
                    }
                    var oldestLot = inStockItemRecordsNotAllOutExDateFIFO.FirstOrDefault();

                    // 表示要出的批號不是最早的那批 IsAbnormal!=true(非user確認過的)
                    if ((requestLot.LotNumberBatch != oldestLot.LotNumberBatch) && request.IsAbnormal != true)
                    {
                        var IsNeedQc2 = requestLot.IsNeedQc == true && requestLot.QcTestStatus == CommonConstants.QcTestStatus.NONE && requestLot.QcType != CommonConstants.QcTypeConstants.NONE;
                        NeedQc? needQc2 = null;
                        if (IsNeedQc2)
                        {
                            var purchaseMain = _stockInService.GetPurchaseMainByInStockId(requestLot);

                            needQc2 = new NeedQc()
                            {
                                PurchaseMainID = purchaseMain.PurchaseMainId,
                                LotNumber = requestLot.LotNumber,
                                LotNumberBatch = requestLot.LotNumberBatch,
                                QcType = requestLot.QcType,
                                ProductID = requestLot.ProductId,
                                ProductName = requestLot.ProductName,
                                ApplyDate = purchaseMain.ApplyDate,
                                AcceptedAt = requestLot.CreatedAt.Value,
                                AcceptUserId = requestLot.UserId,
                                AcceptUserName = requestLot.UserName,
                            };
                        }
                        var warehouseProduct = _warehouseProductService.GetProductByProductId(requestLot.ProductId);
                        List<string> printStickerLotBatchListForBadRequest = new();
                        List<string> isNewLotNumberListForBadRequest = new();
                        List<string> isNewLotNumberBatchListForBadRequest = new();
                        if (warehouseProduct.IsPrintSticker == true)
                        {
                            printStickerLotBatchListForBadRequest.Add(requestLot.LotNumberBatch);
                        }
                        if (requestLot.LotNumber != warehouseProduct.LotNumber)
                        {
                            isNewLotNumberListForBadRequest.Add(requestLot.LotNumber);
                        }
                        if (requestLot.LotNumberBatch != warehouseProduct.LotNumberBatch)
                        {
                            isNewLotNumberBatchListForBadRequest.Add(requestLot.LotNumberBatch);
                        }

                        return BadRequest(new CommonResponse<Dictionary<string, dynamic>>
                        {
                            Result = false,
                            Message = "還有效期更早的批號還沒出",
                            Data = new Dictionary<string, dynamic>
                            {
                                ["isFIFO"] = false,
                                ["oldest"] = oldestLot,
                                ["requestLotNumberBatch"] = requestLot.LotNumberBatch,
                                ["needQc"] = needQc2,
                                ["printStickerLotBatchList"] = printStickerLotBatchListForBadRequest,
                                ["isNewLotNumberList"] = isNewLotNumberListForBadRequest,
                                ["isNewLotNumberBatchList"] = isNewLotNumberBatchListForBadRequest
                            }
                        });
                    }
                }

                var product = _warehouseProductService.GetProductByProductCodeAndCompId(productCode, compId);
                if (product == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "無對應的庫存品項"
                    });
                }
                if (request.ApplyQuantity > requestLot.InStockQuantity)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "出庫數量超過入庫數量"
                    });
                }
                List<string> printStickerLotBatchList = new();
                List<string> isNewLotNumberList = new();
                List<string> isNewLotNumberBatchList = new();
                if (product.IsPrintSticker == true)
                {
                    printStickerLotBatchList.Add(requestLot.LotNumberBatch);
                }
                if (requestLot.LotNumber != product.LotNumber)
                {
                    isNewLotNumberList.Add(requestLot.LotNumber);
                }
                if (requestLot.LotNumberBatch != product.LotNumberBatch)
                {
                    isNewLotNumberBatchList.Add(requestLot.LotNumberBatch);
                }
                var IsNeedQc = requestLot.IsNeedQc == true && requestLot.QcTestStatus == CommonConstants.QcTestStatus.NONE && requestLot.QcType != CommonConstants.QcTypeConstants.NONE;
                NeedQc? needQc = null;
                if (IsNeedQc)
                {
                    var purchaseMain = _stockInService.GetPurchaseMainByInStockId(requestLot);
                    needQc = new NeedQc()
                    {
                        PurchaseMainID = purchaseMain.PurchaseMainId,
                        LotNumber = requestLot.LotNumber,
                        LotNumberBatch = requestLot.LotNumberBatch,
                        QcType = requestLot.QcType,
                        ProductID = requestLot.ProductId,
                        ProductName = requestLot.ProductName,
                        ApplyDate = purchaseMain.ApplyDate,
                        AcceptedAt = requestLot.CreatedAt.Value,
                        AcceptUserId = requestLot.UserId,
                        AcceptUserName = requestLot.UserName,
                    };
                    if (request.IsSkipQc == false)
                    {
                        return BadRequest(new CommonResponse<Dictionary<string, dynamic>>
                        {
                            Result = false,
                            Message = "請確認是否要跳過品質確效出庫",
                            Data = new Dictionary<string, dynamic>
                            {
                                ["needQc"] = needQc,
                                ["printStickerLotBatchList"] = printStickerLotBatchList,
                                ["isNewLotNumberList"] = isNewLotNumberList,
                                ["isNewLotNumberBatchList"] = isNewLotNumberBatchList,
                                ["isNewLotNumberList"] = isNewLotNumberList,
                                ["isNewLotNumberBatchList"] = isNewLotNumberBatchList
                            }
                        });
                    }
                }


                var (result, errorMsg, notifyProductQuantity) = _stockOutService.OutStock(request.Type, request, requestLot, product, memberAndPermissionSetting.Member, compId);


                //CalculateForQuantityToNotity(new List<NotifyProductQuantity> { notifyProductQuantity });
                return Ok(new CommonResponse<dynamic>
                {
                    Result = result,
                    Message = errorMsg,
                    Data = new Dictionary<string, dynamic>
                    {
                        ["needQc"] = needQc,
                        ["printStickerLotBatchList"] = printStickerLotBatchList,
                        ["isNewLotNumberList"] = isNewLotNumberList,
                        ["isNewLotNumberBatchList"] = isNewLotNumberBatchList
                    }
                });
            }
            else
            {
                var inStockItemRecords = _stockInService.GetAllInStockItemRecordsByCompId(compId);
                var products = _warehouseProductService.GetProductsByCompId(compId);

                
                var matchedInStockRecord = inStockItemRecords.Where(i => i.LotNumberBatch == request.LotNumberBatch).FirstOrDefault();
                if (matchedInStockRecord == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"找不到批次:${request.LotNumberBatch}的入庫紀錄"
                    });
                }
                var matchedProduct = products.Where(p => p.ProductId == matchedInStockRecord.ProductId).FirstOrDefault();
                if (matchedProduct == null)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"找不到該品項"
                    });
                }
                
                var (successful, _, notifyProductQuantity) = _stockOutService.OutStock(request.Type, request, matchedInStockRecord, matchedProduct, memberAndPermissionSetting.Member, compId);

                return Ok(new CommonResponse<dynamic>
                {
                    Result = successful,
                });
            }

            
        }

        [HttpPost("batchOutbound")]
        [Authorize]
        public IActionResult BatchOutboundItems(BatchOutboundRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            request.CompId = compId;
            var validationResult = _batchOutboundValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            if (request.Type == CommonConstants.OutStockType.PURCHASE_OUT)
            {
                (List<string> notFoundLotNumberBatchList, Dictionary<string, List<InStockItemRecord>> lotNumberBatchAndProductCodeInStockExFIFORecordsMap, List<string> productCodeList)
                = FindSameProductInStockRecordsNotAllOutExpirationFIFO(request.OutboundItems, compId);
                if (notFoundLotNumberBatchList.Count > 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"以下批次{string.Join(",", notFoundLotNumberBatchList)}未找到品項的入庫紀錄"
                    });
                }

                (notFoundLotNumberBatchList, Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap, productCodeList) = FindMatchedProd(request.OutboundItems, compId);
                if (notFoundLotNumberBatchList.Count > 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"以下批次: {string.Join(",", notFoundLotNumberBatchList)} 未找到相對應的庫存品項productCode: {string.Join(",", productCodeList)}"
                    });
                }


                (List<Dictionary<string, dynamic>> notOldestLotList, Dictionary<string, InStockItemRecord> lotNumberBatchRequestLotMap) =
                    FindNotOldestLotList(request.OutboundItems, lotNumberBatchAndProductMap, lotNumberBatchAndProductCodeInStockExFIFORecordsMap);

                List<NeedQc> needQcListForOutboundItems = FindNeedQcList(request.OutboundItems, lotNumberBatchAndProductMap, lotNumberBatchRequestLotMap);
                List<string> printStickerLotNumberBatchList = GetPrintStickerLotNumberBatchList(request.OutboundItems, lotNumberBatchAndProductMap);
                var (isNewLotNumberList, isNewLotNumberBatchList) = GetNewLotList(request.OutboundItems, lotNumberBatchAndProductMap, lotNumberBatchRequestLotMap);


                //if (notOldestLotList.Count > 0)
                //{
                //    return BadRequest(new CommonResponse<List<Dictionary<string, dynamic>>>
                //    {
                //        Result = false,
                //        Message = "還有效期更早的批號還沒出",
                //        Data = notOldestLotList
                //    });
                //}
                List<string?> unOutableLotNumberBatchList = notOldestLotList.Select(lot => lot.GetValueOrDefault("requestLotNumberBatch") as string).ToList();
                List<string>? needConfirmedQcLotNumberBatchList = needQcListForOutboundItems.Where(qc => !string.IsNullOrEmpty(qc.LotNumberBatch)).Select(qc => qc.LotNumberBatch).ToList();
                var outableOutBoundItems = request.OutboundItems.Where(i => !unOutableLotNumberBatchList.Contains(i.LotNumberBatch) && !needConfirmedQcLotNumberBatchList.Contains(i.LotNumberBatch));



                List<string> failedOutLotNumberBatchList = new();
                List<NotifyProductQuantity> notifyProductQuantityList = new();
                List<NeedQc> needQcList = new();
                foreach (var outItem in outableOutBoundItems)
                {
                    var product = lotNumberBatchAndProductMap[outItem.LotNumberBatch];
                    var requestLot = lotNumberBatchRequestLotMap[outItem.LotNumberBatch];
                    if (outItem.ApplyQuantity > requestLot.InStockQuantity)
                    {
                        return BadRequest(new CommonResponse<dynamic>
                        {
                            Result = false,
                            Message = $"出庫數量超過入庫數量 批次:{outItem.LotNumberBatch},入庫數量:{requestLot.InStockQuantity}"
                        });
                    }

                    var (successful, _, notifyProductQuantity) = _stockOutService.OutStock(request.Type, outItem, requestLot, product, memberAndPermissionSetting.Member, compId);
                    if (!successful)
                    {
                        failedOutLotNumberBatchList.Add(outItem.LotNumberBatch);
                    }
                    else
                    {
                        notifyProductQuantityList.Add(notifyProductQuantity);
                    }
                }
                if (notifyProductQuantityList.Count > 0)
                {
                    //CalculateForQuantityToNotity(notifyProductQuantityList);
                }

                return Ok(new CommonResponse<dynamic>
                {
                    Result = (failedOutLotNumberBatchList.Count == 0 && notOldestLotList.Count == 0 && needQcListForOutboundItems.Count == 0),
                    Data = new Dictionary<string, dynamic>
                    {
                        ["failedLotNumberBatchList"] = failedOutLotNumberBatchList,
                        ["notOldestLotList"] = notOldestLotList,
                        ["needQcList"] = needQcListForOutboundItems,
                        ["printStickerLotBatchList"] = printStickerLotNumberBatchList,
                        ["isNewLotNumberList"] = isNewLotNumberList,
                        ["isNewLotNumberBatchList"] = isNewLotNumberBatchList
                    }
                });
            }
            else
            {
                var inStockItemRecords = _stockInService.GetAllInStockItemRecordsByCompId(compId);
                var products = _warehouseProductService.GetProductsByCompId(compId);

                foreach (var item in request.OutboundItems)
                {
                    var matchedInStockRecord = inStockItemRecords.Where(i => i.LotNumberBatch == item.LotNumberBatch).FirstOrDefault();
                    if (matchedInStockRecord == null)
                    {
                        return BadRequest(new CommonResponse<dynamic>
                        {
                            Result = false,
                            Message = $"找不到批次:${item.LotNumberBatch}的入庫紀錄"
                        });
                    }
                    var matchedProduct = products.Where(p => p.ProductId == matchedInStockRecord.ProductId).FirstOrDefault();
                    if (matchedProduct == null)
                    {
                        return BadRequest(new CommonResponse<dynamic>
                        {
                            Result = false,
                            Message = $"找不到該品項"
                        });
                    }

                }
                foreach (var item in request.OutboundItems)
                {
                    var matchedInStockRecord = inStockItemRecords.Where(i => i.LotNumberBatch == item.LotNumberBatch).FirstOrDefault();
                    var matchedProduct = products.Where(p => p.ProductId == matchedInStockRecord.ProductId).FirstOrDefault();
                    var (successful, _, notifyProductQuantity) = _stockOutService.OutStock(request.Type, item, matchedInStockRecord, matchedProduct, memberAndPermissionSetting.Member, compId);
                    if (!successful)
                    {
                        return Ok(new CommonResponse<dynamic>
                        {
                            Result = false,
                        });
                    }
                
                }

                return Ok(new CommonResponse<dynamic>
                {
                    Result = true,
                });
            }

            
        }

        [HttpPost("owner/outbound")]
        [Authorize]
        public IActionResult OwnerOutboundItems(OwnerOutboundRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var validationResult = _ownerOutboundValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            (List<string> notFoundLotNumberBatchList, Dictionary<string, List<InStockItemRecord>> lotNumberBatchAndProductCodeInStockExFIFORecordsMap,List<string> productCodeList) = FindSameProductInStockRecordsNotAllOutExpirationFIFO(new List<OwnerOutboundRequest> { request }, compId);
            if (notFoundLotNumberBatchList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"批號: {string.Join(",", notFoundLotNumberBatchList)} 未找到對應品項productCode: {string.Join(",", productCodeList)} 的入庫紀錄"
                });
            }

            
            // 檢查要出庫是否為效期最早或FI的那批
            InStockItemRecord? requestLot = null;
            requestLot = lotNumberBatchAndProductCodeInStockExFIFORecordsMap[request.LotNumberBatch].FirstOrDefault(record => record.LotNumberBatch == request.LotNumberBatch);
            if (requestLot == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "未找到相對應尚未出庫的入庫紀錄"
                });
            }
            var oldestLot = lotNumberBatchAndProductCodeInStockExFIFORecordsMap[request.LotNumberBatch].FirstOrDefault();
            // 表示要出的批號不是最早的那批 IsAbnormal!=true(非user確認過的)
            if ((requestLot.LotNumberBatch != oldestLot.LotNumberBatch) && request.IsAbnormal!=true)
            {
                return BadRequest(new CommonResponse<Dictionary<string, dynamic>>
                {
                    Result = false,
                    Message = "沒有先進先出",
                    Data = new Dictionary<string, dynamic>
                    {
                        ["isFIFO"] = false,
                        ["oldest"] = oldestLot,
                        ["requestLotNumberBatch"] = requestLot.LotNumberBatch,
                    }
                });
            }

            var product = _warehouseProductService.GetProductByProductCodeAndCompId(request.ProductCode, compId);
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
            if (request.Type == CommonConstants.OutStockType.SHIFT_OUT)
            {
                // 找到要調撥過去的單位還沒入庫的AcceptItem
                toCompAcceptanceItem = _stockInService.GetAcceptanceItemNotAllInStockByProductCodeAndCompId(requestLot.ProductCode, request.ToCompId).FirstOrDefault();
                if (toCompAcceptanceItem == null )
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = "要調撥過去的單位沒有相符的待驗收採購品項"
                    });
                }
            }

            var (result,_, notifyProductQuantity) = _stockOutService.OwnerOutStock(request.Type,request, requestLot, product, memberAndPermissionSetting.Member, toCompAcceptanceItem, compId);
            //CalculateForQuantityToNotity(new List<NotifyProductQuantity> { notifyProductQuantity });
            
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }


        [HttpPost("owner/batchOutbound")]
        [Authorize]
        public IActionResult OwnerBatchOutboundItems(BatchOwnerOutboundRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            request.CompId = compId;
            var validationResult = _batchOwnerOutboundValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            
            (List<string> notFoundLotNumberBatchList, Dictionary<string, List<InStockItemRecord>>  lotNumberBatchAndProductCodeInStockExFIFORecordsMap,List<string > productCodeList) = FindSameProductInStockRecordsNotAllOutExpirationFIFO(request.OutboundItems,compId);
            if (notFoundLotNumberBatchList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"以下批號: {string.Join(",", notFoundLotNumberBatchList)} 未找到對應品項product:  {string.Join(",", productCodeList)}  的入庫紀錄"
                });
            }

            (notFoundLotNumberBatchList, Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap, productCodeList) = FindMatchedProd(request.OutboundItems, compId);
            if (notFoundLotNumberBatchList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"以下批號: {string.Join(",", notFoundLotNumberBatchList)} 未找到相對應的庫存品項productCode: {string.Join(",", productCodeList)}"
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

            Dictionary<string, AcceptanceItem> lotNumberBatchAndToCompAcceptanceItem = new();
            if (request.Type == CommonConstants.OutStockType.SHIFT_OUT)
            {
                (notFoundLotNumberBatchList, lotNumberBatchAndToCompAcceptanceItem) = FindToCompAcceptItems(request.OutboundItems, lotNumberBatchAndProductCodeInStockExFIFORecordsMap, request.ToCompId);
            }

            if (notFoundLotNumberBatchList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"要調撥過去的單位沒有找到相符批號: {string.Join(",", notFoundLotNumberBatchList)} 的待驗收採購品項"
                });
            }

            (List<Dictionary<string, dynamic>> notOldestLotList, Dictionary< string, InStockItemRecord > lotNumberBatchRequestLotMap) = 
                FindNotOldestLotList(request.OutboundItems,lotNumberBatchAndProductMap, lotNumberBatchAndProductCodeInStockExFIFORecordsMap);
            //if (notOldestLotList.Count > 0)
            //{
            //    return BadRequest(new CommonResponse<List<Dictionary<string, dynamic>>>
            //    {
            //        Result = false,
            //        Message = "還有效期更早的批號還沒出",
            //        Data = notOldestLotList
            //    });
            //};
            List<string?> unOutableLotNumberBatchList = notOldestLotList.Select(lot => lot.GetValueOrDefault("requestLotNumberBatch") as string).ToList();
            var outableOutBoundItems = request.OutboundItems.Where(i => !unOutableLotNumberBatchList.Contains(i.LotNumberBatch));


            List<string> failedOutLotNumberBatchList = new();
            List<NotifyProductQuantity> notifyProductQuantityList = new();
            foreach (var outItem in outableOutBoundItems)
            {
                var product = lotNumberBatchAndProductMap[outItem.LotNumberBatch];
                var requestLot = lotNumberBatchRequestLotMap[outItem.LotNumberBatch];
                var toCompAcceptanceItem = lotNumberBatchAndToCompAcceptanceItem[outItem.LotNumberBatch];
                var (successful,msg, notifyProductQuantity) = _stockOutService.OwnerOutStock(request.Type,outItem, requestLot, product, memberAndPermissionSetting.Member, toCompAcceptanceItem, compId);
                if (!successful)
                {
                    failedOutLotNumberBatchList.Add(outItem.LotNumberBatch);
                }
                else
                {
                    notifyProductQuantityList.Add(notifyProductQuantity);
                }
                if (notifyProductQuantityList.Count > 0)
                {
                    //CalculateForQuantityToNotity(notifyProductQuantityList);
                }
            }
            return Ok(new CommonResponse<dynamic>
            {
                Result = failedOutLotNumberBatchList.Count == 0 && notOldestLotList.Count == 0,
                Data = new Dictionary<string, dynamic>
                {
                    ["failedLotNumberBatchList"] = failedOutLotNumberBatchList,
                    ["notOldestLotList"] = notOldestLotList
                }
            });
        }



        [HttpPost("records/list")]
        [Authorize]
        public IActionResult ListStockOutRecords(ListStockOutRecordsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }

            var validationResult = _listStockOutRecordsValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var (data, totalPages) = _stockOutService.ListStockOutRecords(request);
            var distinctProductIds = data.Select(x => x.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIds, compId);

            var outStockRecordVoList = _mapper.Map<List<OutStockRecordVo>>(data);

            var allReturnStockRecordList = _stockOutService.GetReturnStockRecords(request.CompId);

            var outStockIds = outStockRecordVoList.Select(o => o.OutStockId).ToList();
            var discardRecords = _discardService.ListDiscardRecordsByOutStockIds(outStockIds,compId);

            foreach (var item in outStockRecordVoList)
            {
                var matchedProdcut = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProdcut?.Unit;
                item.OpenDeadline = matchedProdcut?.OpenDeadline ?? 0;
                item.ProductModel = matchedProdcut?.ProductModel;
                if (item.IsReturned == true)
                {
                    var matchedReturnRecords = allReturnStockRecordList.Where(r=>r.OutStockId==item.OutStockId).ToList();
                    var returnInfoList = matchedReturnRecords.Select(r => new ReturnStockInfo
                    {
                        ReturnQuantity = r.ReturnQuantity.Value,
                        OutStockApplyQuantityBefore = r.OutStockApplyQuantityBefore,
                        OutStockApplyQuantityAfter = r.OutStockApplyQuantityAfter,
                        AfterQuantityBefore = r.AfterQuantityBefore.Value,
                        AfterQuantityAfter = r.AfterQuantityAfter.Value,
                        ReturnStockDateTime = r.CreatedAt.Value,
                    }).ToList();
                    item.ReturnStockInfoList = returnInfoList;
                }
                item.IsAllowDiscard = matchedProdcut.IsAllowDiscard;
                var matchedDiscardRecords = discardRecords.Where(d => d.OutStockId == item.OutStockId).OrderBy(r=>r.CreatedAt).ToList();
                if (matchedDiscardRecords.Count > 0)
                {
                    item.DiscardQuantityList = matchedDiscardRecords.Select(d => d.ApplyQuantity).ToList();
                    item.DiscardReasonList = matchedDiscardRecords.Select(d => d.DiscardReason ?? string.Empty).ToList();
                    item.DiscardTimeList = matchedDiscardRecords.Select(d => d.CreatedAt).ToList();
                    item.DiscardUserNameList = matchedDiscardRecords.Select(d => d.DiscardUserName).ToList();
                }
                if (item.InstrumentId.HasValue)
                {
                    var instrument = _instrumentService.GetById(item.InstrumentId.Value);
                    item.InstrumentName = instrument.InstrumentName;
                }

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
            request.CompId = compId;
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
            var outStockIds = outStockRecordVoList.Select(o => o.OutStockId).ToList();
            var discardRecords = _discardService.ListDiscardRecordsByOutStockIds(outStockIds, null);
            foreach (var item in outStockRecordVoList)
            {
                var matchedProdcut = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProdcut?.Unit;
                item.IsAllowDiscard = matchedProdcut.IsAllowDiscard;

                var matchedDiscardRecords = discardRecords.Where(d => d.OutStockId == item.OutStockId).OrderBy(r => r.CreatedAt).ToList();
                if (matchedDiscardRecords.Count > 0)
                {
                    item.DiscardQuantityList = matchedDiscardRecords.Select(d => d.ApplyQuantity).ToList();
                    item.DiscardReasonList = matchedDiscardRecords.Select(d => d.DiscardReason ?? string.Empty).ToList();
                    item.DiscardTimeList = matchedDiscardRecords.Select(d => d.CreatedAt).ToList();
                    item.DiscardUserNameList = matchedDiscardRecords.Select(d => d.DiscardUserName).ToList();
                }
            }


            return Ok(new CommonResponse<List<OutStockRecordVo>>
            {
                Result = true,
                Data = outStockRecordVoList,
                TotalPages = totalPages
            });
        }

        [HttpPost("getProductInfoByLotNumberBatch")]
        [Authorize]
        public IActionResult GetProductInfoByLotNumberBatch(GetProductInfoReqeust request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            var inStockRecord = _stockInService.GetInStockRecordsHistoryByLotNumberBatch(request.LotNumberBatch, compId).FirstOrDefault();
            if (inStockRecord == null)
            {
                return BadRequest(new CommonResponse<WarehouseProductStockOutView>
                {
                    Result = false,
                    Message = "沒有相對應的入庫紀錄",
                });
            }
            if (inStockRecord.OutStockStatus == CommonConstants.OutStockStatus.ALL)
            {
                return BadRequest(new CommonResponse<WarehouseProductStockOutView>
                {
                    Result = false,
                    Message = "此批已全部出庫",
                });
            }

            WarehouseProduct? productInfo = _warehouseProductService.GetProductByProductCodeAndCompId(inStockRecord.ProductCode, inStockRecord.CompId);
            var subItem = _purchaseService.GetPurchaseSubItemByItemId(inStockRecord.ItemId);
            
            if (inStockRecord != null&&productInfo!=null)
            {
                //這是因為畫面批號批次是要顯示正要出庫的這批資料　其他則是庫存品項的相關資料
                productInfo.LotNumber = inStockRecord.LotNumber;
                productInfo.LotNumberBatch = inStockRecord.LotNumberBatch;
            }
            else
            {
                return BadRequest(new CommonResponse<WarehouseProductStockOutView>
                {
                    Result = false,
                    Message = "沒有相對應的庫存品項",
                });
            }

            var productInstruments = _warehouseProductService.GetProductInstrumentsByProductId(productInfo.ProductId);

            WarehouseProductStockOutViewWithInstruments resultItem = new WarehouseProductStockOutViewWithInstruments
            {
                LotNumberBatch = productInfo.LotNumberBatch,
                LotNumber = productInfo.LotNumber,
                CompId = productInfo.CompId,
                ManufacturerId = productInfo.ManufacturerId,
                ManufacturerName = productInfo.ManufacturerName,
                DeadlineRule = productInfo.DeadlineRule,
                DeliverRemarks = productInfo.DeliverRemarks,
                GroupIds = productInfo.GroupIds,
                GroupNames = productInfo.GroupNames,
                InStockQuantity = productInfo.InStockQuantity,
                Manager = productInfo.Manager,
                MaxSafeQuantity = productInfo.MaxSafeQuantity,
                LastAbleDate = productInfo.LastAbleDate,
                LastOutStockDate = productInfo.LastOutStockDate,
                OpenDeadline = productInfo.OpenDeadline,
                OpenedSealName = productInfo.OpenedSealName,
                OriginalDeadline = productInfo.OriginalDeadline,
                PackageWay = productInfo.PackageWay,
                PreDeadline = productInfo.PreDeadline,
                PreOrderDays = productInfo.PreOrderDays,
                ProductCategory = productInfo.ProductCategory,
                ProductCode = productInfo.ProductCode,
                ProductId = productInfo.ProductId,
                ProductModel = productInfo.ProductModel,
                ProductName = productInfo.ProductName,
                ProductRemarks = productInfo.ProductRemarks,
                ProductSpec = productInfo.ProductSpec,
                SafeQuantity = productInfo.SafeQuantity,
                UdibatchCode = productInfo.UdibatchCode,
                UdicreateCode = productInfo.UdicreateCode,
                UdiserialCode = productInfo.UdiserialCode,
                UdiverifyDateCode = productInfo.UdiverifyDateCode,
                Unit = productInfo.Unit,
                Weight = productInfo.Weight,
                ProductMachine = productInfo.ProductMachine,
                DefaultSupplierId = productInfo.DefaultSupplierId,
                DefaultSupplierName = productInfo.DefaultSupplierName,
                IsNeedAcceptProcess = productInfo.IsNeedAcceptProcess,
                AllowReceiveDateRange = productInfo.AllowReceiveDateRange,
                CreatedAt = productInfo.CreatedAt,
                UpdatedAt = productInfo.UpdatedAt,
                UnitConversion = productInfo.UnitConversion,
                TestCount = productInfo.TestCount,
                IsActive = productInfo.IsActive,
                StockLocation = productInfo.StockLocation,
                Delievery = productInfo.Delievery,
                SupplierUnitConvertsion = productInfo.SupplierUnitConvertsion,
                SupplierUnit = productInfo.SupplierUnit,
                DeliverFunction = productInfo.DeliverFunction,
                DeliverTemperature = productInfo.DeliverTemperature,
                SavingFunction = productInfo.SavingFunction,
                SavingTemperature = productInfo.SavingTemperature,
                CompName = productInfo.CompName,
                BatchInStockQuantity = inStockRecord.InStockQuantity,
                BatchOutStockQuantity = inStockRecord.OutStockQuantity,
                BatchExpirationDate = inStockRecord.ExpirationDate,
                PurchaseMainId = subItem?.PurchaseMainId,
                InstrumentIdList = productInstruments.Select(pi => pi.InstrumentId).ToList(),
                InstrumentNameList = productInstruments.Select(pi => pi.InstrumentName).ToList(),
            };



            return Ok(new CommonResponse<WarehouseProductStockOutViewWithInstruments>
            {
                Result = true,
                Data = resultItem,
            });
        }



        private (List<string>, Dictionary<string, List<InStockItemRecord>>, List<string>) FindSameProductInStockRecordsNotAllOutExpirationFIFO(List<OutboundRequest> outBoundItems, string compId)
        {
            List<string> notFoundLotNumberBatchList = new();
            Dictionary<string, List<InStockItemRecord>> lotNumberBatchAndProductCodeInStockExFIFORecordsMap = new();
            List<string> productCodeList = new();
            foreach (var outItem in outBoundItems)
            {
                var inStockRecord = _stockInService.GetInStockRecordByLotNumberBatch(outItem.LotNumberBatch, compId);
                if (inStockRecord == null)
                {
                    notFoundLotNumberBatchList.Add(outItem.LotNumberBatch);
                    return (notFoundLotNumberBatchList, lotNumberBatchAndProductCodeInStockExFIFORecordsMap, productCodeList);
                }

                var productCode = inStockRecord.ProductCode;
                outItem.ProductCode = productCode;
                productCodeList.Add(productCode);
                List<InStockItemRecord> inStockItemRecordsNotAllOutExDateFIFO = _stockInService.GetProductInStockRecordsHistoryNotAllOutExpirationFIFO(productCode, compId);

                if (inStockItemRecordsNotAllOutExDateFIFO.Count == 0)
                {
                    notFoundLotNumberBatchList.Add(outItem.LotNumberBatch);
                }
                else
                {
                    lotNumberBatchAndProductCodeInStockExFIFORecordsMap[outItem.LotNumberBatch] = inStockItemRecordsNotAllOutExDateFIFO;
                }
            }
            return (notFoundLotNumberBatchList, lotNumberBatchAndProductCodeInStockExFIFORecordsMap, productCodeList);
        }

        private (List<string>, Dictionary<string, List<InStockItemRecord>>,List<string>) FindSameProductInStockRecordsNotAllOutExpirationFIFO(List<OwnerOutboundRequest> outBoundItems,string compId)
        {
            List<string> notFoundLotNumberBatchList = new();
            Dictionary<string, List<InStockItemRecord>> lotNumberBatchAndProductCodeInStockExFIFORecordsMap = new();
            List<string> productCodeList = new();
            foreach (var outItem in outBoundItems)
            {
                var inStockRecord = _stockInService.GetInStockRecordByLotNumberBatch(outItem.LotNumberBatch, compId);
                if (inStockRecord == null)
                {
                    notFoundLotNumberBatchList.Add(outItem.LotNumberBatch);
                    return (notFoundLotNumberBatchList, lotNumberBatchAndProductCodeInStockExFIFORecordsMap, productCodeList);
                }
                var productCode = inStockRecord.ProductCode;
                outItem.ProductCode = productCode;
                productCodeList.Add(productCode);
                List<InStockItemRecord> inStockItemRecordsNotAllOutExDateFIFO = _stockInService.GetProductInStockRecordsHistoryNotAllOutExpirationFIFO(productCode, compId);

                if (inStockItemRecordsNotAllOutExDateFIFO.Count == 0)
                {
                    notFoundLotNumberBatchList.Add(outItem.LotNumberBatch);
                }
                else
                {
                    lotNumberBatchAndProductCodeInStockExFIFORecordsMap[outItem.LotNumberBatch] = inStockItemRecordsNotAllOutExDateFIFO;
                }
            }
            return (notFoundLotNumberBatchList,lotNumberBatchAndProductCodeInStockExFIFORecordsMap, productCodeList);
        }

        private (List<string>, Dictionary<string, WarehouseProduct>, List<string>) FindMatchedProd(List<OutboundRequest> outBoundItems, string compId)
        {
            List<string> notFoundLotNumberBatchList = new();
            Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap = new();
            List<string> productCodeList = new();
            foreach (var outItem in outBoundItems)
            {
                var productCode = outItem.ProductCode;
                var product = _warehouseProductService.GetProductByProductCodeAndCompId(productCode, compId);

                if (product == null)
                {
                    notFoundLotNumberBatchList.Add(outItem.LotNumberBatch);
                }
                else
                {
                    lotNumberBatchAndProductMap[outItem.LotNumberBatch] = product;
                    productCodeList.Add(productCode);
                }
            }
            return (notFoundLotNumberBatchList, lotNumberBatchAndProductMap, productCodeList);
        }
        private (List<string>, Dictionary<string, WarehouseProduct>, List<string>) FindMatchedProd(List<OwnerOutboundRequest> outBoundItems, string compId)
        {
            List<string> notFoundLotNumberBatchList = new();
            Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap = new();
            List<string> productCodeList = new();
            foreach (var outItem in outBoundItems)
            {
                var productCode = outItem.ProductCode;
                var product = _warehouseProductService.GetProductByProductCodeAndCompId(productCode, compId);
               
                if (product == null)
                {
                    notFoundLotNumberBatchList.Add(outItem.LotNumberBatch);
                }
                else
                {
                    lotNumberBatchAndProductMap[outItem.LotNumberBatch] = product;
                    productCodeList.Add(productCode);
                }
            }
            return (notFoundLotNumberBatchList, lotNumberBatchAndProductMap, productCodeList);
        }


        private List<NeedQc> FindNeedQcList(List<OutboundRequest> outBoundItems, Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap, Dictionary<string, InStockItemRecord> lotNumberBatchRequestLotMap)
        {
            List<NeedQc> needQcList = new();
            outBoundItems.ForEach(item =>
            {
                var requestLot = lotNumberBatchRequestLotMap[item.LotNumberBatch];
                var IsNeedQc = requestLot.IsNeedQc == true && requestLot.QcTestStatus == CommonConstants.QcTestStatus.NONE &&requestLot.QcType!=CommonConstants.QcTypeConstants.NONE&&item.IsSkipQc!=true;
                if (IsNeedQc)
                {
                    var purchaseMain = _stockInService.GetPurchaseMainByInStockId(requestLot);
                    var needQc = new NeedQc()
                    {
                        PurchaseMainID = purchaseMain.PurchaseMainId,
                        LotNumber = requestLot.LotNumber,
                        LotNumberBatch = requestLot.LotNumberBatch,
                        QcType = requestLot.QcType,
                        ProductID = requestLot.ProductId,
                        ProductName = requestLot.ProductName,
                        ApplyDate = purchaseMain.ApplyDate,
                        AcceptedAt = requestLot.CreatedAt.Value,
                        AcceptUserId = requestLot.UserId,
                        AcceptUserName = requestLot.UserName,
                    };
                    needQcList.Add(needQc);
                }
            });
            return needQcList;
        }

        private List<string> GetPrintStickerLotNumberBatchList(List<OutboundRequest> outBoundItems, Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap)
        {
            List<string> printStickerLotNumberBatchList = new();
            outBoundItems.ForEach(item =>
            {
                var lotNumberBatch = item.LotNumberBatch;
                var product = lotNumberBatchAndProductMap[lotNumberBatch];
                if (product.IsPrintSticker == true)
                {
                    printStickerLotNumberBatchList.Add(lotNumberBatch);
                }
            });
            return printStickerLotNumberBatchList;
        }

        private (List<string>,List<string>) GetNewLotList(List<OutboundRequest> outBoundItems, Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap, Dictionary<string, InStockItemRecord> lotNumberBatchRequestLotMap)
        {
            List<string> printStickerLotNumberBatchList = new();
            List<string> isNewLotNumberList = new();
            List<string> isNewLotNumberBatchList = new();
            outBoundItems.ForEach(item =>
            {
                var requestLot = lotNumberBatchRequestLotMap[item.LotNumberBatch];
                var product = lotNumberBatchAndProductMap[item.LotNumberBatch];

                if (requestLot.LotNumber != product.LotNumber)
                {
                    isNewLotNumberList.Add(requestLot.LotNumber);
                }
                if (requestLot.LotNumberBatch != product.LotNumberBatch)
                {
                    isNewLotNumberBatchList.Add(requestLot.LotNumberBatch);
                }
            });
            return (isNewLotNumberList,isNewLotNumberBatchList);
        }

        private (List<Dictionary<string, dynamic>>, Dictionary<string, InStockItemRecord>) FindNotOldestLotList(List<OutboundRequest> outBoundItems, Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap, Dictionary<string, List<InStockItemRecord>> lotNumberBatchAndproductCodeInStockExFIFORecords)
        {
            List<Dictionary<string, dynamic>> notOldestLotList = new();
            Dictionary<string, InStockItemRecord> lotNumberBatchAndRequestLotInStockRecordMap = new();
            foreach (var outItem in outBoundItems)
            {
                var lotNumbetBatch = outItem.LotNumberBatch;
                var matchedProduct = lotNumberBatchAndProductMap[lotNumbetBatch];
                var inStockItemRecordsNotAllOutExDateFIFO = lotNumberBatchAndproductCodeInStockExFIFORecords[lotNumbetBatch];

                InStockItemRecord? requestLot = null;

                requestLot = inStockItemRecordsNotAllOutExDateFIFO.FirstOrDefault(record => record.LotNumberBatch == outItem.LotNumberBatch);
                var oldestLot = inStockItemRecordsNotAllOutExDateFIFO.FirstOrDefault();
                if (requestLot == null)
                {
                    notOldestLotList.Add(new Dictionary<string, dynamic>
                    {
                        ["requestLotNumberBatch"] = outItem.LotNumberBatch,
                        ["oldest"] = oldestLot
                    });
                    continue;
                }
                // 表示要出的批號不是最早的那批 而且IsAbnormal不為true且AbnormalReason為空的或null(表示沒確認過)
                if ((requestLot.LotNumberBatch != oldestLot.LotNumberBatch) && outItem.IsAbnormal!=true )
                {
                    notOldestLotList.Add(new Dictionary<string, dynamic>
                    {
                        ["requestLotNumberBatch"] = requestLot.LotNumberBatch,
                        ["oldest"] = oldestLot
                    });
                }
                lotNumberBatchAndRequestLotInStockRecordMap[outItem.LotNumberBatch] = requestLot;
            }
            return (notOldestLotList, lotNumberBatchAndRequestLotInStockRecordMap);
        }

        private (List<Dictionary<string, dynamic>>,Dictionary<string,InStockItemRecord>) FindNotOldestLotList(List<OwnerOutboundRequest> outBoundItems,Dictionary<string,WarehouseProduct> lotNumberBatchAndProductMap,Dictionary<string,List<InStockItemRecord>> lotNumberBatchAndproductCodeInStockExFIFORecords)
        {
            List<Dictionary<string, dynamic>> notOldestLotList = new();
            Dictionary<string, InStockItemRecord> lotNumberBatchAndRequestLotInStockRecordMap = new();
            foreach (var outItem in outBoundItems)
            {
                var lotNumbetBatch = outItem.LotNumberBatch;
                var matchedProduct = lotNumberBatchAndProductMap[lotNumbetBatch];
                var inStockItemRecordsNotAllOutExDateFIFO = lotNumberBatchAndproductCodeInStockExFIFORecords[lotNumbetBatch];

                InStockItemRecord? requestLot = null;
                
                requestLot = inStockItemRecordsNotAllOutExDateFIFO.FirstOrDefault(record => record.LotNumberBatch == outItem.LotNumberBatch);
                var oldestLot = inStockItemRecordsNotAllOutExDateFIFO.FirstOrDefault();
                if (requestLot == null)
                {
                    notOldestLotList.Add(new Dictionary<string, dynamic>
                    {
                        ["isFIFO"] = false,
                        ["requestLotNumberBatch"] = outItem.LotNumberBatch,
                        ["oldest"] = oldestLot
                    });
                    continue;
                }
                // 表示要出的批號不是最早的那批 而且IsAbnormal!=true(非user確認過的)
                if ((requestLot.LotNumberBatch != oldestLot.LotNumberBatch)&&outItem.IsAbnormal!=true)
                {
                    notOldestLotList.Add(new Dictionary<string, dynamic>
                    {
                        ["isFIFO"] = false,
                        ["requestLotNumberBatch"] = requestLot.LotNumberBatch,
                        ["oldest"] = oldestLot
                    });
                }
                lotNumberBatchAndRequestLotInStockRecordMap[outItem.LotNumberBatch] = requestLot;
            }
            return (notOldestLotList,lotNumberBatchAndRequestLotInStockRecordMap);
        }

        private (List<string>, Dictionary<string, AcceptanceItem>) FindToCompAcceptItems(List<OwnerOutboundRequest> outBoundItems, Dictionary<string, List<InStockItemRecord>> lotNumberBatchAndproductCodeInStockExFIFORecords,string toCompId)
        {
            List<string> notFoundLotNumberBatchList = new();
            Dictionary<string, AcceptanceItem> lotNumberBatchAndToCompAcceptanceItem = new();
            foreach (var outItem in outBoundItems)
            {
                AcceptanceItem? toCompAcceptanceItem = null;
                List<InStockItemRecord> inStockItemRecordsNotAllOutExDateFIFO = lotNumberBatchAndproductCodeInStockExFIFORecords[outItem.LotNumberBatch];
                var requestLot = inStockItemRecordsNotAllOutExDateFIFO.FirstOrDefault(record => record.LotNumberBatch == outItem.LotNumberBatch);
                // 找到要調撥過去的單位還沒入庫的AcceptItem
                toCompAcceptanceItem = _stockInService.GetAcceptanceItemNotAllInStockByProductCodeAndCompId(requestLot.ProductCode, toCompId).FirstOrDefault();
                if (toCompAcceptanceItem == null )
                {
                    notFoundLotNumberBatchList.Add(outItem.LotNumberBatch);
                }
                else
                {
                    lotNumberBatchAndToCompAcceptanceItem[requestLot.LotNumberBatch] = toCompAcceptanceItem;
                }
            }
            
            return (notFoundLotNumberBatchList,lotNumberBatchAndToCompAcceptanceItem);
        }

        private async Task CalculateForQuantityToNotity(List<NotifyProductQuantity> notifyProductQuantityList)
        {
            var allProductIdList = notifyProductQuantityList.Select(item => item.ProductId).Distinct().ToList();
            var allUnDonePurchaseSubItemList = _purchaseService.GetNotDonePurchaseSubItemByProductIdList(allProductIdList);
            foreach (var notifyProductQuantity in notifyProductQuantityList)
            {
                var matchedSubItemList = allUnDonePurchaseSubItemList.Where(i=>i.ProductId==notifyProductQuantity.ProductId).ToList();
                float inProcessingQrderQuantity = matchedSubItemList.Select(i => i.Quantity??0.0f).DefaultIfEmpty(0.0f).Sum();
                notifyProductQuantity.InProcessingQrderQuantity = (float)inProcessingQrderQuantity ;
            }
            notifyProductQuantityList.ForEach(notifyProductQuantity =>
            {
                float neededOrderQuantity = notifyProductQuantity.SafeQuantity - notifyProductQuantity.InProcessingQrderQuantity - notifyProductQuantity.InStockQuantity;
                if (neededOrderQuantity>0)
                {
                    string title = $"品項:{notifyProductQuantity.ProductName}庫存量不足,需訂購數量{neededOrderQuantity}";
                    string content = $"品項名稱:{notifyProductQuantity.ProductName}<br />品項編號:{notifyProductQuantity.ProductCode}<br />最大庫存量:{notifyProductQuantity.MaxSafeQuantity}<br />"
                    +$"最低庫存量:{notifyProductQuantity.SafeQuantity}<br />目前庫存量:{notifyProductQuantity.InStockQuantity}<br />正在處理中的訂單數量:{notifyProductQuantity.InProcessingQrderQuantity}<br />";
                    List<WarehouseMember> receiverList = _memberService.GetAllMembersOfComp(notifyProductQuantity.CompId).Where(m=>m.IsActive==true).ToList();
                    List<string> effectiveEmailList = receiverList.Where(r=>!string.IsNullOrEmpty(r.Email)).Select(r=>r.Email).ToList();
                    effectiveEmailList.ForEach(effectiveEmail => _emailService.SendAsync(title, content, effectiveEmail));
                }
            });
        }
    }
}
