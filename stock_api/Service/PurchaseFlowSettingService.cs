using AutoMapper;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Controllers.Request;
using stock_api.Models;

namespace stock_api.Service
{
    public class PurchaseFlowSettingService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public PurchaseFlowSettingService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public bool IsSequenceExist(int seq,string compId)
        {
            return _dbContext.PurchaseFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId).ToList().Count > 0;
        }

        public PurchaseFlowSetting? GetPurchaseFlowSettingByFlowId(string flowId)
        {
            return _dbContext.PurchaseFlowSettings.Where(pfs=>pfs.FlowId==flowId).FirstOrDefault();
        }

        public void AddPurchaseFlowSetting(PurchaseFlowSetting newPurchaseFlowSetting)
        {
            newPurchaseFlowSetting.FlowId = Guid.NewGuid().ToString();
            _dbContext.PurchaseFlowSettings.Add(newPurchaseFlowSetting);
            _dbContext.SaveChanges();
            return;
        }

        public void UpdatePurchaseFlowSetting(CreateOrUpdatePurchaseFlowSettingRequest updateRequest,PurchaseFlowSetting existingPurchaseFlowSetting)
        {
            if (updateRequest.Sequence == null)
            {
                updateRequest.Sequence = existingPurchaseFlowSetting.Sequence;
            }
            var updatePurchaseFlowSetting = _mapper.Map<PurchaseFlowSetting>(updateRequest);
            _mapper.Map(updatePurchaseFlowSetting, existingPurchaseFlowSetting);
            
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public List<PurchaseFlowSetting> GetAllPurchaseFlowSettingsByCompId(string compId)
        {
            return _dbContext.PurchaseFlowSettings.Where(_pfs=>_pfs.CompId==compId).ToList();  
            
        }

       
    }
}
