using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class PurchaseFlowSettingVo : PurchaseFlowSetting
    {
        public string UserDisplayName { get; set; }
    }
}
