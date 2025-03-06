using AutoMapper;
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Controllers.Dto;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;

namespace stock_api.Service
{
    public class QcValidationFlowSettingService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public QcValidationFlowSettingService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public bool IsSequenceExist(int seq,string compId)
        {
            return _dbContext.QcValidationFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId).ToList().Count > 0;
        }

        public bool IsSequenceExistForGroupId(int seq,string groupId, string compId)
        {
            return _dbContext.QcValidationFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId&& pfs.ReviewGroupId==groupId).ToList().Count > 0;
        }

        public bool IsSequenceExistForGroupIdAndExcludeSettingId(string excludeSettingId,int seq, string groupId, string compId)
        {
            return _dbContext.QcValidationFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId && pfs.ReviewGroupId == groupId&&pfs.SettingId!= excludeSettingId).ToList().Count > 0;
        }

        public QcValidationFlowSettingVo? GetQcValidationSettingVoBySettingId(string settingId)
        {
            var result = from pfs in _dbContext.QcValidationFlowSettings
                         join member in _dbContext.WarehouseMembers
                         on pfs.ReviewUserId equals member.UserId 
                         join g in _dbContext.WarehouseGroups
                         on pfs.ReviewGroupId equals g.GroupId
                         where pfs.SettingId == settingId
                         select new QcValidationFlowSettingVo
                         {
                             SettingId = pfs.SettingId,
                             CompId = pfs.CompId,
                             FlowName = pfs.FlowName,
                             Sequence = pfs.Sequence,
                             ReviewUserId = pfs.ReviewUserId,
                             ReviewUserName = member.DisplayName,
                             ReviewGroupId = pfs.ReviewGroupId,
                             ReviewGroupName = g.GroupName,
                             CreatedAt = pfs.CreatedAt,
                             UpdatedAt = pfs.UpdatedAt,
                         };

            return result.FirstOrDefault();
        }

        public List<QcValidationFlowSettingVo> GeQcValidationFlowSettingVoListByGroupId(string groupId)
        {
            var result = from pfs in _dbContext.QcValidationFlowSettings
                         join member in _dbContext.WarehouseMembers
                         on pfs.ReviewUserId equals member.UserId
                         join g in _dbContext.WarehouseGroups
                         on pfs.ReviewGroupId equals g.GroupId
                         where pfs.ReviewGroupId == groupId
                         select new QcValidationFlowSettingVo
                         {
                             SettingId = pfs.SettingId,
                             CompId = pfs.CompId,
                             FlowName = pfs.FlowName,
                             Sequence = pfs.Sequence,
                             ReviewUserId = pfs.ReviewUserId,
                             ReviewUserName = member.DisplayName,
                             ReviewGroupId = pfs.ReviewGroupId,
                             ReviewGroupName = g.GroupName,
                             CreatedAt = pfs.CreatedAt,
                             UpdatedAt = pfs.UpdatedAt,
                         };

            return result.ToList();
        }

        public QcValidationFlowSetting? GetQcValidationFlowSettingBySettingId(string settingId)
        {
            return _dbContext.QcValidationFlowSettings.Where(s => s.SettingId == settingId).FirstOrDefault();
        }

        public void AddQcValidationFlowSetting(List<QcValidationFlowSettingRequest> createQcValidationFlowSettingList, string CompId)
        {
            createQcValidationFlowSettingList.ForEach(s => {
                    s.SettingId = Guid.NewGuid().ToString();
                    s.CompId = CompId;
                }) ;

            var newQcValidationFlowSettingList = _mapper.Map<List<QcValidationFlowSetting>>(createQcValidationFlowSettingList);


            _dbContext.QcValidationFlowSettings.AddRange(newQcValidationFlowSettingList);
            _dbContext.SaveChanges();
            return;
        }

        public void UpdateQcValidationFlowSetting(QcValidationFlowSettingRequest updateRequest, QcValidationFlowSetting existingQcValidationFlowSetting)
        {
            updateRequest.Sequence ??= existingQcValidationFlowSetting.Sequence;
            var updateQcValidationFlowSetting = _mapper.Map<QcValidationFlowSetting>(updateRequest);
            _mapper.Map(updateQcValidationFlowSetting, existingQcValidationFlowSetting);
            
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public void DeleteQcValidationFlowSetting(String settingId)
        {
            _dbContext.QcValidationFlowSettings.Where(f => f.SettingId == settingId).ExecuteDelete();
        }


        public List<QcValidationFlowSettingVo> GetAllQcValidationFlowSettingsByCompId(string compId)
        {
            var result = from pfs in _dbContext.QcValidationFlowSettings
                         join member in _dbContext.WarehouseMembers 
                         on pfs.ReviewUserId equals member.UserId into gm
                         from subm in gm.DefaultIfEmpty()
                         join g in _dbContext.WarehouseGroups
                         on pfs.ReviewGroupId equals g.GroupId into gg
                         from subg in gg.DefaultIfEmpty()
                         where pfs.CompId == compId
                         select new QcValidationFlowSettingVo
                         {
                             SettingId = pfs.SettingId,
                             CompId = pfs.CompId,
                             FlowName = pfs.FlowName,
                             Sequence = pfs.Sequence,
                             ReviewUserId = pfs.ReviewUserId,
                             ReviewUserName = subm.DisplayName,
                             ReviewGroupId = pfs.ReviewGroupId,
                             ReviewGroupName = subg.GroupName,
                             CreatedAt = pfs.CreatedAt,
                             UpdatedAt = pfs.UpdatedAt,
                         };


            return result.ToList();
        }
       
    }
}
