namespace stock_api.Controllers.Request
{
    public class AdjustRequest
    {
        public string? CompId { get; set; }
        public List<AdjustItem> AdjustItems { get; set; } = null!;
        
    }


    public class AdjustItem
    {
        public string ProductId { get; set; } = null!;
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public float BeforeQuantity { get; set; }
        public float AfterQuantity { get; set; }
        public List<Assign> BatchAssignList { get; set; } = new List<Assign>();

    }

    public class Assign
    {
        public string InStockId { get; set; } = null!;
        public string? LotNumberBatch { get; set; }
        public string? LotNumber { get; set; }
        public string? ItemId { get; set; }
        public float? InStockQuantity { get; set; }
        public float? OutStockQuantity { get; set; }
        public float? Adjust_calculate_qty { get; set; }
        
    }
}
