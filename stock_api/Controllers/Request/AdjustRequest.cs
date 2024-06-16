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
    }
}
