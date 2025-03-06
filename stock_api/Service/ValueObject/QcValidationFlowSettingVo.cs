using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class QcValidationFlowSettingVo : QcValidationFlowSetting
    {
        public string? ReviewUserName { get; set; }

        public string? ReviewGroupName { get; set; }
    }
}
