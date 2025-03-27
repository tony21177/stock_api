using stock_api.Common.Constant;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class ListMainWithDetailRequest:BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? MainId { get; set; }
        public string? PurchaseMainId { get; set; }
        public string? InStockId { get; set; } = null!;
        public string? QcStartDate { get; set; }
        public string? QcEndDate { get; set;}
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? QcType { get; set; }
        public string? CurrentStatus { get; set; } = CommonConstants.QcCurrentStatus.AGREE;
    }
}
