using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;

namespace stock_api.Service
{
    public class GroupService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public GroupService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public void AddGroup(WarehouseGroup group)
        {
            group.GroupId = Guid.NewGuid().ToString();
            _dbContext.WarehouseGroups.Add(group);
            _dbContext.SaveChanges();
        }

        public void UpdateGroup(UpdateGroupRequest groupRequest,WarehouseGroup existingGroup)
        {
            _mapper.Map(groupRequest,existingGroup);

            _dbContext.SaveChanges();
            return;
        }

        public List<WarehouseGroup> GetGroups(string compId) { 
            return _dbContext.WarehouseGroups.Where(wg=>wg.CompId == compId).ToList();
        }

        public List<WarehouseGroup> GetGroupsByIdList(List<string>? groupIdList)
        {
            if (groupIdList == null) return new();
            return _dbContext.WarehouseGroups.Where(wg => groupIdList.Contains(wg.GroupId)).ToList();
        }

        public WarehouseGroup? GetGroupByGroupId(string groupId)
        {
            return _dbContext.WarehouseGroups.Where(wg=>wg.GroupId==groupId).FirstOrDefault();
        }

        public List<WarehouseGroup> GetGroupsByCompId(string compId)
        {
            return _dbContext.WarehouseGroups.Where(wg => wg.CompId == compId).ToList();
        }

        public List<WarehouseGroup> GetGroupsByGroupNameList(List<string> groupNameList)
        {
            return _dbContext.WarehouseGroups.Where(wg => wg.GroupName!=null &&groupNameList.Contains(wg.GroupName)).ToList();
        }

        public List<WarehouseGroup> GetGroupsByGroupNameListAndCompId(List<string> groupNameList,string compId)
        {
            return _dbContext.WarehouseGroups.Where(wg => wg.GroupName != null && groupNameList.Contains(wg.GroupName)&&wg.CompId==compId).ToList();
        }
    }
}
