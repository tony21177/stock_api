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
        public string PurchaseMainId { get; set; } = null!;
        public DateTime ApplyDate { get; set; } 
        public String InStockId { get; set; } = null!;
        public DateTime AcceptedAt { get; set; }
        public string AcceptUserName { get; set; }
        public string AcceptUserId { get; set; }

        public string ProductSpec { get; set; } = null!;
    }
}
