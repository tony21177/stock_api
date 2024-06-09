namespace stock_api.Controllers.Request
{
    public class UploadProductImageRequest
    {
        public string ProductId { get; set; } = null!;

        public string? CompId { get; set; } 
        public IFormFile? Image { get; set; }
    }
}
