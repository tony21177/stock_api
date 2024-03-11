using stock_api.Models;

namespace stock_api.Utils
{
    public class MemberAndPermissionSetting
    {
        public MemberAndPermissionSetting(Member member, PermissionSetting permissionSetting)
        {
            Member = member;
            PermissionSetting = permissionSetting;
        }

        public Member Member { get; set; }
        public PermissionSetting PermissionSetting { get; set; }
    }
    public class PermissionSetting
    {
        public bool IsCreateAnnouce { get; set; }
        public bool IsUpdateAnnouce { get; set; }
        public bool IsDeleteAnnouce { get; set; }
        public bool IsHideAnnouce { get; set; }
        public bool IsCreateHandover { get; set; }
        public bool IsUpdateHandover { get; set; }
        public bool IsDeleteHandover { get; set; }
        public bool IsMemberControl { get; set; }
        public bool IsCheckReport { get; set; }
    }


}
