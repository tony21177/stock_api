namespace stock_api.Controllers.Request
{
    public class ListSupplierTraceLogRequest:BaseSearchRequest
    {
        public string? SourceType { get; set; }
        public int? InDays { get; set; }
        public string? AbnormalType { get; set; }

        public string? CompId { get; set; }

    }
}
