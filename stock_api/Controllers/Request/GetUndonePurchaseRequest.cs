namespace stock_api.Controllers.Request
{
    public class GetUndonePurchaseRequest
    {
        public string? CompId { get; set; }
        public string ProductId { get; set; } = null!;
    }
}
