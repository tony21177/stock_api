using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateOrUpdatePurchaseFlowSettingRequest
    {
        public string? FlowId { get; set; }
        public string? CompId { get; set; }
        public string? FlowName { get; set; }
        public int? Sequence { get; set; }
        public string? UserId { get; set; }
        public bool? IsActive { get; set; }
    }
}
