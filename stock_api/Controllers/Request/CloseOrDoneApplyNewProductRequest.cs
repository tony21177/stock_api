namespace stock_api.Controllers.Request
{
    public class CloseOrDoneApplyNewProductRequest
    {
        public string ApplyId { get; set; } = null!;
        public string CurrentStatus { get; set; } = null!;
    }
}
