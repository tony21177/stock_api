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

        // 20260104新增
        public string? DeliverFunction { get; set; }
        public string? Delivery { get; set; }
        public bool? IsActive { get; set; }
        public string? PackageWay { get; set; }
        public string? ProductModel { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSpec { get; set; }
        public float? SafeQuantity { get; set; }
        public string? SavingFunction { get; set; }
        public string? UdiSerialCode { get; set; }
        public float? UnitConversion { get; set; }




    }
}
