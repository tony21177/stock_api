namespace stock_api.Controllers.Request
{
    public class ListSupplierTraceLogRequest:BaseSearchRequest
    {
        public string? SourceType { get; set; }
        public int? InDays { get; set; }
        public string? AbnormalType { get; set; }

        public string? CompId { get; set; }

        public string? ProductId { get; set;}
        public int? SupplierId { get; set; }  
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }  

    }
}
