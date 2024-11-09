namespace stock_api.Controllers.Request
{
    public class UpdateInStockRequest
    {
        public string AcceptId { get; set; } = null!;

        public string? LotNumber { get; set; } = null!;

        public string? ExpirationDate { get; set; }
    }
}
