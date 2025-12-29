using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using stock_api.Common;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Controllers.Validator;
using stock_api.Models;
using stock_api.Service;
using stock_api.Service.ValueObject;
using stock_api.Utils;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly QcValidationFlowSettingService _qcValidationFlowSettingService;
        private readonly CompanyService _companyService;
        private readonly MemberService _memberService;
        private readonly IValidator<CreateQcRequest> _createQcValidator;
        private readonly IValidator<ListMainWithDetailRequest> _listQcMainWithDetailValidator;
        private readonly IValidator<AnswerQcFlowRequest> _answerQcFlowRequestValidator;
        private readonly IServiceProvider _serviceProvider;

        public QcController(IMapper mapper, AuthHelpers authHelpers, StockInService stockInService,
            StockOutService stockOutService, WarehouseProductService warehouseProductService,
            QcService qcService, PurchaseService purchaseService, QcValidationFlowSettingService qcValidationFlowSettingService,
            CompanyService companyService, MemberService memberService, IServiceProvider serviceProvider)
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
            _qcValidationFlowSettingService = qcValidationFlowSettingService;
            _answerQcFlowRequestValidator = new AnswerQcFlowValidator();
            _companyService = companyService;
            _memberService = memberService;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("list")]
        [Authorize]
        public async Task<IActionResult> ListUnDoneQcLot(ListUnDoneQcLotRequest request)
        {
            var totalSw = Stopwatch.StartNew();

            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }

            // Step: ListUnDoneQcLotList
            var sw = Stopwatch.StartNew();
            var (unDoneQcList, totalPages) = _qcService.ListUnDoneQcLotList(request);
            sw.Stop();
            Log.Information("[ListUnDoneQcLot] ListUnDoneQcLotList elapsed: {ms}ms, resultCount: {count}", sw.ElapsedMilliseconds, unDoneQcList?.Count ?? 0);

            // distinct lists
            sw.Restart();
            var distinctLotNumberBatchList = unDoneQcList.Where(e => e.LotNumberBatch != null).Select(e => e.LotNumberBatch!).Distinct().ToList();
            var distincLotNumberList = unDoneQcList.Where(e => e.LotNumber != null).Select(e => e.LotNumber!).Distinct().ToList();
            var productIds = unDoneQcList.Select(q => q.ProductId).Distinct().ToList();
            sw.Stop();
            Log.Information("[ListUnDoneQcLot] distinct lists computed: {ms}ms, batchCount: {batchCount}, lotCount: {lotCount}, productCount: {pcount}", sw.ElapsedMilliseconds, distinctLotNumberBatchList.Count, distincLotNumberList.Count, productIds.Count);

            // Parallelize independent DB calls and limit new-lot queries to product scope
            sw.Restart();
            var taskInStock = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetInStockRecordByLotNumberBatchList(distinctLotNumberBatchList, compId);
            });
            var taskOutByLot = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockOutService>();
                return s.GetOutStockRecordsByLotNumberList(distincLotNumberList);
            });
            var taskOutByBatch = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockOutService>();
                return s.GetOutStockRecordsByLotNumberBatchList(distinctLotNumberBatchList);
            });
            var taskNewLotNumberViews = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetInStockItemRecordNewLotNumberViewsByProductIds(productIds);
            });
            var taskNewLotNumberBatchViews = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetProductsNewLotNumberBatchListByProductIds(productIds);
            });
            var taskLastQc = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetLastQcValidationMainsByProductIdList(productIds);
            });

            await Task.WhenAll(taskInStock, taskOutByLot, taskOutByBatch, taskNewLotNumberViews, taskNewLotNumberBatchViews, taskLastQc);

            var inStockItems = taskInStock.Result ?? new List<InStockItemRecord>();
            var outStockRecordsByLotNumber = taskOutByLot.Result ?? new List<OutStockRecord>();
            var outStockRecordsByLotNumberBatch = taskOutByBatch.Result ?? new List<OutStockRecord>();
            var newLotNumberList = (taskNewLotNumberViews.Result ?? new List<InStockItemRecordNewLotNumberVew>()).Where(i => i.IsNewLotNumber == true).ToList();
            var newLotNumberBatchList = taskNewLotNumberBatchViews.Result ?? new List<ProductNewLotnumberbatchView>();
            var lastQcMainList = taskLastQc.Result ?? new List<QcValidationMain>();
            sw.Stop();
            Log.Information("[ListUnDoneQcLot] parallel DB calls elapsed: {ms}ms", sw.ElapsedMilliseconds);

            // map itemId by batch
            sw.Restart();
            var lotNumberBatchAndItemIdMap = inStockItems.Where(i => i.LotNumberBatch != null && i.ItemId != null)
                .GroupBy(i => i.LotNumberBatch)
                .ToDictionary(g => g.Key!, g => g.First().ItemId!);
            sw.Stop();
            Log.Information("[ListUnDoneQcLot] Build lotNumberBatchAndItemIdMap elapsed: {ms}ms, mapSize: {size}", sw.ElapsedMilliseconds, lotNumberBatchAndItemIdMap.Count);

            // purchase details (depends on inStock items -> itemIds)
            sw.Restart();
            var itemIdList = inStockItems.Select(i => i.ItemId).Where(id => id != null).Distinct().ToList()!;
            var purchaseDetailList = itemIdList.Count > 0 ? _purchaseService.GetPurchaseDetailListByItemIdList(itemIdList) : new List<PurchaseDetailView>();
            sw.Stop();
            Log.Information("[ListUnDoneQcLot] GetPurchaseDetailListByItemIdList elapsed: {ms}ms, count: {count}", sw.ElapsedMilliseconds, purchaseDetailList?.Count ?? 0);

            // Build lookups
            sw.Restart();
            var itemIdAndPurchaseDetailMap = purchaseDetailList.Where(d => d.ItemId != null).ToDictionary(d => d.ItemId!, d => d);
            var inStockByBatch = inStockItems.Where(i => i.LotNumberBatch != null).ToDictionary(i => i.LotNumberBatch!, i => i);
            var lastQcByProduct = lastQcMainList.Where(x => x != null).ToDictionary(x => x!.ProductId, x => x!);
            var outStockLotNumberSet = new HashSet<string>(outStockRecordsByLotNumber.Where(o => !string.IsNullOrEmpty(o.LotNumber)).Select(o => o.LotNumber!));
            var outStockLotNumberBatchSet = new HashSet<string>(outStockRecordsByLotNumberBatch.Where(o => !string.IsNullOrEmpty(o.LotNumberBatch)).Select(o => o.LotNumberBatch!));
            var newLotPairsByProduct = newLotNumberList.GroupBy(n => n.ProductId)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => (x.LotNumber ?? "") + "|" + (x.LotNumberBatch ?? ""))));
            var newLotBatchPairsByProduct = newLotNumberBatchList.GroupBy(n => n.ProductId)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => (x.LotNumber ?? "") + "|" + (x.LotNumberBatch ?? ""))));
            sw.Stop();
            Log.Information("[ListUnDoneQcLot] Build lookups elapsed: {ms}ms", sw.ElapsedMilliseconds);

            // prefetch product histories once per productId to reduce DB calls inside loop
            var productIdsForHistory = inStockItems.Select(i => i.ProductId).Distinct().ToList();
            var productHistoryMap = new Dictionary<string, List<InStockItemRecord>>();
            if (productIdsForHistory.Count > 0)
            {
                try
                {
                    var batched = _stockInService.GetInStockRecordsHistoryByProductIdList(productIdsForHistory, compId);
                    foreach (var pid in productIdsForHistory)
                    {
                        if (batched.TryGetValue(pid, out var list)) productHistoryMap[pid] = list ?? new List<InStockItemRecord>();
                        else productHistoryMap[pid] = new List<InStockItemRecord>();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to prefetch histories in batch for products");
                    foreach (var pid in productIdsForHistory) productHistoryMap[pid] = new List<InStockItemRecord>();
                }
            }

            // heavy per-lot processing (optimized)
            sw.Restart();
            foreach (var lot in unDoneQcList)
            {
                inStockByBatch.TryGetValue(lot.LotNumberBatch ?? string.Empty, out var matchedInStock);
                lastQcByProduct.TryGetValue(lot.ProductId, out var matchedLastQcMain);
                if (matchedLastQcMain != null)
                {
                    lot.LastMainId = matchedLastQcMain.MainId;
                    lot.LastFinalResult = matchedLastQcMain.FinalResult;
                    lot.LastInStockLotNumberBatch = matchedLastQcMain.LotNumberBatch;
                }

                if (!string.IsNullOrEmpty(lot.LotNumberBatch) && lotNumberBatchAndItemIdMap.ContainsKey(lot.LotNumberBatch))
                {
                    var matchedItemId = lotNumberBatchAndItemIdMap[lot.LotNumberBatch];
                    if (matchedItemId != null && itemIdAndPurchaseDetailMap.ContainsKey(matchedItemId) && matchedInStock != null)
                    {
                        var matchedPurchaseDetail = itemIdAndPurchaseDetailMap[matchedItemId];
                        lot.PurchaseMainId = matchedPurchaseDetail?.PurchaseMainId;
                        lot.ApplyDate = matchedPurchaseDetail?.ApplyDate;
                        lot.InStockId = matchedInStock.InStockId;
                        lot.AcceptedAt = matchedInStock.CreatedAt.Value;
                        lot.AcceptUserId = matchedInStock.UserId;
                        lot.AcceptUserName = matchedInStock.UserName;
                        lot.ProductSpec = matchedPurchaseDetail?.ProductSpec;
                        lot.ExpirationDate = matchedInStock.ExpirationDate;

                        // previous lot from pre-fetched history
                        if (productHistoryMap.TryGetValue(matchedInStock.ProductId, out var productHistory))
                        {
                            var prev = productHistory.Where(i => i.CreatedAt < matchedInStock.CreatedAt).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                            lot.PrevLotNumber = prev?.LotNumber;
                        }

                        lot.VerifyAt = matchedInStock.CreatedAt;

                        if (!string.IsNullOrEmpty(lot.LotNumber) && outStockLotNumberSet.Contains(lot.LotNumber))
                        {
                            lot.IsLotNumberOutStock = true;
                        }
                        if (!string.IsNullOrEmpty(lot.LotNumberBatch) && outStockLotNumberBatchSet.Contains(lot.LotNumberBatch))
                        {
                            lot.IsLotNumberBatchOutStock = true;
                        }

                        var pairKey = (lot.LotNumber ?? "") + "|" + (lot.LotNumberBatch ?? "");
                        if (newLotPairsByProduct.TryGetValue(lot.ProductId, out var newPairs) && !newPairs.Contains(pairKey))
                        {
                            lot.IsNewLotNumber = false;
                        }
                        if (newLotBatchPairsByProduct.TryGetValue(lot.ProductId, out var newBatchPairs) && !newBatchPairs.Contains(pairKey))
                        {
                            lot.IsNewLotNumberBatch = false;
                        }
                    }
                }
            }
            sw.Stop();
            Log.Information("[ListUnDoneQcLot] per-lot processing elapsed: {ms}ms", sw.ElapsedMilliseconds);

            var response = new CommonResponse<List<UnDoneQcLot>>
            {
                Result = true,
                Data = unDoneQcList,
                TotalPages = totalPages,
            };

            totalSw.Stop();
            Log.Information("[ListUnDoneQcLot] TOTAL elapsed: {ms}ms", totalSw.ElapsedMilliseconds);

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
            if (newQcMain.MainId == null)
            {
                newQcMain.MainId = Guid.NewGuid().ToString();
            }
            List<QcValidationDetail> newQcDetailList = _mapper.Map<List<QcValidationDetail>>(request.Details);
            List<QcAcceptanceDetail> newAcceptanceList = _mapper.Map<List<QcAcceptanceDetail>>(request.AcceptanceDetails);
            List<InStockItemRecord> inStockItemRecordList = new List<InStockItemRecord>();
            if (!string.IsNullOrEmpty(request.LotNumber))
            {
                inStockItemRecordList = _stockInService.GetInStockRecordListByLotNumber(request.LotNumber, compId).OrderByDescending(i => i.CreatedAt).ToList();
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
                if (inStockItemRecord == null)
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
            WarehouseProduct product = _warehouseProductService.GetProductByProductId(inStockItemRecordList[0].ProductId);

            // 審核流程
            List<string> groupIds = product.GroupIds?.Split(',').ToList() ?? new List<string>();
            List<QcValidationFlowSettingVo> qcValidationFlowSettingList = new();
            bool isGroupCrossGroup = (groupIds.Count() > 1);
            if (isGroupCrossGroup == true || groupIds.Count == 0)
            {
                // 拉取不指定組別的審核流程
                var crossCompFlowSettings = _qcValidationFlowSettingService.GeQcValidationFlowSettingVoListByCompIdForCrossComp(compId);
                if (crossCompFlowSettings.Count == 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"尚未建立 {product.ProductCode}({product.ProductName}) 的品質確效跨組別審核流程關卡"
                    });
                }
                qcValidationFlowSettingList.AddRange(crossCompFlowSettings);
            }
            else
            {
                var groupFlowSettings = _qcValidationFlowSettingService.GeQcValidationFlowSettingVoListByGroupId(groupIds[0]);
                if (groupFlowSettings.Count == 0)
                {
                    return BadRequest(new CommonResponse<dynamic>
                    {
                        Result = false,
                        Message = $"尚未建立 {product.ProductCode}({product.ProductName}) 所屬組別 {product.GroupNames.Split(",")[0]} 的品質確效組別審核流程關卡"
                    });
                }
                qcValidationFlowSettingList.AddRange(groupFlowSettings);
            }

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
            newQcMain.ProductModel = product.ProductModel;
            newQcMain.TestUserId = memberAndPermissionSetting.Member.UserId;
            newQcMain.TestUserName = memberAndPermissionSetting.Member.DisplayName;

            newQcDetailList.ForEach(detail => detail.MainId = newQcMain.MainId);
            newAcceptanceList.ForEach(detail => detail.MainId = newQcMain.MainId);

            var (result, erroMsg) = _qcService.CreateQcValidation(newQcMain, newQcDetailList, newAcceptanceList, qcValidationFlowSettingList, inStockItemRecordList[0]);
            var response = new CommonResponse<List<UnDoneQcLot>>
            {
                Result = result,
                Message = erroMsg
            };
            return Ok(response);
        }

        [HttpPost("mainWithDetail/list")]
        [Authorize]
        public async Task<IActionResult> ListMainWithDetail([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ListMainWithDetailRequest? request)
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
            var (qcMainList, totalPages) = _qcService.ListQcMain(request);

            // compute distinct keys and product ids
            var distinctLotNumberList = qcMainList.Where(qc => qc.LotNumber != null).Select(qc => qc.LotNumber!).Distinct().ToList();
            var distinctLotNumberBatchList = qcMainList.Where(qc => qc.LotNumberBatch != null).Select(qc => qc.LotNumberBatch!).Distinct().ToList();
            var productIds = qcMainList.Select(m => m.ProductId).Distinct().ToList();
            var distinctMainIdList = qcMainList.Select(m => m.MainId).Distinct().ToList();
            var differentInstockIds = qcMainList.Select(m => m.InStockId).Distinct().ToList();

            // Parallelize independent DB calls and restrict new-lot queries to productIds
            var taskOutByLot = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockOutService>();
                return s.GetOutStockRecordsByLotNumberList(distinctLotNumberList);
            });
            var taskOutByBatch = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockOutService>();
                return s.GetOutStockRecordsByLotNumberBatchList(distinctLotNumberBatchList);
            });
            var taskNewLotNumberViews = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetInStockItemRecordNewLotNumberViewsByProductIds(productIds);
            });
            var taskNewLotNumberBatchViews = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetProductsNewLotNumberBatchListByProductIds(productIds);
            });
            var taskDetails = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcDetailsByMainIdList(distinctMainIdList);
            });
            var taskAcceptanceDetails = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcAcceptanceDetailsByMainIdList(distinctMainIdList);
            });
            var taskFlows = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcFlowListWithAgentsByMainIdList(distinctMainIdList);
            });
            var taskFlowLogs = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcFlowLogsByMainIdList(distinctMainIdList);
            });
            var taskInStockRecords = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetInStockRecordsByInStockIdList(differentInstockIds);
            });
            var taskProducts = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<WarehouseProductService>();
                return s.GetProductsByProductIdsAndCompId(productIds, compId);
            });
            var taskPurchases = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<PurchaseService>();
                var purchaseMainIds = qcMainList.Where(m => m.PurchaseMainId != null).Select(m => m.PurchaseMainId!).Distinct().ToList();
                return s.GetPurchaseMainsByMainIdList(purchaseMainIds);
            });

            await Task.WhenAll(taskOutByLot, taskOutByBatch, taskNewLotNumberViews, taskNewLotNumberBatchViews, taskDetails, taskAcceptanceDetails, taskFlows, taskFlowLogs, taskInStockRecords, taskProducts, taskPurchases);

            var outStockRecordsByLotNumber = taskOutByLot.Result ?? new List<OutStockRecord>();
            var outStockRecordsByLotNumberBatch = taskOutByBatch.Result ?? new List<OutStockRecord>();
            var newLotNumberList = (taskNewLotNumberViews.Result ?? new List<InStockItemRecordNewLotNumberVew>()).Where(i => i.IsNewLotNumber == true).ToList();
            var newLotNumberBatchList = taskNewLotNumberBatchViews.Result ?? new List<ProductNewLotnumberbatchView>();
            var details = taskDetails.Result ?? new List<QcValidationDetail>();
            var acceptanceDetails = taskAcceptanceDetails.Result ?? new List<QcAcceptanceDetail>();
            var flows = taskFlows.Result ?? new List<QcFlowWithAgentsVo>();
            var flowLogs = taskFlowLogs.Result ?? new List<QcFlowLog>();
            var inStockRecords = taskInStockRecords.Result ?? new List<InStockItemRecord>();
            var products = taskProducts.Result ?? new List<WarehouseProduct>();
            var purchases = taskPurchases.Result ?? new List<PurchaseMainSheet>();

            // Build lookup dictionaries to avoid repeated scans
            var detailsByMain = details.GroupBy(d => d.MainId).ToDictionary(g => g.Key, g => g.ToList());
            var acceptanceByMain = acceptanceDetails.GroupBy(d => d.MainId).ToDictionary(g => g.Key, g => g.ToList());
            var flowsByMain = flows.GroupBy(f => f.MainId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Sequence).ToList());
            var flowLogsByMain = flowLogs.GroupBy(l => l.MainId).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.UpdatedAt).ToList());
            var inStockById = inStockRecords.GroupBy(i => i.InStockId).ToDictionary(g => g.Key, g => g.First());
            var productsById = products.ToDictionary(p => p.ProductId);
            var purchasesById = purchases.ToDictionary(p => p.PurchaseMainId);
            var outStockLotNumberSet = new HashSet<string>(outStockRecordsByLotNumber.Where(o => !string.IsNullOrEmpty(o.LotNumber)).Select(o => o.LotNumber!));
            var outStockLotNumberBatchSet = new HashSet<string>(outStockRecordsByLotNumberBatch.Where(o => !string.IsNullOrEmpty(o.LotNumberBatch)).Select(o => o.LotNumberBatch!));

            var productNewLotPairs = newLotNumberList.GroupBy(n => n.ProductId)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => (x.LotNumber ?? "") + "|" + (x.LotNumberBatch ?? ""))));
            var productNewLotBatchPairs = newLotNumberBatchList.GroupBy(n => n.ProductId)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => (x.LotNumber ?? "") + "|" + (x.LotNumberBatch ?? ""))));

            // Prefetch product histories once per productId
            var productIdsForHistory = productIds; // already distinct
            var productHistoryMap = new Dictionary<string, List<InStockItemRecord>>();
            if (productIdsForHistory.Count > 0)
            {
                try
                {
                    var batched = _stockInService.GetInStockRecordsHistoryByProductIdList(productIdsForHistory, compId);
                    foreach (var pid in productIdsForHistory)
                    {
                        if (batched.TryGetValue(pid, out var list)) productHistoryMap[pid] = list ?? new List<InStockItemRecord>();
                        else productHistoryMap[pid] = new List<InStockItemRecord>();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to prefetch histories in batch for products");
                    foreach (var pid in productIdsForHistory) productHistoryMap[pid] = new List<InStockItemRecord>();
                }
            }

            var qcMainWithDetailAndFlowsList = _mapper.Map<List<QcMainWithDetailAndFlows>>(qcMainList);

            // Fill mapped objects using dict lookups
            foreach (var m in qcMainWithDetailAndFlowsList)
            {
                detailsByMain.TryGetValue(m.MainId, out var matchedDetails);
                m.DetailList = matchedDetails ?? new List<QcValidationDetail>();

                acceptanceByMain.TryGetValue(m.MainId, out var matchedAcceptance);
                m.AcceptanceDetails = matchedAcceptance ?? new List<QcAcceptanceDetail>();

                if (outStockLotNumberSet.Contains(m.LotNumber ?? string.Empty)) m.IsLotNumberOutStock = true;
                if (outStockLotNumberBatchSet.Contains(m.LotNumberBatch ?? string.Empty)) m.IsLotNumberBatchOutStock = true;

                flowsByMain.TryGetValue(m.MainId, out var matchedFlows);
                flowLogsByMain.TryGetValue(m.MainId, out var matchedFlowLogs);
                m.Flows = matchedFlows ?? new List<QcFlowWithAgentsVo>();
                m.FlowLogs = matchedFlowLogs ?? new List<QcFlowLog>();

                // set member auth values for flow reviewers (batch lookup of members)
                // gather reviewer ids from flows list later to reduce separate DB call; build members map once
            }

            // Batch get members for flows' reviewUserIds
            var reviewUserIds = qcMainWithDetailAndFlowsList.SelectMany(x => x.Flows).Select(f => f.ReviewUserId).Distinct().ToList();
            var members = reviewUserIds.Count > 0 ? _memberService.GetMembersByUserIdList(reviewUserIds) : new List<WarehouseMember>();
            var membersById = members.ToDictionary(mem => mem.UserId, mem => mem);

            // Fill reviewer auth values and other fields using dictionaries
            foreach (var m in qcMainWithDetailAndFlowsList)
            {
                foreach (var flow in m.Flows)
                {
                    if (membersById.TryGetValue(flow.ReviewUserId, out var member))
                    {
                        flow.ReviewUserAuthValue = member.AuthValue;
                    }
                }

                // new lot checks
                var pairKey = (m.LotNumber ?? "") + "|" + (m.LotNumberBatch ?? "");
                if (productNewLotPairs.TryGetValue(m.ProductId, out var newPairs) && !newPairs.Contains(pairKey)) m.IsNewLotNumber = false;
                if (productNewLotBatchPairs.TryGetValue(m.ProductId, out var newBatchPairs) && !newBatchPairs.Contains(pairKey)) m.IsNewLotNumberBatch = false;

                // prev lot from pre-fetched history
                if (productHistoryMap.TryGetValue(m.ProductId, out var history))
                {
                    var prev = history.Where(i => i.CreatedAt < m.InStockTime).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                    m.PrevLotNumber = prev?.LotNumber;
                }
                else
                {
                    m.PrevLotNumber = null;
                }

                m.VerifyAt = m.InStockTime;

                if (inStockById.TryGetValue(m.InStockId, out var matchedInStockRecord))
                {
                    m.ExpirationDate = matchedInStockRecord?.ExpirationDate;
                }

                if (productsById.TryGetValue(m.ProductId, out var matchedProduct))
                {
                    m.GroupIdList = matchedProduct.GroupIds?.Split(',').ToList() ?? new List<string>();
                    m.GroupNameList = matchedProduct.GroupNames?.Split(',').ToList() ?? new List<string>();
                }

                if (!string.IsNullOrEmpty(m.PurchaseMainId) && purchasesById.TryGetValue(m.PurchaseMainId, out var matchedPurchase))
                {
                    m.ApplyDate = matchedPurchase.ApplyDate;
                }
            }

            var response = new CommonResponse<List<QcMainWithDetailAndFlows>>
            {
                Result = true,
                Data = qcMainWithDetailAndFlowsList,
                TotalPages = totalPages
            };
            return Ok(response);
        }

        [HttpGet("flows/my")]
        [Authorize]
        public async Task<IActionResult> GetFlowsSignedByMy()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            var memberId = memberAndPermissionSetting.Member.UserId;

            // Step 1: get flows signed by me
            var flowsSignedByMe = _qcService.GetFlowsByUserId(memberId);
            var distinctMainIdList = flowsSignedByMe.Select(f => f.MainId).Distinct().ToList();

            // Step 2: get main list and filter
            var qcMainList = _qcService.GetQcMainsByMainIdList(distinctMainIdList).OrderByDescending(m => m.CreatedAt).ToList();
            qcMainList = qcMainList.Where(m => m.CurrentStatus == CommonConstants.QcCurrentStatus.APPLY).ToList();

            // Prepare ids
            var productIds = qcMainList.Select(m => m.ProductId).Distinct().ToList();
            var differentInstockIds = qcMainList.Select(m => m.InStockId).Distinct().ToList();
            var purchaseMainIds = qcMainList.Where(m => m.PurchaseMainId != null).Select(m => m.PurchaseMainId!).Distinct().ToList();
            var distinctMainIdsForQuery = qcMainList.Select(m => m.MainId).Distinct().ToList();

            // Parallelize independent DB calls using scope-per-task
            var taskNewLotNumberViews = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetInStockItemRecordNewLotNumberViewsByProductIds(productIds);
            });

            var taskNewLotNumberBatchViews = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetProductsNewLotNumberBatchListByProductIds(productIds);
            });

            var taskDetails = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcDetailsByMainIdList(distinctMainIdsForQuery);
            });

            var taskAcceptanceDetails = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcAcceptanceDetailsByMainIdList(distinctMainIdsForQuery);
            });

            var taskFlowsWithAgents = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcFlowListWithAgentsByMainIdList(distinctMainIdsForQuery);
            });

            var taskFlowLogs = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<QcService>();
                return s.GetQcFlowLogsByMainIdList(distinctMainIdsForQuery);
            });

            var taskInStockRecords = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<StockInService>();
                return s.GetInStockRecordsByInStockIdList(differentInstockIds);
            });

            var taskProducts = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<WarehouseProductService>();
                return s.GetProductsByProductIdsAndCompId(productIds, compId);
            });

            var taskPurchases = Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var s = scope.ServiceProvider.GetRequiredService<PurchaseService>();
                return s.GetPurchaseMainsByMainIdList(purchaseMainIds);
            });

            await Task.WhenAll(taskNewLotNumberViews, taskNewLotNumberBatchViews, taskDetails, taskAcceptanceDetails, taskFlowsWithAgents, taskFlowLogs, taskInStockRecords, taskProducts, taskPurchases);

            var newLotNumberList = (taskNewLotNumberViews.Result ?? new List<InStockItemRecordNewLotNumberVew>()).Where(i => i.IsNewLotNumber == true).ToList();
            var newLotNumberBatchList = taskNewLotNumberBatchViews.Result ?? new List<ProductNewLotnumberbatchView>();
            var details = taskDetails.Result ?? new List<QcValidationDetail>();
            var acceptanceDetails = taskAcceptanceDetails.Result ?? new List<QcAcceptanceDetail>();
            var flows = taskFlowsWithAgents.Result ?? new List<QcFlowWithAgentsVo>();
            var flowLogs = taskFlowLogs.Result ?? new List<QcFlowLog>();
            var inStockRecords = taskInStockRecords.Result ?? new List<InStockItemRecord>();
            var products = taskProducts.Result ?? new List<WarehouseProduct>();
            var purchases = taskPurchases.Result ?? new List<PurchaseMainSheet>();

            // Build lookup dictionaries
            var detailsByMain = details.GroupBy(d => d.MainId).ToDictionary(g => g.Key, g => g.ToList());
            var acceptanceByMain = acceptanceDetails.GroupBy(d => d.MainId).ToDictionary(g => g.Key, g => g.ToList());
            var flowsByMain = flows.GroupBy(f => f.MainId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Sequence).ToList());
            var flowLogsByMain = flowLogs.GroupBy(l => l.MainId).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.UpdatedAt).ToList());
            var inStockById = inStockRecords.GroupBy(i => i.InStockId).ToDictionary(g => g.Key, g => g.First());
            var productsById = products.ToDictionary(p => p.ProductId);
            var purchasesById = purchases.ToDictionary(p => p.PurchaseMainId);

            var productNewLotPairs = newLotNumberList.GroupBy(n => n.ProductId)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => (x.LotNumber ?? "") + "|" + (x.LotNumberBatch ?? ""))));
            var productNewLotBatchPairs = newLotNumberBatchList.GroupBy(n => n.ProductId)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => (x.LotNumber ?? "") + "|" + (x.LotNumberBatch ?? ""))));

            // Prefetch product histories
            var productHistoryMap = new Dictionary<string, List<InStockItemRecord>>();
            if (productIds.Count > 0)
            {
                try
                {
                    var batched = _stockInService.GetInStockRecordsHistoryByProductIdList(productIds, compId);
                    foreach (var pid in productIds)
                    {
                        if (batched.TryGetValue(pid, out var list)) productHistoryMap[pid] = list ?? new List<InStockItemRecord>();
                        else productHistoryMap[pid] = new List<InStockItemRecord>();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to prefetch histories in batch for products");
                    foreach (var pid in productIds) productHistoryMap[pid] = new List<InStockItemRecord>();
                }
            }

            var qcMainWithDetailAndFlowsList = _mapper.Map<List<QcMainWithDetailAndFlows>>(qcMainList);

            // Batch get members for flows' reviewer ids
            var reviewUserIds = flows.Select(f => f.ReviewUserId).Distinct().ToList();
            var members = reviewUserIds.Count > 0 ? _memberService.GetMembersByUserIdList(reviewUserIds) : new List<WarehouseMember>();
            var membersById = members.ToDictionary(mem => mem.UserId, mem => mem);

            // Fill mapped objects
            foreach (var m in qcMainWithDetailAndFlowsList)
            {
                detailsByMain.TryGetValue(m.MainId, out var matchedDetails);
                m.DetailList = matchedDetails ?? new List<QcValidationDetail>();

                acceptanceByMain.TryGetValue(m.MainId, out var matchedAcceptance);
                m.AcceptanceDetails = matchedAcceptance ?? new List<QcAcceptanceDetail>();

                flowsByMain.TryGetValue(m.MainId, out var matchedFlows);
                flowLogsByMain.TryGetValue(m.MainId, out var matchedFlowLogs);
                m.Flows = matchedFlows ?? new List<QcFlowWithAgentsVo>();
                m.FlowLogs = matchedFlowLogs ?? new List<QcFlowLog>();

                // set reviewer auth
                foreach (var flow in m.Flows)
                {
                    if (membersById.TryGetValue(flow.ReviewUserId, out var member)) flow.ReviewUserAuthValue = member.AuthValue;
                }

                // new lot checks
                var pairKey = (m.LotNumber ?? "") + "|" + (m.LotNumberBatch ?? "");
                if (productNewLotPairs.TryGetValue(m.ProductId, out var newPairs) && !newPairs.Contains(pairKey)) m.IsNewLotNumber = false;
                if (productNewLotBatchPairs.TryGetValue(m.ProductId, out var newBatchPairs) && !newBatchPairs.Contains(pairKey)) m.IsNewLotNumberBatch = false;

                // prev lot
                if (productHistoryMap.TryGetValue(m.ProductId, out var history))
                {
                    var prev = history.Where(i => i.CreatedAt < m.InStockTime).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                    m.PrevLotNumber = prev?.LotNumber;
                }
                else m.PrevLotNumber = null;

                m.VerifyAt = m.InStockTime;

                if (inStockById.TryGetValue(m.InStockId, out var matchedInStockRecord)) m.ExpirationDate = matchedInStockRecord?.ExpirationDate;

                if (productsById.TryGetValue(m.ProductId, out var matchedProduct))
                {
                    m.GroupIdList = matchedProduct.GroupIds?.Split(',').ToList() ?? new List<string>();
                    m.GroupNameList = matchedProduct.GroupNames?.Split(',').ToList() ?? new List<string>();
                }

                if (!string.IsNullOrEmpty(m.PurchaseMainId) && purchasesById.TryGetValue(m.PurchaseMainId, out var matchedPurchase))
                {
                    m.ApplyDate = matchedPurchase.ApplyDate;
                }
            }

            var response = new CommonResponse<List<QcMainWithDetailAndFlows>>
            {
                Result = true,
                Data = qcMainWithDetailAndFlowsList
            };
            return Ok(response);
        }

        [HttpPost("flow/answer")]
        [Authorize]
        public IActionResult FlowSign(AnswerQcFlowRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var verifier = memberAndPermissionSetting.Member;
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            var validationResult = _answerQcFlowRequestValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(CommonResponse<dynamic>.BuildValidationFailedResponse(validationResult));
            }
            var qcFlow = _qcService.GetFlowsByFlowId(request.FlowId);

            if (qcFlow == null)
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "審核流程不存在"
                });
            }

            var qcComp = _companyService.GetCompanyByCompId(qcFlow.CompId);
            if (qcComp == null)
            {
                return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
            }
            if (qcComp.Type != CommonConstants.CompanyType.ORGANIZATION_NOSTOCK || memberAndPermissionSetting.Member.IsNoStockReviewer == false)
            {

                if (qcFlow.CompId != compId)
                {
                    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());
                }
            }


            bool isVerifiedByAgent = false;
            if (qcFlow.ReviewUserId != verifier.UserId)
            {
                // 檢查是否為代理人
                var flowVerifier = _memberService.GetMemberByUserId(qcFlow.ReviewUserId);
                if (flowVerifier.Agents.Contains(verifier.UserId))
                {
                    isVerifiedByAgent = true;
                }
                else
                {
                    return BadRequest(CommonResponse<dynamic>.BuildNotAuthorizeResponse());

                }
            }
            if (!qcFlow.Answer.IsNullOrEmpty())
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "不能重複審核"
                });
            }


            var beforeFlows = _qcService.GetBeforeFlows(qcFlow);
            if (beforeFlows.Any(f => f.Answer == CommonConstants.PurchaseFlowAnswer.EMPTY))
            {
                return BadRequest(new CommonResponse<dynamic>()
                {
                    Result = false,
                    Message = "之前的審核流程還在跑"
                });
            }

            var result = _qcService.AnswerFlow(qcFlow, memberAndPermissionSetting, request.Answer, request.Reason, isVerifiedByAgent);


            var response = new CommonResponse<dynamic>
            {
                Result = result,
                Data = null
            };
            return Ok(response);
        }

        private class LotNumberAndLotNumberBatch
        {
            public string? LotNumber { get; set; }
            public string? LotNumberBatch { get; set; }

            public LotNumberAndLotNumberBatch(string? lotNumber, string? lotNumberBatch)
            {
                LotNumber = lotNumber;
                LotNumberBatch = lotNumberBatch;
            }

            public override bool Equals(object? obj)
            {
                if (obj is LotNumberAndLotNumberBatch other)
                {
                    return LotNumber == other.LotNumber && LotNumberBatch == other.LotNumberBatch;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(LotNumber, LotNumberBatch);
            }
        }
    }
}
