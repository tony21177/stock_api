namespace stock_api.Controllers.Request
{
    public class ListMyReviewPurchaseRequest : BaseSearchRequest
    {
        public string? UserId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Type { get; set; }
     }
}
