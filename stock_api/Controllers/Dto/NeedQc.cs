namespace stock_api.Controllers.Dto
{
    public class NeedQc
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string InStockDate { get; set; }
        public string InStockUserName { get; set; }
        public string InStockUserId { get; set; }
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? QcType { get; set;} 
            
    }
}
