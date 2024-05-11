namespace stock_api.Controllers.Request
{
    public class OutboundRequest
    {
        public string? CompId { get; set; }
        public string LotNumberBatch { get; set; } = null!;

        public string LotNumber { get; set; } = null!;

        public string ProductCode { get; set; } = null!;

        public int ApplyQuantity { get; set; } 
        public bool IsAbnormal { get; set; } = false;

        public string? AbnormalReason { get; set; }  

        public string Type { get; set; } = null!;
        public bool? IsConfirmed { get; set; }
    }
}
