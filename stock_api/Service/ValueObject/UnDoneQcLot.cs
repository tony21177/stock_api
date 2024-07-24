namespace stock_api.Service.ValueObject
{
    public class UnDoneQcLot
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string QcType { get; set; } = null!;
        public string? QcTestStatus { get; set; }
    }
}
