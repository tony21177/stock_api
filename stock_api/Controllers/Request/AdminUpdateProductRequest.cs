namespace stock_api.Controllers.Request
{
    public class AdminUpdateProductRequest:UpdateProductRequest
    {
        public string?   ManufacturerId { get; set; }
        public string? DeadlineRule { get; set; }
        public int? OpenDeadline { get; set; }
        public string? ProductName { get; set; }
        public string?  ProductCode { get; set; }
        public string? ProductModel { get; set; }
        public string? ProductSpec { get; set; }
        public string? Unit { get; set; }
        public string? ProductMachine { get; set; }
        public int? DefaultSupplierId { get; set; }
        public int? UnitConversion { get; set; }
        public int? TestCount { get; set; }
        public string? Manager { get; set; }
        public int? MaxSafeQuantity { get; set; }
        public string? LastAbleDate { get; set; }
        public string? LastOutStockDate { get; set; }
        public string? OriginalDeadline { get; set; }
        public int? PreDeadline { get; set; }
        public int? SafeQuantity { get; set; }
        public string? Weight { get; set; }
        public int? AllowReceiveDateRange { get; set; }
        public string? Delievery { get; set; }

    }
}
