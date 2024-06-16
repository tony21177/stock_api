namespace stock_api.Controllers.Request
{
    public class ManualCreateSupplierTraceLogRequest
    {
        public string AbnormalDate { get; set; }
        public int  SupplierId { get; set; }
        public string AbnormalType { get; set; } = null!;
        public string? CompId { get; set; }
        public string? AbnormalContent { get; set; }
    }
}
