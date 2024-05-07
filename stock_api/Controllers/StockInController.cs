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
    public class StockInController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers;
        private readonly GroupService _groupService;
        private readonly StockInService _stockInService;
        private readonly WarehouseProductService _warehouseProductService;
        private readonly PurchaseService _purchaseService;
        private readonly IValidator<SearchPurchaseAcceptItemRequest> _searchPurchaseAcceptItemValidator;
        private readonly IValidator<UpdateBatchAcceptItemsRequest> _updateBatchAcceptItemsRequestValidator;
        private readonly IValidator<UpdateAcceptItemRequest> _updateAcceptItemRequestValidator;
        private readonly IValidator<ListStockInRecordsRequest> _listStockInRecordsValidator;

        public StockInController(IMapper mapper, AuthHelpers authHelpers, GroupService groupService, StockInService stockInService, WarehouseProductService warehouseProductService, PurchaseService purchaseService)
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
            _listStockInRecordsValidator = new ListStockInRecordsValidator();
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
                        item.UDIBatchCode = matchedProdcut.UdibatchCode;
                        item.UDICreateCode = matchedProdcut.UdicreateCode;
                        item.UDIVerifyDateCode = matchedProdcut.UdiverifyDateCode;
                    }
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
            data = data.OrderByDescending(item => item.ApplyDate).ToList();


            var response = new CommonResponse<List<PurchaseAcceptItemsVo>>
            {
                Result = true,
                Data = data
            };
            return Ok(response);
        }

        [HttpPost("acceptItems/batch/verify")]
        [Authorize]
        public IActionResult VerifyAcceptItems(UpdateBatchAcceptItemsRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;

            request.UpdateAcceptItemList.ForEach(item => { item.AcceptUserId = userId; });

            var validationResult = _updateBatchAcceptItemsRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var acceptIdList = request.UpdateAcceptItemList.Select(i => i.AcceptId).ToList();
            var exsitingAcceptItems = _stockInService.GetAcceptanceItemsByAccepIdList(acceptIdList, compId);
            if (exsitingAcceptItems.Any(i => i.CompId != compId))
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var existingAcceptIdList = exsitingAcceptItems.Select(i => i.AcceptId).ToList();
            var notExistAcceptIdList = acceptIdList.Except(existingAcceptIdList);
            if (notExistAcceptIdList.Any())
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = $"{String.Join(",", notExistAcceptIdList)} 不存在"
                });
            }

            var products = _warehouseProductService.GetProductsByProductIds(exsitingAcceptItems.Select(i => i.ProductId).ToList());
            var purchaseMain = _purchaseService.GetPurchaseMainByMainId(exsitingAcceptItems[0].PurchaseMainId);
            var result = _stockInService.UpdateAccepItems(purchaseMain, exsitingAcceptItems, request.UpdateAcceptItemList, products, compId, memberAndPermissionSetting.Member);

            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
            });
        }

        [HttpPost("acceptItems/search")]
        [Authorize]
        public IActionResult SearchAcceptItem(SearchAcceptItemRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;


            List<AcceptanceItem> acceptanceItems = _stockInService.acceptanceItemsByUdiSerialCode(request.UdiserialCode, compId).Where(i => i.QcStatus == null).ToList();
            var result = acceptanceItems.OrderByDescending(i => i.UpdatedAt).FirstOrDefault();

            // Gary 增加取得 accept 品項連動的 product 資料
            SearchPurchaseAcceptItemRequest subRequest = new SearchPurchaseAcceptItemRequest();
            subRequest.PurchaseMainId = result.PurchaseMainId;
            List<PurchaseAcceptanceItemsView> purchaseAcceptanceItemsViewList = _stockInService.SearchPurchaseAcceptanceItems(subRequest);
            List<string> distinctProductIdList = purchaseAcceptanceItemsViewList.Select(x => x.ProductId).Distinct().ToList();
            List<WarehouseProduct> products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);
            var matchedProdcut = products.Where(p => p.ProductId == result.ProductId).FirstOrDefault();

            AcceptItem resultItem = new AcceptItem();
            resultItem.AcceptId = result.AcceptId;
            resultItem.AcceptQuantity = result.AcceptQuantity;
            resultItem.AcceptUserId = result.AcceptUserId;
            resultItem.LotNumberBatch = result.LotNumberBatch;
            resultItem.LotNumber = result.LotNumber;
            resultItem.ExpirationDate = result.ExpirationDate;
            resultItem.ItemId = result.ItemId;
            resultItem.OrderQuantity = result.OrderQuantity;
            resultItem.PackagingStatus = result.PackagingStatus;
            resultItem.ProductId = result.ProductId;
            resultItem.ProductName = result.ProductName;
            resultItem.ProductSpec = result.ProductSpec;
            resultItem.UdiserialCode = result.UdiserialCode;
            resultItem.QcStatus = result.QcStatus;
            resultItem.CurrentTotalQuantity = result.CurrentTotalQuantity;
            resultItem.Comment = result.Comment;
            resultItem.QcComment = result.QcComment;
            resultItem.DeliverFunction = result.DeliverFunction;
            resultItem.DeliverTemperature = result.DeliverTemperature;
            resultItem.SavingFunction = result.SavingFunction;
            resultItem.SavingTemperature = result.SavingTemperature;

            if (matchedProdcut != null)
            {
                resultItem.Unit = matchedProdcut.Unit;
                resultItem.UDIBatchCode = matchedProdcut.UdibatchCode;
                resultItem.UDICreateCode = matchedProdcut.UdicreateCode;
                resultItem.UDIVerifyDateCode = matchedProdcut.UdiverifyDateCode;
            }

            return Ok(new CommonResponse<AcceptItem>
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
            if(existingAcceptItem.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (existingAcceptItem.QcStatus != null)
            {
                return BadRequest(new CommonResponse<dynamic>{
                    Result = false,
                    Message = $"此驗收單狀態已為{existingAcceptItem.QcStatus},不可重複驗收"
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

            var result = _stockInService.UpdateAccepItem(purchaseMain, existingAcceptItem, request, product, compId, memberAndPermissionSetting.Member);

            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
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

            request.CompId = compId;
            if (request.PaginationCondition.OrderByField == null)
            {
                request.PaginationCondition.OrderByField = "updatedAt";
            }

            var (data,pages) = _stockInService.ListStockInRecords(request);
            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = data,
                TotalPages =pages
            });
        }
    }
}
