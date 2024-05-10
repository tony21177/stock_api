namespace stock_api.Controllers.Request
{
    public class ProductDetailByCodeRequest
    {
        public string ProductCode { get; set; }
        public string? CompId { get; set; }
    }
}
