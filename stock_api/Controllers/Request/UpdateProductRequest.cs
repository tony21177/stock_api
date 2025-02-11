using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class UpdateProductRequest
    {
        public string? CompId {  get; set; }
        public string ProductId { get; set; }
        public string? DeliverFunction { get; set; }
        public string? DeliverRemarks { get; set; }
        public List<string>? GroupIds { get; set; }
        public string? OpenedSealName { get; set; }
        public string? PackageWay { get; set; }
        public int? PreOrderDays { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductRemarks { get; set; }
        public string? SavingFunction { get; set; }
        public string? UdibatchCode { get; set; }
        public string? UdicreateCode { get; set; }
        public string? UdiserialCode { get; set; }
        public string? UdiverifyDateCode { get; set; }
        public bool? IsNeedAcceptProcess { get; set; }
        public string? StockLocation { get; set; }
        public string? DeadlineRule { get; set; }
        public int? OpenDeadline { get; set; }
        public string? Unit { get; set; }
        public string? QcType { get; set; }
        public bool? IsPrintSticker { get; set; }
        public string? PreDeadline {get;set;}

        public bool? IsAllowDiscard { get; set; }

        public List<int>? InstrumentIds { get; set; }
    }
}
