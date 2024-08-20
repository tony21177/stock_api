namespace stock_api.Controllers.Request
{
    public class GetProductInAndOutStockRecordsRequest : BaseSearchRequest
    {
        public string GroupId { get; set; } = null!;
        public string? CompId { get; set; }
        public string? InStartDate { get; set; }
        public string? InEndDate { get; set; }
        public string? OutStartDate { get; set; }
        public string? OutEndDate { get; set; }
    }
}
