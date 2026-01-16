using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using stock_api.Auth;
using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using System.Diagnostics;

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
        private readonly ILogger<StockInController> _logger;
        private readonly IValidator<SearchPurchaseAcceptItemRequest> _searchPurchaseAcceptItemValidator;
        private readonly IValidator<UpdateBatchAcceptItemsRequest> _updateBatchAcceptItemsRequestValidator;
        private readonly IValidator<UpdateAcceptItemRequest> _updateAcceptItemRequestValidator;
        private readonly IValidator<UpdateBatchAcceptItemsRequest> _batchUdateAcceptItemRequestValidator;
        private readonly IValidator<ListStockInRecordsRequest> _listStockInRecordsValidator;
        private readonly IValidator<ReturnRequest> _returnStockValidator;
        private readonly IValidator<ListReturnRecordsRequest> _listReturnRecordsValidator;
        private readonly IValidator<UpdateInStockRequest> _updateInStockRequestValidator;
        private readonly IValidator<OwnerStockInRequest> _ownerStockInRequestValidator;

        public StockInController(IMapper mapper, AuthHelpers authHelpers, GroupService groupService, StockInService stockInService, WarehouseProductService warehouseProductService, PurchaseService purchaseService, StockOutService stockOutService, ILogger<StockInController> logger)
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
            _updateInStockRequestValidator = new UpdateInStockRequestValidator();
            _ownerStockInRequestValidator = new OwnerStockInRequestValidator();
            _logger = logger;
        }

        [HttpPost("purchaseAndAcceptItems/list")]
        [Authorize]
        public IActionResult ListPurchases(SearchPurchaseAcceptItemRequest request)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var stepStopwatch = new Stopwatch();

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            request.CompId = compId;

            var validationResult = _searchPurchaseAcceptItemValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            // 判斷是否可以使用 DB 端分頁（沒有 Keywords 和 GroupId 搜尋，且不是 GroupBySupplier 模式）
            bool canUseDbPagination = request.Keywords == null && request.GroupId == null && request.IsGroupBySupplier != true;

            if (canUseDbPagination)
            {
                return ListPurchasesWithDbPagination(request, totalStopwatch, stepStopwatch, compId);
            }

            // 使用原有的程式端分頁邏輯（有 Keywords 或 GroupId 搜尋時）
            return ListPurchasesWithMemoryPagination(request, totalStopwatch, stepStopwatch, compId);
        }

        /// <summary>
        /// 使用 DB 端分頁的查詢邏輯
        /// </summary>
        private IActionResult ListPurchasesWithDbPagination(
            SearchPurchaseAcceptItemRequest request,
            Stopwatch totalStopwatch,
            Stopwatch stepStopwatch,
            string compId)
        {
            // Step 1: 使用 DB 端分頁查詢採購驗收項目
            stepStopwatch.Restart();
            var (purchaseAcceptanceItemsViewList, totalPages, totalItems) = _stockInService.SearchPurchaseAcceptanceItemsWithPagination(request);
            _logger.LogInformation("[ListPurchases-DbPagination] SearchPurchaseAcceptItems: {elapsed}ms, 筆數: {count}, 總筆數: {total}", 
                stepStopwatch.ElapsedMilliseconds, purchaseAcceptanceItemsViewList.Count, totalItems);

            // Step 2: 查詢產品資料並建立 Dictionary
            stepStopwatch.Restart();
            List<string> distinctProductIdList = purchaseAcceptanceItemsViewList.Select(x => x.ProductId).Distinct().ToList();
            List<WarehouseProduct> products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);
            var productDict = products.ToDictionary(p => p.ProductId);
            _logger.LogInformation("[ListPurchases-DbPagination] GetProducts: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, products.Count);

            // Step 3: 建立 PurchaseMain 分組 Map
            stepStopwatch.Restart();
            var purchaseMainIdAndAcceptionItemListMap = purchaseAcceptanceItemsViewList
                .GroupBy(item => item.PurchaseMainId)
                .ToDictionary(g => g.Key, g => g.ToList());
            _logger.LogInformation("[ListPurchases-DbPagination] BuildPurchaseMainMap: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            // Step 4: 查詢採購子項並建立 Dictionary
            stepStopwatch.Restart();
            List<string> distinctItemIdList = purchaseAcceptanceItemsViewList.Select(a => a.ItemId).Distinct().ToList();
            List<PurchaseSubItem> purchaseSubItems = _purchaseService.GetPurchaseSubItemByItemIdList(distinctItemIdList);
            var purchaseSubItemDict = purchaseSubItems.ToDictionary(s => s.ItemId);
            _logger.LogInformation("[ListPurchases-DbPagination] GetPurchaseSubItems: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, purchaseSubItems.Count);

            // Step 5: 建立資料清單
            stepStopwatch.Restart();
            List<PurchaseAcceptItemsVo> data = BuildPurchaseAcceptItemsData(
                purchaseMainIdAndAcceptionItemListMap,
                productDict,
                purchaseSubItemDict,
                request);
            _logger.LogInformation("[ListPurchases-DbPagination] BuildDataList: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, data.Count);

            // Step 6: 移除 OwnerProcess == "NOT_AGREE" 的 AcceptItems
            stepStopwatch.Restart();
            foreach (var vo in data)
            {
                vo.AcceptItems.RemoveAll(a => a.PurchaseSubItem != null && a.PurchaseSubItem.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE);
            }
            data = data.Where(e => e.AcceptItems.Count > 0).ToList();
            _logger.LogInformation("[ListPurchases-DbPagination] FilterAcceptItems: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            // Step 7: 查詢入庫紀錄並建立 Dictionary
            stepStopwatch.Restart();
            var lotNumberBatchList = data.SelectMany(e => e.AcceptItems)
                .Where(a => a.LotNumberBatch != null)
                .Select(a => a.LotNumberBatch)
                .Distinct()
                .ToList();
            var inStockItemRecords = _stockInService.GetInStockItemRecordsByLotNumberBatchList(compId, lotNumberBatchList);
            var inStockRecordDict = inStockItemRecords
                .Where(i => i.LotNumberBatch != null)
                .GroupBy(i => i.LotNumberBatch)
                .ToDictionary(g => g.Key!, g => g.First());
            _logger.LogInformation("[ListPurchases-DbPagination] GetInStockRecords: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, inStockItemRecords.Count);

            // Step 8: 排序 AcceptItems
            stepStopwatch.Restart();
            data.ForEach(e => e.AcceptItems = e.AcceptItems.OrderBy(a => a.ProductCode).ToList());

            // 套用排序邏輯（資料已在 DB 端排序過，這裡保持一致性）
            data = ApplySorting(data, request.PaginationCondition);
            _logger.LogInformation("[ListPurchases-DbPagination] SortAcceptItems: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            // Step 9: 指派 InStockId
            stepStopwatch.Restart();
            AssignInStockIds(data, inStockRecordDict);
            _logger.LogInformation("[ListPurchases-DbPagination] AssignInStockIds: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            _logger.LogInformation("[ListPurchases-DbPagination] Total: {elapsed}ms", totalStopwatch.ElapsedMilliseconds);

            return Ok(new CommonResponse<List<PurchaseAcceptItemsVo>>
            {
                Result = true,
                Data = data,
                TotalPages = totalPages
            });
        }

        /// <summary>
        /// 使用程式端分頁的查詢邏輯（有 Keywords 或 GroupId 搜尋時使用）
        /// </summary>
        private IActionResult ListPurchasesWithMemoryPagination(
            SearchPurchaseAcceptItemRequest request,
            Stopwatch totalStopwatch,
            Stopwatch stepStopwatch,
            string compId)
        {
            // Step 1: 查詢採購驗收項目
            stepStopwatch.Restart();
            List<PurchaseAcceptanceItemsView> purchaseAcceptanceItemsViewList = _stockInService.SearchPurchaseAcceptanceItems(request);
            _logger.LogInformation("[ListPurchases-MemoryPagination] SearchPurchaseAcceptItems: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, purchaseAcceptanceItemsViewList.Count);

            // Step 2: 查詢產品資料並建立 Dictionary
            stepStopwatch.Restart();
            List<string> distinctProductIdList = purchaseAcceptanceItemsViewList.Select(x => x.ProductId).Distinct().ToList();
            List<WarehouseProduct> products = _warehouseProductService.GetProductsByProductIdsAndCompId(distinctProductIdList, compId);
            var productDict = products.ToDictionary(p => p.ProductId);
            _logger.LogInformation("[ListPurchases-MemoryPagination] GetProducts: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, products.Count);

            // Step 3: 過濾並建立 PurchaseMain 分組 Map (使用 GroupBy 取代 ForEach)
            stepStopwatch.Restart();
            purchaseAcceptanceItemsViewList = purchaseAcceptanceItemsViewList
                .Where(i => i.OwnerProcess != CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE).ToList();

            var purchaseMainIdAndAcceptionItemListMap = purchaseAcceptanceItemsViewList
                .GroupBy(item => item.PurchaseMainId)
                .ToDictionary(g => g.Key, g => g.ToList());
            _logger.LogInformation("[ListPurchases-MemoryPagination] BuildPurchaseMainMap: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            // Step 4: 查詢採購子項並建立 Dictionary
            stepStopwatch.Restart();
            List<string> distinctItemIdList = purchaseAcceptanceItemsViewList.Select(a => a.ItemId).Distinct().ToList();
            List<PurchaseSubItem> purchaseSubItems = _purchaseService.GetPurchaseSubItemByItemIdList(distinctItemIdList);
            var purchaseSubItemDict = purchaseSubItems.ToDictionary(s => s.ItemId);
            _logger.LogInformation("[ListPurchases-MemoryPagination] GetPurchaseSubItems: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, purchaseSubItems.Count);

            // Step 5: 建立資料清單
            stepStopwatch.Restart();
            List<PurchaseAcceptItemsVo> data = BuildPurchaseAcceptItemsData(
                purchaseMainIdAndAcceptionItemListMap,
                productDict,
                purchaseSubItemDict,
                request);
            _logger.LogInformation("[ListPurchases-MemoryPagination] BuildDataList: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, data.Count);

            // Step 6: 依 GroupId 過濾
            stepStopwatch.Restart();
            if (request.GroupId != null)
            {
                foreach (var mainAndAccptItems in data)
                {
                    mainAndAccptItems.AcceptItems = mainAndAccptItems.AcceptItems
                        .Where(a => a.PurchaseSubItem != null && a.PurchaseSubItem.GroupIds.Contains(request.GroupId))
                        .ToList();
                }
                data = data.Where(e => e.AcceptItems.Count > 0).ToList();
            }

            // 移除 OwnerProcess == "NOT_AGREE" 的 AcceptItems
            foreach (var vo in data)
            {
                vo.AcceptItems.RemoveAll(a => a.PurchaseSubItem != null && a.PurchaseSubItem.OwnerProcess == CommonConstants.PurchaseMainOwnerProcessStatus.NOT_AGREE);
            }
            _logger.LogInformation("[ListPurchases-MemoryPagination] FilterByGroupId: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            // Step 7: 查詢入庫紀錄並建立 Dictionary
            stepStopwatch.Restart();
            var lotNumberBatchList = data.SelectMany(e => e.AcceptItems)
                .Where(a => a.LotNumberBatch != null)
                .Select(a => a.LotNumberBatch)
                .Distinct()
                .ToList();
            var inStockItemRecords = _stockInService.GetInStockItemRecordsByLotNumberBatchList(compId, lotNumberBatchList);
            var inStockRecordDict = inStockItemRecords
                .Where(i => i.LotNumberBatch != null)
                .GroupBy(i => i.LotNumberBatch)
                .ToDictionary(g => g.Key!, g => g.First());
            _logger.LogInformation("[ListPurchases-MemoryPagination] GetInStockRecords: {elapsed}ms, 筆數: {count}", stepStopwatch.ElapsedMilliseconds, inStockItemRecords.Count);

            // Step 8: 處理 GroupBySupplier 情境
            if (request.IsGroupBySupplier == true)
            {
                stepStopwatch.Restart();
                var result = BuildSupplierGroupedResult(data, inStockRecordDict, request);
                _logger.LogInformation("[ListPurchases-MemoryPagination] BuildSupplierGroupedResult: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);
                _logger.LogInformation("[ListPurchases-MemoryPagination] Total: {elapsed}ms", totalStopwatch.ElapsedMilliseconds);

                return Ok(new CommonResponse<List<SupplierAccepItemsVo>>
                {
                    Result = true,
                    Data = result
                });
            }

            // Step 9: 排序與分頁
            stepStopwatch.Restart();
            data.ForEach(e => e.AcceptItems = e.AcceptItems.OrderBy(a => a.ProductCode).ToList());

            data = ApplySorting(data, request.PaginationCondition);

            // 過濾零數量
            data = data.Where(e =>
            {
                e.AcceptItems.RemoveAll(i => i.OrderQuantity == 0);
                return e.AcceptItems.Count > 0;
            }).ToList();

            var totalItems = data.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);
            data = data
                .Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize)
                .Take(request.PaginationCondition.PageSize)
                .ToList();
            _logger.LogInformation("[ListPurchases-MemoryPagination] SortAndPaginate: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            // Step 10: 指派 InStockId
            stepStopwatch.Restart();
            AssignInStockIds(data, inStockRecordDict);
            _logger.LogInformation("[ListPurchases-MemoryPagination] AssignInStockIds: {elapsed}ms", stepStopwatch.ElapsedMilliseconds);

            _logger.LogInformation("[ListPurchases-MemoryPagination] Total: {elapsed}ms", totalStopwatch.ElapsedMilliseconds);

            return Ok(new CommonResponse<List<PurchaseAcceptItemsVo>>
            {
                Result = true,
                Data = data,
                TotalPages = totalPages
            });
        }

        /// <summary>
        /// 建立採購驗收項目資料清單
        /// </summary>
        private List<PurchaseAcceptItemsVo> BuildPurchaseAcceptItemsData(
            Dictionary<string, List<PurchaseAcceptanceItemsView>> purchaseMainIdAndAcceptionItemListMap,
            Dictionary<string, WarehouseProduct> productDict,
            Dictionary<string, PurchaseSubItem> purchaseSubItemDict,
            SearchPurchaseAcceptItemRequest request)
        {
            List<PurchaseAcceptItemsVo> data = new();

            // 使用 HashSet 提升搜尋效能
            var allItemSupplierNames = purchaseMainIdAndAcceptionItemListMap
                .SelectMany(entry => entry.Value)
                .Select(item => item.ArrangeSupplierName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet();

            bool isSearchSupplierNameKeywords = request.Keywords != null &&
                allItemSupplierNames.Any(name => name != null && name.Contains(request.Keywords));

            foreach (var keyValuePair in purchaseMainIdAndAcceptionItemListMap)
            {
                List<PurchaseAcceptanceItemsView> purchaseAcceptanceItemViewList = keyValuePair.Value;
                PurchaseAcceptItemsVo purchaseAcceptItemsVo = _mapper.Map<PurchaseAcceptItemsVo>(purchaseAcceptanceItemViewList[0]);
                List<AcceptItem> acceptItems = _mapper.Map<List<AcceptItem>>(purchaseAcceptanceItemViewList);

                // 使用 Dictionary.TryGetValue 取代 LINQ Where+FirstOrDefault (O(n) -> O(1))
                foreach (var item in acceptItems)
                {
                    if (productDict.TryGetValue(item.ProductId, out var matchedProduct))
                    {
                        item.Unit = matchedProduct.Unit;
                        item.ProductCode = matchedProduct.ProductCode;
                        item.UDIBatchCode = matchedProduct.UdibatchCode;
                        item.UDICreateCode = matchedProduct.UdicreateCode;
                        item.UDIVerifyDateCode = matchedProduct.UdiverifyDateCode;
                        item.Prod_supplierName = matchedProduct.DefaultSupplierName;
                        item.DeliverFunction = matchedProduct.DeliverFunction;
                        item.DeliverTemperature = matchedProduct.DeliverTemperature;
                        item.SavingFunction = matchedProduct.SavingFunction;
                        item.SavingTemperature = matchedProduct.SavingTemperature;
                        item.ProductModel = matchedProduct.ProductModel;
                        item.OpenDeadline = matchedProduct.OpenDeadline;
                    }

                    if (purchaseSubItemDict.TryGetValue(item.ItemId, out var matchedSubItem))
                    {
                        item.PurchaseSubItem = matchedSubItem;
                    }
                }

                // 判斷是否應該加入結果
                if (ShouldIncludeInResult(purchaseAcceptItemsVo, acceptItems, request, isSearchSupplierNameKeywords))
                {
                    purchaseAcceptItemsVo.AcceptItems = acceptItems;
                    data.Add(purchaseAcceptItemsVo);
                }
            }

            return data;
        }

        /// <summary>
        /// 判斷是否應該將資料加入結果
        /// </summary>
        private bool ShouldIncludeInResult(
            PurchaseAcceptItemsVo purchaseAcceptItemsVo,
            List<AcceptItem> acceptItems,
            SearchPurchaseAcceptItemRequest request,
            bool isSearchSupplierNameKeywords)
        {
            if (request.Keywords == null)
            {
                return true;
            }

            if (!isSearchSupplierNameKeywords &&
                (purchaseAcceptItemsVo.IsContainKeywords(request.Keywords) ||
                 acceptItems.Any(acceptItem => acceptItem.IsContainKeywords(request.Keywords))))
            {
                return true;
            }

            if (isSearchSupplierNameKeywords &&
                acceptItems.Any(acceptItem => acceptItem.IsContainSupplierName(request.Keywords)))
            {
                var filteredAcceptItems = acceptItems
                    .Where(acceptItem => acceptItem.IsContainKeywords(request.Keywords))
                    .ToList();
                return filteredAcceptItems.Count > 1;
            }

            return false;
        }

        /// <summary>
        /// 套用排序邏輯
        /// </summary>
        private List<PurchaseAcceptItemsVo> ApplySorting(List<PurchaseAcceptItemsVo> data, PaginationCondition paginationCondition)
        {
            paginationCondition.OrderByField ??= "ProductCode";

            var orderByField = StringUtils.CapitalizeFirstLetter(paginationCondition.OrderByField);
            bool isDesc = paginationCondition.IsDescOrderBy;

            return orderByField switch
            {
                "ApplyDate" => isDesc
                    ? data.OrderByDescending(item => item.ApplyDate).ToList()
                    : data.OrderBy(item => item.ApplyDate).ToList(),
                "DemandDate" => isDesc
                    ? data.OrderByDescending(item => item.DemandDate).ToList()
                    : data.OrderBy(item => item.DemandDate).ToList(),
                "GroupId" => data.OrderBy(item => item.GroupIds).ToList(),
                _ => data
            };
        }

        /// <summary>
        /// 建立按供應商分組的結果
        /// </summary>
        private List<SupplierAccepItemsVo> BuildSupplierGroupedResult(
            List<PurchaseAcceptItemsVo> data,
            Dictionary<string, InStockItemRecord> inStockRecordDict,
            SearchPurchaseAcceptItemRequest request)
        {
            var allAcceptItemList = data.SelectMany(item => item.AcceptItems).ToList();

            // 使用 GroupBy 取代手動迴圈
            var supplierIdAndAcceptItemListMap = allAcceptItemList
                .GroupBy(item => item.ArrangeSupplierId ?? -1)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 批次查詢 PurchaseMain
            var purchaseMainIdList = allAcceptItemList
                .Where(i => i.PurchaseMainId != null)
                .Select(i => i.PurchaseMainId)
                .Distinct()
                .ToList();

            var allPurchaseMainList = _purchaseService.GetPurchaseMainsByMainIdList(purchaseMainIdList);
            var purchaseMainDict = allPurchaseMainList.ToDictionary(m => m.PurchaseMainId);

            List<SupplierAccepItemsVo> result = new();

            foreach (var keyValuePair in supplierIdAndAcceptItemListMap)
            {
                if (keyValuePair.Value.Count == 0) continue;

                var firstItem = keyValuePair.Value[0];
                var supplierVo = new SupplierVo
                {
                    ArrangeSupplierId = firstItem.ArrangeSupplierId ?? -1,
                    ArrangeSupplierName = firstItem.ArrangeSupplierName,
                };

                // 使用 Dictionary.TryGetValue 取代 LINQ Where+FirstOrDefault
                foreach (var item in keyValuePair.Value)
                {
                    if (item.PurchaseMainId != null && purchaseMainDict.TryGetValue(item.PurchaseMainId, out var matchedPurchaseMain))
                    {
                        item.ApplyDate = matchedPurchaseMain.ApplyDate;
                    }

                    if (item.LotNumberBatch != null && inStockRecordDict.TryGetValue(item.LotNumberBatch, out var matchedInStockItem))
                    {
                        item.InStockId = matchedInStockItem.InStockId;
                    }
                }

                result.Add(new SupplierAccepItemsVo
                {
                    Supplier = supplierVo,
                    AcceptItems = keyValuePair.Value,
                });
            }

            if (request.SupplierId != null)
            {
                result = result.Where(i => i.Supplier.ArrangeSupplierId == request.SupplierId).ToList();
            }

            return result;
        }

        /// <summary>
        /// 指派 InStockId 到 AcceptItems
        /// </summary>
        private void AssignInStockIds(List<PurchaseAcceptItemsVo> data, Dictionary<string, InStockItemRecord> inStockRecordDict)
        {
            foreach (var element in data)
            {
                foreach (var item in element.AcceptItems)
                {
                    if (item.LotNumberBatch != null && inStockRecordDict.TryGetValue(item.LotNumberBatch, out var matchedInStockItem))
                    {
                        item.InStockId = matchedInStockItem.InStockId;
                    }
                }
            }
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
                ApplyDate = purchaseMain != null ? DateOnly.FromDateTime(purchaseMain.ApplyDate) : null,
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
            var (result, message, qc) = _stockInService.UpdateAcceptItem(purchaseMain, purchaseSubItem, existingAcceptItem, request, product, compId, memberAndPermissionSetting.Member, isDirectOutStock);
            if (request.LotNumber != lastLotNumber)
            {
                newLotNumberIdList.Add(request.AcceptId);
            }
            List<Qc> qcList = new();
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
                vo.ProductModel = product.ProductModel;
            });



            return Ok(new CommonResponse<dynamic>
            {
                Result = true,
                Data = stockInRecordVoList,
                TotalPages = pages
            });
        }

        [HttpPost("recordsWithNewLotNumber/list")]
        [Authorize]
        public IActionResult ListStockInRecordsWithNewLotNumber(ListStockInRecordsWithNewLotNumberRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var userId = memberAndPermissionSetting.Member.UserId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }
            if (request.PaginationCondition.OrderByField == null)
            {
                request.PaginationCondition.OrderByField = "CreatedAt";
            }

            var (data, pages) = _stockInService.ListStockInRecordsWithNewLotNumber(request);
            var stockInRecordVoList = _mapper.Map<List<InStockItemRecordNewLotNumberVo>>(data);
            var allProducts = _warehouseProductService.GetAllProducts(request.CompId);
            var noNeedDisplayProducts = allProducts.Where(p => p.IsNeedAcceptProcess == null || p.IsNeedAcceptProcess == false || p.QcType == CommonConstants.QcTypeConstants.NONE).ToList();
            var itemIds = data.Select(i => i.ItemId).Distinct().ToList();
            var subItems = _purchaseService.GetPurchaseSubItemByItemIdList(itemIds);

            foreach (var item in stockInRecordVoList)
            {
                var matchedProduct = allProducts.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                var matchedSubItem = subItems.Where(s => s.ItemId == item.ItemId).FirstOrDefault();
                item.ProductUnit = matchedProduct?.Unit;
                item.SavingFunction = matchedProduct?.SavingFunction;
                item.SavingTemperature = matchedProduct?.SavingTemperature;
                item.ProductModel = matchedProduct?.ProductModel;
                item.OpenDeadline = matchedProduct?.OpenDeadline;
                item.DeadlineRule = matchedProduct?.DeadlineRule;
                item.OrderQuantity = matchedSubItem.Quantity;
            }


            //if(memberAndPermissionSetting.CompanyWithUnit.UnitId== "Changhua-unit")
            //{
            //    // 彰化醫院婉君要求 設定無的不需要顯示
            //    stockInRecordVoList.RemoveAll(vo =>
            //    {
            //        if (noNeedDisplayProducts.Find(p => p.ProductId == vo.ProductId) != null) return true;
            //        return false;
            //    });
            //}

            // 改用IsOnlyDisplayNeedAcceptProcessItems要顯示所有入庫還是只顯示需要驗收的品項
            if (request.IsOnlyDisplayNeedAcceptProcessItems == true)
            {
                stockInRecordVoList.RemoveAll(vo =>
                {
                    if (noNeedDisplayProducts.Find(p => p.ProductId == vo.ProductId) != null) return true;
                    return false;
                });

            }
            

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
            var existingItemId = existingAcceptItemList.Select(item => item.ItemId).ToList();
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

            foreach (var existingAcceptItem in existingAcceptItemList)
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
                var matchedPurchaseSubItem = existingPurchaseSubItems.Where(s => s.ItemId == matchedExistAcceptItem.ItemId).FirstOrDefault();

                List<InStockItemRecord> existingStockInRecords = _stockInService.GetInStockRecordsHistory(matchedExistAcceptItem.ProductId, compId).OrderByDescending(item => item.CreatedAt).ToList();
                var lastLotNumber = existingStockInRecords.FirstOrDefault()?.LotNumber;
                if (item.LotNumber != null && item.LotNumber != lastLotNumber)
                {
                    newLotNumberIdList.Add(item.AcceptId);
                }
                var (result, message, qc) = _stockInService.UpdateAcceptItem(matchedPurchaseMain, matchedPurchaseSubItem, matchedExistAcceptItem, item, matchedProduct, compId, memberAndPermissionSetting.Member, isDirectOutStock);
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
            if (accepItem.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var inStockRecords = _stockInService.GetProductInStockRecordsByAcceptId(accepItem.ItemId);
            var productIdList = inStockRecords.Select(item => item.ProductId).ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIdList, compId);

            List<InStockRecordForPrint> data = _mapper.Map<List<InStockRecordForPrint>>(inStockRecords);

            data.ForEach(item =>
            {
                var matchedProduct = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
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

            if (outStockRecord.ApplyQuantity < request.ReturnQuantity)
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

            var (result, errorMsg) = _stockInService.Return(outStockRecord, product, memberAndPermissionSetting.Member, request.ReturnQuantity);
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
            if (request.CompId == null) request.CompId = compId;


            var returnStockRecords = _stockInService.ListReturnRecords(request);
            returnStockRecords = returnStockRecords.OrderByDescending(r => r.CreatedAt).ToList();

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

        [HttpPost("update")]
        [AuthorizeRoles("1", "3", "5", "7")]
        // TODO: 未來還需考慮若已出庫還可更改嗎? 若可以出庫資料也須跟著更動?
        public IActionResult Update(UpdateInStockRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var validationResult = _updateInStockRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var acceptItem = _stockInService.GetAcceptanceItemByAcceptId(request.AcceptId);
            if (acceptItem == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該筆項目不存在"
                });
            }
            if (acceptItem.CompId != memberAndPermissionSetting.CompanyWithUnit.CompId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }

            var (isSuccessful, errorMsg) = _stockInService.UpdateInStockItem(request, acceptItem);

            return Ok(new CommonResponse<dynamic>
            {
                Result = isSuccessful,
                Message = errorMsg,
            });
        }

        [HttpPost("delete")]
        [Authorize]
        public IActionResult DeleteInStockItem(DeleteInStockRecordRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var inStockItemRecord = _stockInService.GetInStockRecordById(request.InStockId);
            if (inStockItemRecord == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該筆入庫紀錄不存在"
                });
            }
            if (inStockItemRecord.CompId != compId)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (inStockItemRecord.ItemId == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該筆入庫紀錄非經由採購而來,不可取消"
                });
            }

            var outStockRecords = _stockOutService.GetOutStockRecordsByLotNumberBatch(inStockItemRecord.LotNumberBatch);
            if (outStockRecords.Count > 0)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "已有出庫,不可刪除"
                });
            }
            var (isSuccessful, errorMsg) = _stockInService.DeleteInStockRecord(inStockItemRecord);

            return Ok(new CommonResponse<dynamic>
            {
                Result = isSuccessful,
                Message = errorMsg,
            });
        }

        [HttpPost("owner/productDirectIn")]
        [Authorize]
        public IActionResult ProductDirectIn(OwnerStockInRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (memberAndPermissionSetting.CompanyWithUnit.Type != CommonConstants.CompanyType.OWNER)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            request.CompId = compId;

            var validationResult = _ownerStockInRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }

            var product = _warehouseProductService.GetProductByProductId(request.ProductId);
            if (product == null)
            {
                return BadRequest(new CommonResponse<dynamic>
                {
                    Result = false,
                    Message = "該品項不存在"
                });
            }

            var (result, errorMsg,lotNumberBatch) = _stockInService.OwnerStockInService(request, product, memberAndPermissionSetting.Member);

            return Ok(new CommonResponse<dynamic>
            {
                Result = result,
                Message = errorMsg,
                Data = new Dictionary<string, dynamic>
                {
                    {"lotNumberBatch", lotNumberBatch },
                    {"openDeadLine", product.OpenDeadline },
                    {"productCode", product.ProductCode },
                    {"productModel", product.ProductModel },
                    {"groupName", product.GroupNames },
                    {"savingFunction", product.SavingFunction },
                }
            });
           
        }

        public class SupplierAccepItemsVo
        {
            public SupplierVo Supplier { get; set; } = null!;

            public List<AcceptItem> AcceptItems { get; set; } = new();

        }

        public class SupplierVo
        {
            public int ArrangeSupplierId { get; set; }
            public string? ArrangeSupplierName { get; set; }
        }
    }
}