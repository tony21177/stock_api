namespace stock_api.Controllers.Request
{
    public class UpdateProductToCompRequest
    {
        public string? FromCompId { get; set; }
        public List<string> ToCompIds { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public bool IsActive { get; set; } =true;
    }
}
