namespace stock_api.Controllers.Request
{

    public class OwnerDirectBatchOutboundRequest
    {
        public List<OutItem> OutItems { get; set; } = null!;
        public string? CompId { get; set; }

    }

    public class OutItem
    {
        public string ProductId { get; set; } = null!;
        public int OutQuantity { get; set; } 
        public string? Remark { get; set; }
    }
}
