namespace stock_api.Controllers.Request
{
    public class ReturnRequest
    {
        public string OutStockId { get; set; } = null!;
        public float ReturnQuantity { get; set; } 
    }
}
