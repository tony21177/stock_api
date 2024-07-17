namespace stock_api.Service.ValueObject
{
    public class NotifyProductQuantity
    {
        public string CompId { get; set; } = null!;
        public float InStockQuantity { get; set; } = 0.0f;
        public float? OutStockQuantity { get; set; }
        public float InProcessingQrderQuantity { get; set; } = 0.0f;

        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public float SafeQuantity { get; set; } 
        public float? MaxSafeQuantity { get; set; }
    }
}
