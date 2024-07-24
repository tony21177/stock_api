using AutoMapper;
using stock_api.Common.Constant;
using stock_api.Models;
using stock_api.Service.ValueObject;

namespace stock_api.Service
{
    public class QcService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public QcService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }


        public List<UnDoneQcLot> ListUnDoneQcLotList(string compId)
        {
            var needQcProductList = _dbContext.WarehouseProducts.Where(p=>p.IsActive==true&&p.IsNeedAcceptProcess==true&&p.QcType!=CommonConstants.QcTypeConstants.NONE).ToList();    
            var needQcProductIdList = needQcProductList.Select(p=>p.ProductId).ToList();
            var unDoneLotNumberQcInStockRecords = _dbContext.InStockItemRecords.Where(i=>i.CompId==compId&&i.QcTestStatus==CommonConstants.QcTestStatus.NONE).ToList();
            unDoneLotNumberQcInStockRecords = unDoneLotNumberQcInStockRecords.Where(r => needQcProductIdList.Contains(r.ProductId)).ToList();
            List<UnDoneQcLot> unDoneQcLotList = new();
            foreach (var inStockItemRecord in unDoneLotNumberQcInStockRecords)
            {
                var matchedProduct = needQcProductList.Where(p=>p.ProductId == inStockItemRecord.ProductId).FirstOrDefault();
                var unDoneQcLot = new UnDoneQcLot()
                {
                    ProductId = inStockItemRecord.ProductId,
                    ProductCode = inStockItemRecord.ProductCode,
                    ProductName = inStockItemRecord.ProductName,
                    LotNumber = inStockItemRecord.LotNumber,
                    LotNumberBatch = inStockItemRecord.LotNumberBatch,
                    QcType = matchedProduct.QcType,
                    QcTestStatus = inStockItemRecord.QcTestStatus
                };
                unDoneQcLotList.Add(unDoneQcLot);
            }
            return unDoneQcLotList;
        }
    }
}
