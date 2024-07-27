namespace stock_api.Controllers.Dto
{
    public class NeedQc
    {
        public string PurchaseMainID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public DateTime ApplyDate { get; set; }
        public DateTime AcceptedAt { get; set; }
        public string AcceptUserName { get; set; }
        public string AcceptUserId { get; set; }
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? QcType { get; set;} 
            
    }
}
