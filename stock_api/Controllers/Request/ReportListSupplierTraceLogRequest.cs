namespace stock_api.Controllers.Request
{
    public class ReportListSupplierTraceLogRequest : BaseSearchRequest
    {
        public int SupplierId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set;}
        public string? CompId { get; set; }
         

    }
}
