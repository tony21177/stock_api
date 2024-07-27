using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stock_api.Common;
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

        public QcController(IMapper mapper, AuthHelpers authHelpers, StockInService stockInService, StockOutService stockOutService, WarehouseProductService warehouseProductService, QcService qcService,PurchaseService purchaseService)
        {
            _mapper = mapper;
            _authHelpers = authHelpers;
            _stockInService = stockInService;
            _stockOutService = stockOutService;
            _warehouseProductService = warehouseProductService;
            _qcService = qcService;
            _purchaseService = purchaseService;
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult ListUnDoneQcLot()
        {
            var memberAndPermissionSetting = _authHelpers.GetMemberAndPermissionSetting(User);
            var compId = memberAndPermissionSetting.CompanyWithUnit.CompId;

            List<UnDoneQcLot> unDoneQcList = _qcService.ListUnDoneQcLotList(compId);
            var distinctLotNumberBatchList = unDoneQcList.Select(e=>e.LotNumberBatch).ToList();
            List<InStockItemRecord> inStockItems = _stockInService.GetInStockRecordByLotNumberBatchList(distinctLotNumberBatchList, compId);
            Dictionary<string, string> lotNumberBatchAndItemIdMap = new Dictionary<string, string>();
            inStockItems.ForEach(i =>
            {
                lotNumberBatchAndItemIdMap.Add(i.LotNumberBatch, i.ItemId);
            });

            var purchaseDetailList = _purchaseService.GetPurchaseDetailListByItemIdList(inStockItems.Select(i => i.ItemId).ToList());
            Dictionary<String,PurchaseDetailView> itemIdAndPurchaseDetailMap = new Dictionary<String,PurchaseDetailView>();
            purchaseDetailList.ForEach(d =>
            {
                itemIdAndPurchaseDetailMap.Add(d.ItemId, d);
            });

            unDoneQcList.ForEach(lot =>
            {
                var matchedInStock = inStockItems.Where(i=>i.LotNumberBatch==lot.LotNumberBatch).FirstOrDefault();
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
            });


            var response = new CommonResponse<List<UnDoneQcLot>>
            {
                Result = true,
                Data = unDoneQcList
            };
            return Ok(response);
        }
    }
}
