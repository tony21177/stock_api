using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Linq;

namespace stock_api.Service
{
    public class MemberService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public MemberService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public WarehouseMember? GetMemberByUserId(string userId)
        {
            return _dbContext.WarehouseMembers.Where(member => member.UserId == userId ).FirstOrDefault();
        }

        public List<WarehouseMember> GetMembersByUserIdList(List<string> userIdList, string compId)
        {
            return _dbContext.WarehouseMembers.Where(member => userIdList.Contains(member.UserId) && member.CompId == compId).ToList();
        }

        public List<WarehouseMember> GetActiveMembersByUserIds(List<string> userIdList, string compId)
        {
            if (userIdList == null || userIdList.Count == 0)
            {
                return new List<WarehouseMember>();
            }

            return _dbContext.WarehouseMembers.Where(member => userIdList.Contains(member.UserId) && member.IsActive == true && member.CompId==compId).ToList();
        }

        public WarehouseMember? GetMemberByAccount(string account)
        {
            var member = _dbContext.WarehouseMembers.Where(member => member.Account == account ).FirstOrDefault();
            return member;
        }

        public List<string> GetDisplayNameByUserIdList(List<string> userIdList, string compId)
        {
            // 宣告一個空的 DisplayName 列表
            var displayNames = new List<string>();

            // 如果提供的 userIdList 不為空
            if (userIdList != null && userIdList.Any())
            {
                // 從資料庫中查詢對應的 DisplayName
                var members = _dbContext.WarehouseMembers
                    .Where(member => userIdList.Contains(member.UserId) && member.CompId == compId) // 篩選出指定的 UserId
                    .ToList();

                // 提取 DisplayName
                displayNames = members.Select(member => member.DisplayName).ToList();
            }

            return displayNames;
        }

        public List<WarehouseMember> GetAllMembersOfComp(string compId)
        {
            return _dbContext.WarehouseMembers.Where(member=>member.CompId==compId).ToList();
        }

        public List<MemberWithCompanyUnitVo> GetAllMembersForOwner()
        {
            var result = from member in _dbContext.WarehouseMembers
                         join company in _dbContext.Companies
                         on member.CompId equals company.CompId
                         join companyUnit in _dbContext.CompanyUnits
                         on member.CompId equals companyUnit.CompId
                         select new MemberWithCompanyUnitVo
                         {
                             Account = member.Account,
                             Password = member.Password,
                             AuthValue = member.AuthValue,
                             DisplayName = member.DisplayName,
                             GroupIds = member.GroupIds.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                             PhotoUrl = member.PhotoUrl,
                             CompId = member.CompId,
                             UserId = member.UserId,
                             IsActive = member.IsActive,
                             CreatedAt = member.CreatedAt,
                             UpdatedAt = member.UpdatedAt,
                             Name = company.Name,
                             Type = company.Type,
                             UnitId = companyUnit.UnitId,
                             UnitName = companyUnit.UnitName,
                         };


            return result.ToList();
        }

        public List<WarehouseMember> GetAllMembersOfCompList(List<string> compIdList)
        {
            return _dbContext.WarehouseMembers.Where(member => compIdList.Contains(member.CompId)).ToList();
        }

        public List<Recipient> GetAlRecipientsOfComp(string compId)
        {
            var result = from member in _dbContext.WarehouseMembers
                         join authLayer in _dbContext.WarehouseAuthlayers
                         on member.AuthValue equals authLayer.AuthValue
                         where member.IsActive == true && member.CompId == compId
                         select new Recipient
                         {
                             UserId = member.UserId,
                             DisplayName = member.DisplayName,
                             Account = member.Account,
                             AuthValue = member.AuthValue,
                             AuthName = authLayer.AuthName,
                             AuthDescription = authLayer.AuthDescription
                         };

            return result.ToList();
        }

        public void UpdateMember(CreateOrUpdateMemberRequest request,WarehouseMember toBeUpdateMember)
        {
            var updateMember = _mapper.Map<WarehouseMember>(request);
            _mapper.Map(updateMember, toBeUpdateMember);

            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return ;
        }

        public WarehouseMember CreateMember(WarehouseMember newMember)
        {
            _dbContext.WarehouseMembers.Add(newMember);
            _dbContext.SaveChanges(true);
            return newMember;
        }

        public void DeleteMember(string userId)
        {
            var membersToDelete = _dbContext.WarehouseMembers.Where(member => member.UserId == userId).ToList();

            if (membersToDelete.Any())
            {
                _dbContext.WarehouseMembers.RemoveRange(membersToDelete);
                _dbContext.SaveChanges();
            }
        }

        public bool IsAccountNotExist(string account)
        {
            var existMemeber = _dbContext.WarehouseMembers.Where(member => member.Account == account).FirstOrDefault();
            if (existMemeber == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateMemberGroupIds(WarehouseMember member,List<string> groupIdList)
        {
            member.GroupIds = string.Join(",", groupIdList);
            _dbContext.SaveChanges();
            return;
        }

      
    }
}
