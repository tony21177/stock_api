namespace stock_api.Controllers.Request
{
    public class SearchProductLotQuantityRequest
    {
        public string? CompId { get; set; }
        public string ProductId { get; set; } = null!;
    }
}
