namespace stock_api.Controllers.Request
{
    public class ManualUpdateSupplierTraceLogRequest
    {
        public int Id { get; set; }
        public string? AbnormalDate { get; set; }
        public int? SupplierId { get; set; }
        public string? AbnormalType { get; set; } = null!;
        public string? CompId { get; set; }
        public string? AbnormalContent { get; set; }
        public string? ProductId { get; set; }

        public string? SourceType { get; set; }
        public string? PurchaseMainId { get; set; }
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
    }
}
