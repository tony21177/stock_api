﻿using AutoMapper;
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
        private readonly IValidator<BatchOutboundRequest> _batchOutboundValidator;
        private readonly IValidator<OwnerOutboundRequest> _ownerOutboundValidator;
        private readonly IValidator<BatchOwnerOutboundRequest> _batchOwnerOutboundValidator;
        private readonly IValidator<ListStockOutRecordsRequest> _listStockOutRecordsValidator;


        public StockOutController(IMapper mapper, AuthHelpers authHelpers, GroupService groupService, StockInService stockInService, WarehouseProductService warehouseProductService, PurchaseService purchaseService, StockOutService stockOutService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _warehouseProductService = warehouseProductService;
            _stockOutService = stockOutService;
            _outboundValidator = new OutboundValidator();
            _batchOutboundValidator = new BatchOutboundValidator();
            _ownerOutboundValidator = new OwnerOutboundValidator();
            _batchOwnerOutboundValidator = new BatchOwnerOutboundValidator();
            _listStockOutRecordsValidator = new ListStockOutRecordsValidator();
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

            var inStockRecord = _stockInService.GetInStockRecordByLotNumberBatch(request.LotNumberBatch,compId);
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
                    return BadRequest(new CommonResponse<Dictionary<string, dynamic>>
                    {
                        Result = false,
                        Message = "還有效期更早的批號還沒出",
                        Data = new Dictionary<string, dynamic>
                        {
                            ["isFIFO"] = false,
                            ["oldest"] = oldestLot,
                            ["requestLotNumberBatch"] = requestLot.LotNumberBatch,
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

            var result = _stockOutService.OutStock(request.Type,request, requestLot, product, memberAndPermissionSetting.Member,compId);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
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


            (List<string> notFoundLotNumberBatchList, Dictionary<string, List<InStockItemRecord>> lotNumberBatchAndProductCodeInStockExFIFORecordsMap,List<string> productCodeList) 
                = FindSameProductInStockRecordsNotAllOutExpirationFIFO(request.OutboundItems,compId);
            if (notFoundLotNumberBatchList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"以下批次{string.Join(",", notFoundLotNumberBatchList)}未找到品項的入庫紀錄"
                });
            }

            (notFoundLotNumberBatchList, Dictionary<string, WarehouseProduct> lotNumberBatchAndProductMap,productCodeList) = FindMatchedProd(request.OutboundItems,compId);
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
            var outableOutBoundItems = request.OutboundItems.Where(i => !unOutableLotNumberBatchList.Contains(i.LotNumberBatch));



            List<string> failedOutLotNumberBatchList = new();
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

                var successful = _stockOutService.OutStock(request.Type,outItem, requestLot, product, memberAndPermissionSetting.Member,compId);
                if (!successful)
                {
                    failedOutLotNumberBatchList.Add(outItem.LotNumberBatch);
                }
            }


            return Ok(new CommonResponse<dynamic>
            {
                Result = (failedOutLotNumberBatchList.Count == 0&& notOldestLotList.Count==0),
                Data = new Dictionary<string, dynamic>
                {
                    ["failedLotNumberBatchList"] = failedOutLotNumberBatchList,
                    ["notOldestLotList"] = notOldestLotList
                }
            });
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

            var result = _stockOutService.OwnerOutStock(request.Type,request, requestLot, product, memberAndPermissionSetting.Member, toCompAcceptanceItem, compId);
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
            foreach (var outItem in outableOutBoundItems)
            {
                var product = lotNumberBatchAndProductMap[outItem.LotNumberBatch];
                var requestLot = lotNumberBatchRequestLotMap[outItem.LotNumberBatch];
                var toCompAcceptanceItem = lotNumberBatchAndToCompAcceptanceItem[outItem.LotNumberBatch];
                var successful = _stockOutService.OwnerOutStock(request.Type,outItem, requestLot, product, memberAndPermissionSetting.Member, toCompAcceptanceItem, compId);
                if (!successful)
                {
                    failedOutLotNumberBatchList.Add(outItem.LotNumberBatch);
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
            request.CompId = compId;
            var validationResult = _listStockOutRecordsValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            request.CompId = compId;
            var (data, totalPages) = _stockOutService.ListStockOutRecords(request);
            var distinctProductIds = data.Select(x => x.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIds, compId);

            var outStockRecordVoList = _mapper.Map<List<OutStockRecordVo>>(data);
            foreach (var item in outStockRecordVoList)
            {
                var matchedProdcut = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProdcut?.Unit;
                item.OpenDeadline = matchedProdcut?.OpenDeadline ?? 0;
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

            WarehouseProductStockOutView resultItem = new WarehouseProductStockOutView
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
                BatchExpirationDate = inStockRecord.ExpirationDate
            };



            return Ok(new CommonResponse<WarehouseProductStockOutView>
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
    }
}
