using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;

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
            return _dbContext.PurchaseFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId&&pfs.IsActive==true).ToList().Count > 0;
        }

        public PurchaseFlowSettingVo? GetPurchaseFlowSettingVoByFlowId(string flowId)
        {
            var result = from pfs in _dbContext.PurchaseFlowSettings
                         join member in _dbContext.WarehouseMembers
                         on pfs.UserId equals member.UserId
                         where pfs.FlowId == flowId
                         select new PurchaseFlowSettingVo
                         {
                             FlowId = pfs.FlowId,
                             CompId = pfs.CompId,
                             FlowName = pfs.FlowName,
                             Sequence = pfs.Sequence,
                             UserId = pfs.UserId,
                             IsActive = pfs.IsActive,
                             CreatedAt = pfs.CreatedAt,
                             UpdatedAt = pfs.UpdatedAt,
                             UserDisplayName = member.DisplayName
                         };

            return result.FirstOrDefault();
        }

        public PurchaseFlowSetting? GetPurchaseFlowSettingByFlowId(string flowId)
        {
            return _dbContext.PurchaseFlowSettings.Where(s => s.FlowId == flowId).FirstOrDefault();
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
            updateRequest.Sequence ??= existingPurchaseFlowSetting.Sequence;
            var updatePurchaseFlowSetting = _mapper.Map<PurchaseFlowSetting>(updateRequest);
            _mapper.Map(updatePurchaseFlowSetting, existingPurchaseFlowSetting);
            
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public void InactivePurchaseFlowSetting(String flowId,bool isActive)
        {
            _dbContext.PurchaseFlowSettings.Where(f => f.FlowId == flowId).ExecuteUpdate(f => f.SetProperty(f => f.IsActive, false));
        }


        public List<PurchaseFlowSettingVo> GetAllPurchaseFlowSettingsByCompId(string compId)
        {
            var result = from pfs in _dbContext.PurchaseFlowSettings
                         join member in _dbContext.WarehouseMembers
                         on pfs.UserId equals member.UserId
                         where pfs.CompId == compId
                         select new PurchaseFlowSettingVo
                         {
                             FlowId = pfs.FlowId,
                             CompId = pfs.CompId,
                             FlowName = pfs.FlowName,
                             Sequence = pfs.Sequence,
                             UserId = pfs.UserId,
                             IsActive = pfs.IsActive,
                             CreatedAt = pfs.CreatedAt,
                             UpdatedAt = pfs.UpdatedAt,
                             UserDisplayName = member.DisplayName 
                         };


            return result.ToList();
        }
       
    }
}
