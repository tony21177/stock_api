using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class ApplyProductFlowSettingVo : ApplyProductFlowSetting
    {
        public string? ReviewUserName { get; set; }

        public string? ReviewGroupName { get; set; }
    }
}
