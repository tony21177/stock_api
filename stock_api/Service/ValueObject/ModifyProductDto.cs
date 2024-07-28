namespace stock_api.Service.ValueObject
{
    public class ModifyProductDto
    {
        public string ProductCode { get; set; } = null!;
        public int? DeadlineRule { get; set; }
        public string? DeliverRemarks { get; set; }
        public string? GroupNames {  get; set; }
        public string? Manager { get; set; } 
        public int? MaxSafeQuantity { get; set; }
        public string? OpenedSealName { get; set; }
        public int? PreOrderDays { get; set; }
        public string? ProductCategory { get; set; } 
        public string? ProductRemarks { get; set; }
        public string? Unit { get; set; } 
        public string? ProductMachine { get; set; }
        public bool? IsNeedAcceptProcess { get; set; }
        public string? StockLocation { get; set; } 
        public bool? IsPrintSticker { get; set; }
    }
}
