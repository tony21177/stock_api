namespace stock_api.Controllers.Request
{
    public class RejectItemRequest
    {
        public string InStockId { get; set; } = null!;
        public string RejectReason { get; set; }
        public string? RejectDate { get; set; }
        public float? RejectQuantity { get; set; }
    }
}
