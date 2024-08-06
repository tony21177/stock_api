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
    public class ApplyProductFlowSettingService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public ApplyProductFlowSettingService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public bool IsSequenceExist(int seq,string compId)
        {
            return _dbContext.ApplyProductFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId).ToList().Count > 0;
        }

        public bool IsSequenceExistForGroupId(int seq,string groupId, string compId)
        {
            return _dbContext.ApplyProductFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId&& pfs.ReviewGroupId==groupId).ToList().Count > 0;
        }

        public bool IsSequenceExistForGroupIdAndExcludeSettingId(string excludeSettingId,int seq, string groupId, string compId)
        {
            return _dbContext.ApplyProductFlowSettings.Where(pfs => pfs.Sequence == seq && pfs.CompId == compId && pfs.ReviewGroupId == groupId&&pfs.SettingId!= excludeSettingId).ToList().Count > 0;
        }

        public ApplyProductFlowSettingVo? GetApplyProductFlowSettingVoBySettingId(string settingId)
        {
            var result = from pfs in _dbContext.ApplyProductFlowSettings
                         join member in _dbContext.WarehouseMembers
                         on pfs.ReviewUserId equals member.UserId 
                         join g in _dbContext.WarehouseGroups
                         on pfs.ReviewGroupId equals g.GroupId
                         where pfs.SettingId == settingId
                         select new ApplyProductFlowSettingVo
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

        public List<ApplyProductFlowSettingVo> GetApplyProductFlowSettingVoListByGroupId(string groupId)
        {
            var result = from pfs in _dbContext.ApplyProductFlowSettings
                         join member in _dbContext.WarehouseMembers
                         on pfs.ReviewUserId equals member.UserId
                         join g in _dbContext.WarehouseGroups
                         on pfs.ReviewGroupId equals g.GroupId
                         where pfs.ReviewGroupId == groupId
                         select new ApplyProductFlowSettingVo
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

        public ApplyProductFlowSetting? GetApplyProductFlowSettingBySettingId(string settingId)
        {
            return _dbContext.ApplyProductFlowSettings.Where(s => s.SettingId == settingId).FirstOrDefault();
        }

        public void AddApplyProductFlowSetting(List<ApplyProductFlowSettingRequest> createApplyProductFlowSettingList,string CompId)
        {
            createApplyProductFlowSettingList.ForEach(s => {
                    s.SettingId = Guid.NewGuid().ToString();
                    s.CompId = CompId;
                }) ;

            var newApplyProductFlowSettingList = _mapper.Map<List<ApplyProductFlowSetting>>(createApplyProductFlowSettingList);


            _dbContext.ApplyProductFlowSettings.AddRange(newApplyProductFlowSettingList);
            _dbContext.SaveChanges();
            return;
        }

        public void UpdateApplyProductFlowSetting(ApplyProductFlowSettingRequest updateRequest,ApplyProductFlowSetting existingApplyProductFlowSetting)
        {
            updateRequest.Sequence ??= existingApplyProductFlowSetting.Sequence;
            var updateApplyProductFlowSetting = _mapper.Map<ApplyProductFlowSetting>(updateRequest);
            _mapper.Map(updateApplyProductFlowSetting, existingApplyProductFlowSetting);
            
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return;
        }

        public void DeleteApplyProductFlowSetting(String settingId)
        {
            _dbContext.ApplyProductFlowSettings.Where(f => f.SettingId == settingId).ExecuteDelete();
        }


        public List<ApplyProductFlowSettingVo> GetAllApplyProductFlowSettingsByCompId(string compId)
        {
            var result = from pfs in _dbContext.ApplyProductFlowSettings
                         join member in _dbContext.WarehouseMembers 
                         on pfs.ReviewUserId equals member.UserId into gm
                         from subm in gm.DefaultIfEmpty()
                         join g in _dbContext.WarehouseGroups
                         on pfs.ReviewGroupId equals g.GroupId into gg
                         from subg in gg.DefaultIfEmpty()
                         where pfs.CompId == compId
                         select new ApplyProductFlowSettingVo
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
