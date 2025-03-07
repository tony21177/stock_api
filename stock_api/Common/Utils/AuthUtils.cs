using stock_api.Common.Constant;
using stock_api.Utils;

namespace stock_api.Common.Utils
{
    public class AuthUtils
    {
        public static bool IsCrossCompAuthorized(MemberAndPermissionSetting memberAndPermissionSetting)
        {
            return memberAndPermissionSetting.CompanyWithUnit?.Type == CommonConstants.CompanyType.OWNER
                || memberAndPermissionSetting.Member?.IsAdmin == true
                || memberAndPermissionSetting.Member?.IsNoStockReviewer == true;
        }
    }
}
