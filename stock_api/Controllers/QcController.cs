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

        public QcController(IMapper mapper, AuthHelpers authHelpers, StockInService stockInService,
            StockOutService stockOutService, WarehouseProductService warehouseProductService,
            QcService qcService, PurchaseService purchaseService,QcValidationFlowSettingService qcValidationFlowSettingService
            ,CompanyService companyService,MemberService memberService)
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
        }

        [HttpPost("list")]
        [Authorize]
        public IActionResult ListUnDoneQcLot(ListUnDoneQcLotRequest request)
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;
            if (request.CompId == null)
            {
                request.CompId = compId;
            }

            var (unDoneQcList, totalPages) = _qcService.ListUnDoneQcLotList(request);
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


            var newLotNumberList = _stockInService.GetInStockItemRecordNewLotNumberViews().Where(i => i.IsNewLotNumber == true).ToList();
            var newLotNumberBatchList = _stockInService.GetProductsNewLotNumberBatchList().ToList();

            var lastQcMainList = _qcService.GetLastQcValidationMainsByProductIdList(unDoneQcList.Select(q=>q.ProductId).ToList());

            unDoneQcList.ForEach(lot =>
            {
                var matchedInStock = inStockItems.Where(i => i.LotNumberBatch == lot.LotNumberBatch).FirstOrDefault();
                var matchedLastQcMain = lastQcMainList.Where(i => i.ProductId == lot.ProductId).FirstOrDefault();
                if (matchedLastQcMain != null)
                {
                    lot.LastMainId = matchedLastQcMain.MainId;
                    lot.LastFinalResult = matchedLastQcMain.FinalResult;
                    lot.LastInStockLotNumberBatch = matchedLastQcMain.LotNumberBatch;
                }


                if (lotNumberBatchAndItemIdMap.ContainsKey(lot.LotNumberBatch) && lotNumberBatchAndItemIdMap.ContainsKey(lot.LotNumberBatch)){
                    var matchedItemId = lotNumberBatchAndItemIdMap[lot.LotNumberBatch];
                    // ItemId為null表示非由採購來的,可能是盤點
                    if (matchedItemId != null)
                    {
                        PurchaseDetailView? matchedPurchaseDetail = null;
                        if (itemIdAndPurchaseDetailMap.ContainsKey(matchedItemId))
                        {
                            matchedPurchaseDetail = itemIdAndPurchaseDetailMap[matchedItemId];
                            lot.PurchaseMainId = matchedPurchaseDetail?.PurchaseMainId;
                            lot.ApplyDate = matchedPurchaseDetail?.ApplyDate;
                            lot.InStockId = matchedInStock.InStockId;
                            lot.AcceptedAt = matchedInStock.CreatedAt.Value;
                            lot.AcceptUserId = matchedInStock.UserId;
                            lot.AcceptUserName = matchedInStock.UserName;
                            lot.ProductSpec = matchedPurchaseDetail?.ProductSpec;
                            // 新增：保存期限
                            lot.ExpirationDate = matchedInStock.ExpirationDate;
                            // 新增：前一次入庫的批號 (找出同一產品在當前入庫時間之前的最後一筆入庫紀錄)
                            var productHistory = _stockInService.GetInStockRecordsHistory(matchedInStock.ProductId, matchedInStock.CompId);
                            var prev = productHistory.Where(i => i.CreatedAt < matchedInStock.CreatedAt).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                            if (prev != null)
                            {
                                lot.PrevLotNumber = prev.LotNumber;
                            }
                            else
                            {
                                lot.PrevLotNumber = null;
                            }
                            lot.VerifyAt = matchedInStock.CreatedAt;
                            if (outStockRecordsByLotNumber.Where(i => i.LotNumber == lot.LotNumber).FirstOrDefault() != null)
                            {
                                lot.IsLotNumberOutStock = true;
                            }
                            if (outStockRecordsByLotNumberBatch.Where(i => i.LotNumberBatch == lot.LotNumberBatch).FirstOrDefault() != null)
                            {
                                lot.IsLotNumberBatchOutStock = true;
                            }
                            var productNewLotNumberList = newLotNumberList.Where(i => i.ProductId == lot.ProductId).Select(m => m.LotNumber).ToList();
                            if (!productNewLotNumberList.Contains(lot.LotNumber))
                            {
                                lot.IsNewLotNumber = false;
                            }
                            //var productNewLotNumberBatchList = newLotNumberBatchList.Where(i => i.ProductId == lot.ProductId).Select(m => m.LotNumberBatch).ToList();
                            //if (!productNewLotNumberBatchList.Contains(lot.LotNumberBatch))
                            //{
                            //    lot.IsNewLotNumberBatch = false;
                            //}

                            // 歷史資料批次以前沒出現過
                            if (productHistory.Where(i => i.CreatedAt < lot.InStockTime).Any(p => p.LotNumberBatch == lot.LotNumberBatch))
                            {
                                lot.IsNewLotNumberBatch = false;
                            }
                        }
                    }
                }
            });


            var response = new CommonResponse<List<UnDoneQcLot>>
            {
                Result = true,
                Data = unDoneQcList,
                TotalPages = totalPages,
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
            if (newQcMain.MainId == null)
            {
                newQcMain.MainId = Guid.NewGuid().ToString();
            }
            List<QcValidationDetail> newQcDetailList = _mapper.Map<List<QcValidationDetail>>(request.Details);
            List<QcAcceptanceDetail> newAcceptanceList = _mapper.Map<List<QcAcceptanceDetail>>(request.AcceptanceDetails);
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
            } else {
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


            var (result,erroMsg) = _qcService.CreateQcValidation(newQcMain, newQcDetailList, newAcceptanceList, qcValidationFlowSettingList, inStockItemRecordList[0]);
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

            var newLotNumberList = _stockInService.GetInStockItemRecordNewLotNumberViews().Where(i => i.IsNewLotNumber==true).ToList();
            var newLotNumberBatchList = _stockInService.GetProductsNewLotNumberBatchList().ToList();


            var details = _qcService.GetQcDetailsByMainIdList(qcMainList.Select(m => m.MainId).ToList());
            var acceptanceDetails = _qcService.GetQcAcceptanceDetailsByMainIdList(qcMainList.Select(m => m.MainId).ToList());
            var distinctMainIdList = qcMainList.Select(m => m.MainId).ToList();
            var flows = _qcService.GetQcFlowListWithAgentsByMainIdList(distinctMainIdList);
            var flowLogs = _qcService.GetQcFlowLogsByMainIdList(distinctMainIdList);
            var qcMainWithDetailAndFlowsList = _mapper.Map<List<QcMainWithDetailAndFlows>>(qcMainList);
            var differentInstockIds = qcMainList.Select(m => m.InStockId).Distinct().ToList();
            var inStockRecords = _stockInService.GetInStockRecordsByInStockIdList(differentInstockIds);
            var productIds = qcMainList.Select(m => m.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIds, compId);
            var purchaseMainIds = qcMainList.Where(m => m.PurchaseMainId != null).Select(m => m.PurchaseMainId!).Distinct().ToList();
            var purchases = _purchaseService.GetPurchaseMainsByMainIdList(purchaseMainIds);


            qcMainWithDetailAndFlowsList.ForEach(m =>
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
                var matchedAcceptanceDetailList = acceptanceDetails.Where(d => d.MainId == m.MainId).ToList();
                m.AcceptanceDetails = matchedAcceptanceDetailList;

                var matchedFlows = flows.Where(f => f.MainId == m.MainId).OrderBy(f=>f.Sequence).ToList();
                var matchedFlowLogs = flowLogs.Where(l => l.MainId == m.MainId).OrderByDescending(l=>l.UpdatedAt).ToList();
                m.FlowLogs = matchedFlowLogs;
                m.Flows = matchedFlows;

                var productNewLotNumberList = newLotNumberList.Where(i => i.ProductId == m.ProductId).Select(m=>m.LotNumber).ToList();
                if (!productNewLotNumberList.Contains(m.LotNumber))
                {
                    m.IsNewLotNumber = false;
                }
                
                // 新增：前一次入庫的批號 (找出同一產品在當前入庫時間之前的最後一筆入庫紀錄)
                var productHistory = _stockInService.GetInStockRecordsHistory(m.ProductId, m.CompId);
                var prev = productHistory.Where(i => i.CreatedAt < m.InStockTime).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                if (prev != null)
                {
                    m.PrevLotNumber = prev.LotNumber;
                }
                else
                {
                    m.PrevLotNumber = null;
                }
                m.VerifyAt = m.InStockTime;

                // 歷史資料批次以前沒出現過
                if (productHistory.Where(i=>i.CreatedAt< m.InStockTime).Any(p=>p.LotNumberBatch==m.LotNumberBatch))
                {
                    m.IsNewLotNumberBatch = false;
                }

                var matchedInStockRecord = inStockRecords.Where(i => i.InStockId == m.InStockId).FirstOrDefault();
                m.ExpirationDate = matchedInStockRecord?.ExpirationDate;


                var matchedProduct = products.Where(p => p.ProductId == m.ProductId).FirstOrDefault();
                if (matchedProduct != null)
                {
                    m.GroupIdList = matchedProduct.GroupIds?.Split(',').ToList() ?? new List<string>();
                    m.GroupNameList = matchedProduct.GroupNames?.Split(',').ToList() ?? new List<string>();
                }
                var matchedPurchase = purchases.Where(p => p.PurchaseMainId == m.PurchaseMainId).FirstOrDefault();
                if (matchedPurchase != null)
                {
                    m.ApplyDate = matchedPurchase.ApplyDate;
                }


            });

            
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
        public IActionResult GetFlowsSignedByMy()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;


            var flowsSignedByMe = _qcService.GetFlowsByUserId(memberAndPermissionSetting.Member.UserId);


            var distinctMainIdList = flowsSignedByMe.Select(f => f.MainId).Distinct().ToList();
            var qcMainList = _qcService.GetQcMainsByMainIdList(distinctMainIdList).OrderByDescending(m => m.CreatedAt).ToList();
            var details = _qcService.GetQcDetailsByMainIdList(qcMainList.Select(m => m.MainId).ToList());
            var acceptanceDetails = _qcService.GetQcAcceptanceDetailsByMainIdList(qcMainList.Select(m => m.MainId).ToList());

            var flows = _qcService.GetQcFlowListWithAgentsByMainIdList(distinctMainIdList);
            var flowLogs = _qcService.GetQcFlowLogsByMainIdList(distinctMainIdList);

            List<QcMainWithDetailAndFlows> qcMainWithDetailAndFlowsList = new List<QcMainWithDetailAndFlows>();

            qcMainList = qcMainList.Where(m=>m.CurrentStatus==CommonConstants.QcCurrentStatus.APPLY).ToList();

            var newLotNumberList = _stockInService.GetInStockItemRecordNewLotNumberViews().Where(i => i.IsNewLotNumber == true).ToList();
            var newLotNumberBatchList = _stockInService.GetProductsNewLotNumberBatchList().ToList();

            var differentInstockIds = qcMainList.Select(m => m.InStockId).Distinct().ToList();
            var inStockRecords = _stockInService.GetInStockRecordsByInStockIdList(differentInstockIds);

            var productIds = qcMainList.Select(m => m.ProductId).Distinct().ToList();
            var products = _warehouseProductService.GetProductsByProductIdsAndCompId(productIds, compId);

            var purchaseMainIds = qcMainList.Where(m => m.PurchaseMainId != null).Select(m => m.PurchaseMainId!).Distinct().ToList();
            var purchases = _purchaseService.GetPurchaseMainsByMainIdList(purchaseMainIds);

            qcMainList.ForEach(m =>
            {
                var qcMainWithDetailAndFlows = _mapper.Map<QcMainWithDetailAndFlows>(m);

                var matchedDetails = details.Where(s => s.MainId == m.MainId).OrderBy(s => s.ItemNumber).ToList();
                var matchedAcceptanceDetails = acceptanceDetails.Where(s => s.MainId == m.MainId).OrderBy(s => s.ItemNumber).ToList();
                

                var matchedFlows = flows.Where(f => f.MainId == m.MainId).OrderBy(f => f.Sequence).ToList();
                var matchedFlowLogs = flowLogs.Where(l => l.MainId == m.MainId).OrderBy(l => l.UpdatedAt).ToList();



                qcMainWithDetailAndFlows.AcceptanceDetails = matchedAcceptanceDetails;
                qcMainWithDetailAndFlows.DetailList = matchedDetails;
                qcMainWithDetailAndFlows.Flows = matchedFlows;
                qcMainWithDetailAndFlows.FlowLogs = matchedFlowLogs;
                var productNewLotNumberList = newLotNumberList.Where(i => i.ProductId == m.ProductId).Select(m => m.LotNumber).ToList();
                if (!productNewLotNumberList.Contains(m.LotNumber))
                {
                    qcMainWithDetailAndFlows.IsNewLotNumber = false;
                }
                //var productNewLotNumberBatchList = newLotNumberBatchList.Where(i => i.ProductId == m.ProductId).Select(m => m.LotNumberBatch).ToList();
                //if (!productNewLotNumberBatchList.Contains(m.LotNumberBatch))
                //{
                //    qcMainWithDetailAndFlows.IsNewLotNumberBatch = false;
                //}

                // 新增：前一次入庫的批號 (找出同一產品在當前入庫時間之前的最後一筆入庫紀錄)
                var productHistory = _stockInService.GetInStockRecordsHistory(qcMainWithDetailAndFlows.ProductId, qcMainWithDetailAndFlows.CompId);
                var prev = productHistory.Where(i => i.CreatedAt < qcMainWithDetailAndFlows.InStockTime).OrderByDescending(i => i.CreatedAt).FirstOrDefault();
                if (prev != null)
                {
                    qcMainWithDetailAndFlows.PrevLotNumber = prev.LotNumber;
                }
                else
                {
                    qcMainWithDetailAndFlows.PrevLotNumber = null;
                }

                // 歷史資料批次以前沒出現過
                if (productHistory.Where(i => i.CreatedAt < m.InStockTime).Any(p => p.LotNumberBatch == m.LotNumberBatch))
                {
                    qcMainWithDetailAndFlows.IsNewLotNumberBatch = false;
                }

                var matchedInStockRecord = inStockRecords.Where(i => i.InStockId==qcMainWithDetailAndFlows.InStockId).FirstOrDefault();
                qcMainWithDetailAndFlows.ExpirationDate = matchedInStockRecord?.ExpirationDate;

                qcMainWithDetailAndFlows.VerifyAt = qcMainWithDetailAndFlows.InStockTime;
                var matchedProduct = products.Where(p => p.ProductId == qcMainWithDetailAndFlows.ProductId).FirstOrDefault();
                if (matchedProduct != null)
                {
                    qcMainWithDetailAndFlows.GroupIdList = matchedProduct.GroupIds?.Split(',').ToList() ?? new List<string>();
                    qcMainWithDetailAndFlows.GroupNameList = matchedProduct.GroupNames?.Split(',').ToList() ?? new List<string>();
                }

                var matchedPurchase = purchases.Where(p => p.PurchaseMainId == qcMainWithDetailAndFlows.PurchaseMainId).FirstOrDefault();
                if (matchedPurchase != null)
                {
                    qcMainWithDetailAndFlows.ApplyDate = matchedPurchase.ApplyDate;
                }

                qcMainWithDetailAndFlowsList.Add(qcMainWithDetailAndFlows);

                
            });
            qcMainWithDetailAndFlowsList = qcMainWithDetailAndFlowsList.OrderBy(m => m.UpdatedAt).ToList();


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
    }
}
