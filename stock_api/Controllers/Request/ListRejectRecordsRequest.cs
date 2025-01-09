namespace stock_api.Controllers.Request
{
    public class ListRejectRecordsRequest : BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? ProductId { get; set; }
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? RejectUserId { get; set; }
        public string? InStockUserId { get; set; }

     }
}
