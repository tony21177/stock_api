using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
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
    public class StockInController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly GroupService _groupService;
        private readonly StockInService _stockInService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly PurchaseService _purchaseService;
        private readonly StockOutService _stockOutService;
        private readonly IValidator<SearchPurchaseAcceptItemRequest> _searchPurchaseAcceptItemValidator;
        private readonly IValidator<UpdateBatchAcceptItemsRequest> _updateBatchAcceptItemsRequestValidator;
        private readonly IValidator<UpdateAcceptItemRequest> _updateAcceptItemRequestValidator;
        private readonly IValidator<UpdateBatchAcceptItemsRequest> _batchUdateAcceptItemRequestValidator;
        private readonly IValidator<ListStockInRecordsRequest> _listStockInRecordsValidator;
        private readonly IValidator<ReturnRequest> _returnStockValidator;
        private readonly IValidator<ListReturnRecordsRequest> _listReturnRecordsValidator;

        public StockInController(IMapper mapper, AuthHelpers authHelpers, GroupService groupService, StockInService stockInService, WarehouseProductService warehouseProductService, PurchaseService purchaseService, StockOutService stockOutService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _groupService = groupService;
            _stockInService = stockInService;
            _warehouseProductService = warehouseProductService;
            _purchaseService = purchaseService;
            _searchPurchaseAcceptItemValidator = new SearchPurchaseAcceptItemValidator(groupService);
            _updateBatchAcceptItemsRequestValidator = new UpdateBatchAcceptItemsRequestValidator();
            _updateAcceptItemRequestValidator = new UpdateAcceptItemValidator();
            _batchUdateAcceptItemRequestValidator = new UpdateBatchAcceptItemsRequestValidator();
            _listStockInRecordsValidator = new ListStockInRecordsValidator();
            _stockOutService = stockOutService;
            _returnStockValidator = new ReturnStockValidator();
            _listReturnRecordsValidator = new ListReturnRecordsValidator();
        }

        [HttpPost("purchaseAndAcceptItems/list")]
        [Authorize]
        public IActionResult ListPurchases(SearchPurchaseAcceptItemRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            request.CompId = compId;

            var validationResult = _searchPurchaseAcceptItemValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            List<PurchaseAcceptanceItemsView> purchaseAcceptanceItemsViewList = _stockInService.SearchPurchaseAcceptanceItems(request);
            List<string> distinctProductIdList = purchaseAcceptanceItemsViewList.Select(x => x.ProductId).Distinct().ToList();
            List<WarehouseProduct> products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);

            Dictionary<string, List<PurchaseAcceptanceItemsView>> purchaseMainIdAndAcceptionItemListMap = new();
            purchaseAcceptanceItemsViewList.ForEach(item =>
            {
                if (!purchaseMainIdAndAcceptionItemListMap.ContainsKey(item.PurchaseMainId))
                {
                    purchaseMainIdAndAcceptionItemListMap[item.PurchaseMainId] = new List<PurchaseAcceptanceItemsView>();
                };
                purchaseMainIdAndAcceptionItemListMap[item.PurchaseMainId].Add(item);
            });

            List<string> distinctItemIdList = purchaseAcceptanceItemsViewList.Select(a => a.ItemId).Distinct().ToList();
            List<PurchaseSubItem> purchaseSubItems = _purchaseService.GetPurchaseSubItemByItemIdList(distinctItemIdList);

            List<PurchaseAcceptItemsVo> data = new();

            foreach (var keyValuePair in purchaseMainIdAndAcceptionItemListMap)
            {
                List<PurchaseAcceptanceItemsView> purchaseAcceptanceItemViewList = keyValuePair.Value;
                PurchaseAcceptItemsVo purchaseAcceptItemsVo = _mapper.Map<PurchaseAcceptItemsVo>(purchaseAcceptanceItemViewList[0]);
                List<AcceptItem> acceptItems = _mapper.Map<List<AcceptItem>>(purchaseAcceptanceItemViewList);
                foreach (var item in acceptItems)
                {
                    var matchedProdcut = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                    if (matchedProdcut != null)
                    {
                        item.Unit = matchedProdcut.Unit;
                        item.ProductCode = matchedProdcut.ProductCode;
                        item.UDIBatchCode = matchedProdcut.UdibatchCode;
                        item.UDICreateCode = matchedProdcut.UdicreateCode;
                        item.UDIVerifyDateCode = matchedProdcut.UdiverifyDateCode;
                        item.Prod_supplierName = matchedProdcut.DefaultSupplierName;
                        item.ArrangeSupplierId = item.ArrangeSupplierId;
                        item.ArrangeSupplierName = item.ArrangeSupplierName;
                    }
                    var matchedSubItem = purchaseSubItems.Where(s=>s.ItemId==item.ItemId).FirstOrDefault();
                    item.PurchaseSubItem = matchedSubItem;

                }

                if (request.Keywords == null)
                {
                    purchaseAcceptItemsVo.AcceptItems = acceptItems;
                    data.Add(purchaseAcceptItemsVo);
                    continue;
                }
                if (request.Keywords != null && (purchaseAcceptItemsVo.IsContainKeywords(request.Keywords) || acceptItems.Any(acceptItem => acceptItem.IsContainKeywords(request.Keywords))))
                {
                    purchaseAcceptItemsVo.AcceptItems = acceptItems;
                    data.Add(purchaseAcceptItemsVo);
                    continue;
                }

            }
            if (request.GroupId != null)
            {
                foreach (var mainAndAccptItems in data)
                {
                    mainAndAccptItems.AcceptItems = mainAndAccptItems.AcceptItems.Where(a => a.PurchaseSubItem != null && a.PurchaseSubItem.GroupIds.Contains(request.GroupId)).ToList();
                }
                data = data.Where(e=>e.AcceptItems.Count>0).ToList();
            }

            List<AcceptItem> allAcceptItemList = new();
            if (request.IsGroupBySupplier == true)
            {
                

                allAcceptItemList = data.SelectMany(item => item.AcceptItems).ToList();
                Dictionary<int, List<AcceptItem>> supplierIdAndAcceptItemListMap = new();
                supplierIdAndAcceptItemListMap[-1] = new();
                allAcceptItemList.ForEach(item =>
                {
                    if (item.ArrangeSupplierId != null)
                    {
                        if (!supplierIdAndAcceptItemListMap.ContainsKey(item.ArrangeSupplierId.Value))
                        {
                            supplierIdAndAcceptItemListMap[item.ArrangeSupplierId.Value] = new();
                        }
                        supplierIdAndAcceptItemListMap[item.ArrangeSupplierId.Value].Add(item);
                    }
                    else
                    {
                        supplierIdAndAcceptItemListMap[-1].Add(item);
                    }
                });

                List<SupplierAccepItemsVo> result = new();
                foreach (var keyValuePair in supplierIdAndAcceptItemListMap)
                {
                    if (keyValuePair.Value.Count > 0)
                    {
                        SupplierVo supplierVo = new()
                        {
                            ArrangeSupplierId = keyValuePair.Value[0].ArrangeSupplierId ?? -1,
                            ArrangeSupplierName = keyValuePair.Value[0].ArrangeSupplierName,
                        };
                        SupplierAccepItemsVo supplierAccepItemsVo = new()
                        {
                            Supplier = supplierVo,
                            AcceptItems = keyValuePair.Value,
                        };
                        result.Add(supplierAccepItemsVo);
                    }
                }
                if (request.SupplierId != null)
                {
                    result = result.Where(i=>i.Supplier.ArrangeSupplierId==request.SupplierId).ToList();    
                }
                var purchaseMainIdList = result.SelectMany(e => e.AcceptItems.Where(i=>i.PurchaseMainId!=null).Select(i => i.PurchaseMainId)).Distinct().ToList();
                var allPurchaseMainList = _purchaseService.GetPurchaseMainsByMainIdList(purchaseMainIdList);
                result.ForEach(e => e.AcceptItems.ForEach(i =>
                {
                    var matchedPurchaseMain = allPurchaseMainList.Where(m=>m.PurchaseMainId==i.PurchaseMainId).FirstOrDefault();
                    i.PurchaseMainId = matchedPurchaseMain.PurchaseMainId;
                    i.ApplyDate = matchedPurchaseMain.ApplyDate;
                }));

                var responseGroup = new CommonResponse<List<SupplierAccepItemsVo>>
                {
                    Result = true,
                    Data = result
                };
                return Ok(responseGroup);
            }
            data.ForEach(e => e.AcceptItems = e.AcceptItems.OrderBy(a => a.ProductCode).ToList());

            int totalPages = 0;
            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "ProductCode";
            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);

                switch (orderByField)
                {
                    case "ApplyDate":
                        data = data.OrderByDescending(item => item.ApplyDate).ToList();
                        break;
                    case "DemandDate":
                        data = data.OrderByDescending(item => item.DemandDate).ToList();
                        break;
                    case "GroupId":
                        data = data.OrderBy(item => item.GroupIds).ToList();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                switch (orderByField)
                {
                    case "ApplyDate":
                        data = data.OrderBy(item => item.ApplyDate).ToList();
                        break;
                    case "DemandDate":
                        data = data.OrderBy(item => item.DemandDate).ToList();
                        break;
                    case "GroupId":
                        data = data.OrderBy(item => item.GroupIds).ToList();
                        break;
                    default:
                        break;
                }
            }
            int totalItems = data.Count;
            totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);
            data = data.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize).ToList();


            var response = new CommonResponse<List<PurchaseAcceptItemsVo>>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }


        [HttpPost("acceptItems/search")]
        [Authorize]
        public IActionResult SearchAcceptItem(SearchAcceptItemRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;


            List<AcceptanceItem> acceptanceItems = _stockInService.AcceptanceItemsByUdiSerialCode(request.UdiserialCode, compId).Where(i => i.AcceptUserId == null).ToList();
            var unVerifyAcceptance = acceptanceItems.Where(i => i.InStockStatus != CommonConstants.PurchaseSubItemReceiveStatus.DONE).OrderByDescending(i => i.UpdatedAt).FirstOrDefault();
            if (unVerifyAcceptance == null)
            {
                // 代表沒有可以驗收入庫的項目
                return Ok(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "以唯一碼：" + request.UdiserialCode + "搜尋後，並無發現需要驗收/入庫的項目"
                });
            }

            // Gary 增加取得 accept 品項連動的 product 資料
            List<string> distinctProductIdList = new List<string> { unVerifyAcceptance.ProductId };
            List<WarehouseProduct> products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);
            var matchedProdcut = products.Where(p => p.ProductId == unVerifyAcceptance.ProductId).FirstOrDefault();
            var purchaseMain = _purchaseService.GetPurchaseMainByMainId(unVerifyAcceptance.PurchaseMainId);


            ManualAcceptItem resultItem = new ManualAcceptItem
            {
                PurchaseMainId = unVerifyAcceptance.PurchaseMainId,
                AcceptId = unVerifyAcceptance.AcceptId,
                AcceptQuantity = unVerifyAcceptance.AcceptQuantity,
                AcceptUserId = unVerifyAcceptance.AcceptUserId,
                LotNumberBatch = unVerifyAcceptance.LotNumberBatch,
                LotNumber = unVerifyAcceptance.LotNumber,
                ExpirationDate = unVerifyAcceptance.ExpirationDate,
                ItemId = unVerifyAcceptance.ItemId,
                OrderQuantity = unVerifyAcceptance.OrderQuantity,
                PackagingStatus = unVerifyAcceptance.PackagingStatus,
                ProductId = unVerifyAcceptance.ProductId,
                ProductName = unVerifyAcceptance.ProductName,
                ProductSpec = unVerifyAcceptance.ProductSpec,
                UdiserialCode = unVerifyAcceptance.UdiserialCode,
                QcStatus = unVerifyAcceptance.QcStatus,
                CurrentTotalQuantity = unVerifyAcceptance.CurrentTotalQuantity,
                Comment = unVerifyAcceptance.Comment,
                QcComment = unVerifyAcceptance.QcComment,
                DeliverFunction = unVerifyAcceptance.DeliverFunction,
                DeliverTemperature = unVerifyAcceptance.DeliverTemperature,
                SavingFunction = unVerifyAcceptance.SavingFunction,
                SavingTemperature = unVerifyAcceptance.SavingTemperature,
                DemandDate = purchaseMain != null ? purchaseMain.DemandDate : null,
            };

            if (matchedProdcut != null)
            {
                resultItem.Unit = matchedProdcut.Unit;
                resultItem.UDIBatchCode = matchedProdcut.UdibatchCode;
                resultItem.UDICreateCode = matchedProdcut.UdicreateCode;
                resultItem.UDIVerifyDateCode = matchedProdcut.UdiverifyDateCode;
                resultItem.Prod_savingFunction = matchedProdcut.SavingFunction;
                resultItem.Prod_stockLocation = matchedProdcut.StockLocation;
            }

            return Ok(new CommonResponse<ManualAcceptItem>
            {
                Result = true,
                Data = resultItem,
            });
        }

        [HttpPost("acceptItem/verify")]
        [Authorize]
        public IActionResult VerifyAcceptItem(UpdateAcceptItemRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            request.AcceptUserId = userId;
            var isDirectOutStock = false;
            if (memberAndPermissionSetting.CompanyWithUnit.Type == CommonConstants.CompanyType.ORGANIZATION_NOSTOCK)
            {
                isDirectOutStock = true;
            }

            var validationResult = _updateAcceptItemRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var existingAcceptItem = _stockInService.GetAcceptanceItemByAcceptId(request.AcceptId);
            if (existingAcceptItem == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "驗收品項不存在"
                });
            }
            if (request.AcceptQuantity + existingAcceptItem.AcceptQuantity > existingAcceptItem.OrderQuantity)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"不可超過訂購數量,已入數量:{existingAcceptItem.AcceptQuantity},訂購數量:{existingAcceptItem.OrderQuantity}"
                });
            }

            var purchaseSubItem = _purchaseService.GetPurchaseSubItemByItemId(existingAcceptItem.ItemId);
            if (purchaseSubItem == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "對應的採購品項不存在"
                });
            }


            if (existingAcceptItem.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (existingAcceptItem.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE)
            {
                //return BadRequest(new CommonResponse<dynamic>{
                //    Result = false,
                //    Message = $"此驗收單狀態已為{existingAcceptItem.QcStatus},不可重複驗收"
                //});
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"此驗收單已全部入庫"
                });
            }


            var product = _warehouseProductService.GetProductByProductIdAndCompId(existingAcceptItem.ProductId, compId);
            if (product == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "庫存品項不存在"
                });
            }

            var purchaseMain = _purchaseService.GetPurchaseMainByMainId(existingAcceptItem.PurchaseMainId);
            if (purchaseMain == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "採購單不存在"
                });
            }
            if (request.ExpirationDate != null && product.DeadlineRule != null)
            {
                var expirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.ExpirationDate).Value);
                // check : 如果今天日期+deallinerule(至少要可以放幾天) > 保存期限，代表保存期前過短
                if (DateOnly.FromDateTime(DateTime.Now).AddDays(product.DeadlineRule.Value) > expirationDate && request.IsConfirmed != true)
                {
                    return Ok(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Data = new
                        {
                            isExceedDeadlineRule = true,
                            exceedDeadLineRuleIdList = new List<string> { existingAcceptItem.AcceptId }
                        }
                    });
                }
            }

            List<InStockItemRecord> existingStockInRecords = _stockInService.GetInStockRecordsHistory(existingAcceptItem.ProductId, compId).OrderByDescending(item => item.CreatedAt).ToList();
            var lastLotNumber = existingStockInRecords.FirstOrDefault()?.LotNumber;
            List<string> newLotNumberIdList = new();
            var (result, message,qc) = _stockInService.UpdateAcceptItem(purchaseMain, purchaseSubItem, existingAcceptItem, request, product, compId, memberAndPermissionSetting.Member, isDirectOutStock);
            if (request.LotNumber != lastLotNumber)
            {
                newLotNumberIdList.Add(request.AcceptId);
            }
            List<Qc> qcList = new ();
            if (qc != null)
            {
                qcList.Add(qc);
            }

            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = message,
                Data = new
                {
                    //IsNewLot = existingStockInRecordLotNumber.Contains(request.LotNumber)
                    isNewLot = request.LotNumber != null ? request.LotNumber != lastLotNumber : false,
                    newLotNumberIdList,
                    qcList
                }
            });
        }

        [HttpPost("records/list")]
        [Authorize]
        public IActionResult ListStockInRecords(ListStockInRecordsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            var validationResult = _listStockInRecordsValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            if (request.CompId == null)
            {
                request.CompId = compId;
            }
            if (request.PaginationCondition.OrderByField == null)
            {
                request.PaginationCondition.OrderByField = "UpdatedAt";
            }

            var (data, pages) = _stockInService.ListStockInRecords(request);
            var stockInRecordVoList = _mapper.Map<List<InStockItemRecordVo>>(data);
            var productIds = stockInRecordVoList.Select(i => i.ProductId).ToList();
            var products = _warehouseProductService.GetProductsByProductIds(productIds);
            stockInRecordVoList.ForEach(vo =>
            {
                var product = products.Where(p => p.ProductId == vo.ProductId).FirstOrDefault();
                vo.GroupIds = product.GroupIds;
                vo.GroupNames = product.GroupNames;
            });



            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = stockInRecordVoList,
                TotalPages = pages
            });
        }

        [HttpPost("acceptItem/batchVerify")]
        [Authorize]
        public IActionResult BatchVerifyAcceptItem(UpdateBatchAcceptItemsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            request.UpdateAcceptItemList.ForEach(item => item.AcceptUserId = userId);
            var isDirectOutStock = false;
            if (memberAndPermissionSetting.CompanyWithUnit.Type == CommonConstants.CompanyType.ORGANIZATION_NOSTOCK)
            {
                isDirectOutStock = true;
            }
            var validationResult = _batchUdateAcceptItemRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var updateAcceptIdList = request.UpdateAcceptItemList.Select(i => i.AcceptId).ToList();
            var existingAcceptItemList = _stockInService.GetAcceptanceItemByAcceptIdList(updateAcceptIdList);
            var existingItemId = existingAcceptItemList.Select(item=>item.ItemId).ToList();
            var existingPurchaseSubItems = _purchaseService.GetPurchaseSubItemByItemIdList(existingItemId);


            var existingAcceptIdList = existingAcceptItemList.Select(i => i.AcceptId).ToList();
            var notExistAcceptIdList = updateAcceptIdList.Except(existingAcceptIdList).ToList();
            if (notExistAcceptIdList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"驗收品項 {string.Join(", ", notExistAcceptIdList)} 不存在"
                });
            }
            if (existingAcceptItemList.Any(i => i.CompId != compId))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            foreach(var existingAcceptItem in existingAcceptItemList)
            {
                var matchedUpdateAcceptItemRequest = request.UpdateAcceptItemList.Find(a => a.AcceptId == existingAcceptItem.AcceptId);

                if (matchedUpdateAcceptItemRequest != null && matchedUpdateAcceptItemRequest.AcceptQuantity + existingAcceptItem.AcceptQuantity > existingAcceptItem.OrderQuantity)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"不可超過訂購數量,已入數量:{existingAcceptItem.AcceptQuantity},訂購數量:{existingAcceptItem.OrderQuantity}"
                    });
                }
            }

            var inStockedAcceptIdList = existingAcceptItemList.Where(i => i.InStockStatus == CommonConstants.PurchaseSubItemReceiveStatus.DONE).Select(i => i.AcceptId).ToList();
            if (inStockedAcceptIdList.Count > 0)
            {

                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"驗收項目 {string.Join(",", inStockedAcceptIdList)} 皆已全入庫"
                });
            }

            var existingAcceptProductIdList = existingAcceptItemList.Select(i => i.ProductId).ToList();
            var existingProductList = _warehouseProductService.GetProductsByProductIdsAndCompId(existingAcceptProductIdList, compId);
            var existingProductIdList = existingProductList.Select(p => p.ProductId).ToList();
            var notExistProductIdList = existingAcceptProductIdList.Except(existingProductIdList).ToList();
            if (notExistProductIdList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"庫存品項 {string.Join(",", notExistProductIdList)} 不存在"
                });
            }




            var updatePurchaseMainIdList = existingAcceptItemList.Select(i => i.PurchaseMainId).ToList();
            var purchaseMainList = _purchaseService.GetPurchaseMainsByMainIdList(updatePurchaseMainIdList);
            var existingPurchaseMainIdList = purchaseMainList.Select(p => p.PurchaseMainId).ToList();
            var notExistPurchaseMainIdList = existingPurchaseMainIdList.Except(existingPurchaseMainIdList).ToList();
            if (notExistPurchaseMainIdList.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"採購單 {string.Join(",", notExistPurchaseMainIdList)} 不存在"
                });
            }

            var updateAcceptItemsList = request.UpdateAcceptItemList;
            List<string> exceedDeadLineRuleIdList = new();
            List<UpdateAcceptItemRequest> notExceedDeadLineRuleRequestList = new();

            foreach (var item in updateAcceptItemsList)
            {
                var matchedExistAcceptItem = existingAcceptItemList.Where(i => i.AcceptId == item.AcceptId).FirstOrDefault();
                var matchedProduct = existingProductList.Where(p => p.ProductId == matchedExistAcceptItem.ProductId).FirstOrDefault();
                if (item.ExpirationDate != null && matchedProduct.DeadlineRule != null)
                {
                    var expirationDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(item.ExpirationDate).Value);
                    // check : 如果今天日期+deallinerule(至少要可以放幾天) > 保存期限，代表保存期前過短
                    if (DateOnly.FromDateTime(DateTime.Now).AddDays(matchedProduct.DeadlineRule.Value) > expirationDate)
                    {
                        exceedDeadLineRuleIdList.Add(item.AcceptId);
                    }
                    else
                    {
                        notExceedDeadLineRuleRequestList.Add(item);
                    }
                }
                else
                {
                    notExceedDeadLineRuleRequestList.Add(item);
                }
            }


            List<dynamic> updateResultDataList = new();
            List<string> newLotNumberIdList = new();
            List<string> failedIdList = new();
            List<Qc> qcList = new();

            // 未超過允收末效的就先入庫
            foreach (var item in notExceedDeadLineRuleRequestList)
            {
                var matchedExistAcceptItem = existingAcceptItemList.Where(i => i.AcceptId == item.AcceptId).FirstOrDefault();
                var matchedProduct = existingProductList.Where(p => p.ProductId == matchedExistAcceptItem.ProductId).FirstOrDefault();
                var matchedPurchaseMain = purchaseMainList.Where(p => p.PurchaseMainId == matchedExistAcceptItem.PurchaseMainId).FirstOrDefault();
                var matchedPurchaseSubItem = existingPurchaseSubItems.Where(s=>s.ItemId==matchedExistAcceptItem.ItemId).FirstOrDefault();

                List<InStockItemRecord> existingStockInRecords = _stockInService.GetInStockRecordsHistory(matchedExistAcceptItem.ProductId, compId).OrderByDescending(item => item.CreatedAt).ToList();
                var lastLotNumber = existingStockInRecords.FirstOrDefault()?.LotNumber;
                if (item.LotNumber != null && item.LotNumber != lastLotNumber)
                {
                    newLotNumberIdList.Add(item.AcceptId);
                }
                var (result, message,qc) = _stockInService.UpdateAcceptItem(matchedPurchaseMain, matchedPurchaseSubItem, matchedExistAcceptItem, item, matchedProduct, compId, memberAndPermissionSetting.Member, isDirectOutStock);
                if (result != true)
                {
                    failedIdList.Add(matchedExistAcceptItem.AcceptId);
                }
                if (qc != null)
                {
                    qcList.Add(qc);
                }
            }

            // 有效日期短於末效,只有第一次才會批次所以不需要判斷user有沒有確認,因為批次超過允收日期的會打單一驗收

            if (exceedDeadLineRuleIdList.Count > 0)
            {
                return Ok(new CommonResponse<dynamic>
                {
                    Result = false, //有超過允收日期的需要確認的,都回false
                    Data = new
                    {
                        isExceedDeadlineRule = true,
                        exceedDeadlineRuleIdList = exceedDeadLineRuleIdList,
                        newLotNumberIdList,
                        qcList
                    }
                });
            }

            return Ok(new CommonResponse<dynamic>
            {
                Result = failedIdList.Count == 0,
                Message = "",
                Data = new
                {
                    newLotNumberIdList,
                    failedIdList
                }
            });
        }

        [HttpPost("acceptItem/print/inStockRecords")]
        [Authorize]
        public IActionResult GetInStockRecordsByAcceptId(GetInStockRecordsByAcceptIdRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var accepItem = _stockInService.GetAcceptanceItemByAcceptId(request.AcceptId);
            if (accepItem == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "驗收項目不存在"
                });
            }
            if (accepItem.CompId!=compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var inStockRecords = _stockInService.GetProductInStockRecordsByAcceptId(accepItem.ItemId);
            var productIdList = inStockRecords.Select(item=>item.ProductId).ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIdList, compId);

            List<InStockRecordForPrint> data =  _mapper.Map<List<InStockRecordForPrint>>(inStockRecords);

            data.ForEach(item =>
            {
                var matchedProduct = products.Where(p=>p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProduct?.Unit;
            });


            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = data,
            });
        }

        [HttpPost("return")]
        [Authorize]
        public IActionResult Return(ReturnRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);

            var validationResult = _returnStockValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var outStockRecord = _stockOutService.GetOutStockRecordById(request.OutStockId);
            if (outStockRecord == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該出庫紀錄不存在"
                });
            }

            if (outStockRecord.ApplyQuantity <= request.ReturnQuantity)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "退庫數量已超過出庫出數量"
                });
            }

            //if (outStockRecord.IsReturned==true)
            //{
            //    return BadRequest(new CommonResponse<dynamic>
            //    {
            //        Result = false,
            //        Message = "該出庫紀錄已經退庫過"
            //    });
            //}
            var product = _warehouseProductService.GetProductByProductId(outStockRecord.ProductId);

            var (result, errorMsg) = _stockInService.Return(outStockRecord, product, memberAndPermissionSetting.Member,request.ReturnQuantity);
            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = errorMsg,
            });
        }

        [HttpPost("listReturnRecords")]
        [Authorize]
        public IActionResult ListReturnRecords(ListReturnRecordsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var validationResult = _listReturnRecordsValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if(request.CompId==null) request.CompId = compId;


            var returnStockRecords = _stockInService.ListReturnRecords(request);
            returnStockRecords = returnStockRecords.OrderByDescending(r=>r.CreatedAt).ToList();
          
            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = returnStockRecords,
            });
        }


        [HttpPost("remind/expired")]
        [Authorize]
        public IActionResult GetRemindExpiredList(GetRemindExpiredListRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            var nearExpiredProductVoList = _stockInService.GetNearExpiredProductList(compId, today, request.PreDeadline);
            
            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = nearExpiredProductVoList,
            });
        }
    }

    public class SupplierAccepItemsVo
    {
        public SupplierVo Supplier { get; set; } = null!;

        public List<AcceptItem> AcceptItems {  get; set; } = new();

    }

    public class SupplierVo
    {
        public int ArrangeSupplierId { get; set; }
        public string? ArrangeSupplierName { get; set; }
    }
}