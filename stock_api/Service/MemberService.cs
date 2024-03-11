using AutoMapper;
using stock_api.Models;
using stock_api.Service.ValueObject;

namespace stock_api.Service
{
    public class MemberService
    {
        private readonly HandoverContext _dbContext;
        private readonly IMapper _mapper;

        public MemberService(HandoverContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public Member? GetMemberByUserId(string userId)
        {
            return _dbContext.Members.Where(member => member.UserId == userId).FirstOrDefault();
        }

        public List<Member> GetMembersByUserIdList(List<string> userIdList)
        {
            return _dbContext.Members.Where(member => userIdList.Contains(member.UserId)).ToList();
        }

        public List<Member> GetActiveMembersByUserIds(List<string> userIdList)
        {
            if (userIdList == null || userIdList.Count == 0)
            {
                return new List<Member>();
            }

            return _dbContext.Members.Where(member => userIdList.Contains(member.UserId) && member.IsActive == true).ToList();
        }

        public Member? GetMemberByAccount(string account)
        {
            var member = _dbContext.Members.Where(member => member.Account == account).FirstOrDefault();
            return member;
        }

        public List<string> GetDisplayNameByUserIdList(List<string> userIdList)
        {
            // 宣告一個空的 DisplayName 列表
            var displayNames = new List<string>();

            // 如果提供的 userIdList 不為空
            if (userIdList != null && userIdList.Any())
            {
                // 從資料庫中查詢對應的 DisplayName
                var members = _dbContext.Members
                    .Where(member => userIdList.Contains(member.UserId)) // 篩選出指定的 UserId
                    .ToList();

                // 提取 DisplayName
                displayNames = members.Select(member => member.DisplayName).ToList();
            }

            return displayNames;
        }

        public List<Member> GetAllMembers()
        {
            return _dbContext.Members.ToList();
        }

        public List<Recipient> GetAlRecipients()
        {
            var result = from member in _dbContext.Members
                         join authLayer in _dbContext.Authlayers
                         on member.AuthValue equals authLayer.AuthValue
                         where member.IsActive == true
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

        public Member? UpdateMember(Member member)
        {
            var toBeUpdateMember = _dbContext.Members.FirstOrDefault(_member => _member.UserId == member.UserId);
            if (toBeUpdateMember == null) return null;
            _mapper.Map(member, toBeUpdateMember);

            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return member;
        }

        public Member CreateMember(Member newMember)
        {
            _dbContext.Members.Add(newMember);
            _dbContext.SaveChanges(true);
            return newMember;
        }

        public void DeleteMember(string userId)
        {
            var membersToDelete = _dbContext.Members.Where(member => member.UserId == userId).ToList();

            if (membersToDelete.Any())
            {
                _dbContext.Members.RemoveRange(membersToDelete);
                _dbContext.SaveChanges();
            }
        }

        public bool IsAccountNotExist(string account)
        {
            var existMemeber = _dbContext.Members.Where(member => member.Account == account).FirstOrDefault();
            if (existMemeber == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsUidNotExist(string uid)
        {
            var existMemeber = _dbContext.Members.Where(member => member.Uid == uid).FirstOrDefault();
            if (existMemeber == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
