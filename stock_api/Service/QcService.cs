using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Transactions;

namespace stock_api.Service
{
    public class QcService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<QcService> _logger;

        public QcService(StockDbContext dbContext, IMapper mapper, ILogger<QcService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }


        public List<UnDoneQcLot> ListUnDoneQcLotList(string compId)
        {
            var needQcProductList = _dbContext.WarehouseProducts.Where(p=>p.IsActive==true&&p.IsNeedAcceptProcess==true&&p.QcType!=CommonConstants.QcTypeConstants.NONE).ToList();    
            var needQcProductIdList = needQcProductList.Select(p=>p.ProductId).ToList();
            var unDoneLotNumberQcInStockRecords = _dbContext.InStockItemRecords.Where(i=>i.CompId==compId&&i.QcTestStatus==CommonConstants.QcTestStatus.NONE
            &&i.IsNeedQc==true&&i.QcType!=CommonConstants.QcTypeConstants.NONE).ToList();
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

        public (bool,string?) CreateQcValidation(QcValidationMain newQcValidationMain,List<QcValidationDetail> newQcValidationDetailList)
        {
            using var scope = new TransactionScope();
            try
            {
                _dbContext.QcValidationMains.Add(newQcValidationMain);
                _dbContext.QcValidationDetails.AddRange(newQcValidationDetailList);

                if (newQcValidationMain.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER)
                {
                    _dbContext.InStockItemRecords.Where(i => i.IsNeedQc == true && i.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER && i.LotNumber == newQcValidationMain.LotNumber)
                        .ExecuteUpdate(item => item.SetProperty(x => x.IsNeedQc, false));


                }
                if (newQcValidationMain.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH)
                {
                    _dbContext.InStockItemRecords.Where(i => i.IsNeedQc == true && i.QcType == CommonConstants.QcTypeConstants.LOT_NUMBER_BATCH && i.LotNumberBatch == newQcValidationMain.LotNumberBatch)
                        .ExecuteUpdate(item => item.SetProperty(x => x.IsNeedQc, false));
                }

                _dbContext.SaveChanges();
                scope.Complete();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("事務失敗[CreateQcValidation]：{msg}", ex);
                return (false, ex.Message);
            }

        }
    }
}
