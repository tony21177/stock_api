namespace stock_api.Controllers.Request
{
    public class DiscardRequest
    {
        public string OutStockId { get; set; } = null!;
        public float ApplyQuantity { get; set; }
    }
}
