namespace stock_api.Controllers.Request
{
    public class ListDiscardRecordsRequest : BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? ProductId { get; set; }
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? DiscardUserId { get; set; }
        public string? OutStockUserId { get; set; }

     }
}
