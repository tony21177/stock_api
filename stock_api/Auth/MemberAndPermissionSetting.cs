using stock_api.Models;
using stock_api.Service.ValueObject;

namespace stock_api.Utils
{
    public class MemberAndPermissionSetting
    {
        public MemberAndPermissionSetting(WarehouseMember member, PermissionSetting permissionSetting, CompanyWithUnitVo companyWithUnit)
        {
            Member = member;
            PermissionSetting = permissionSetting;
            CompanyWithUnit = companyWithUnit;
        }

        public WarehouseMember Member { get; set; }
        public PermissionSetting PermissionSetting { get; set; }

        public CompanyWithUnitVo CompanyWithUnit { get; set; }
    }
    public class PermissionSetting
    {
        /// <summary>
        /// 是否可以申請新增品項
        /// </summary>
        public bool IsApplyItemManage { get; set; }

        /// <summary>
        /// 是否可以進行群組管理
        /// </summary>
        public bool IsGroupManage { get; set; }

        /// <summary>
        /// 是否可以進行入庫作業
        /// </summary>
        public bool IsInBoundManage { get; set; }

        /// <summary>
        /// 是否可以進行出庫作業
        /// </summary>
        public bool IsOutBoundManage { get; set; }

        /// <summary>
        /// 是否可以進行庫存管理
        /// </summary>
        public bool IsInventoryManage { get; set; }

        /// <summary>
        /// 是否可以進行品項管理
        /// </summary>
        public bool IsItemManage { get; set; }

        /// <summary>
        /// 是否可以進行成員管理
        /// </summary>
        public bool IsMemberManage { get; set; }

        /// <summary>
        /// 是否可以進行盤點
        /// </summary>
        public bool IsRestockManage { get; set; }

        /// <summary>
        /// 是否可以進行抽點
        /// </summary>
        public bool IsVerifyManage { get; set; }
    }


}
