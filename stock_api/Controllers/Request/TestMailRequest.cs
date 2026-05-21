namespace stock_api.Controllers.Request
{
    public class TestMailRequest
    {
        public string Email { get; set; } = null!;
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
