namespace stock_api.Controllers.Request
{
    public class ListAdjustItemsRequest : BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? MainId { get; set; }
        public string? AdjustCompId { get; set; }
        public string? Type {  get; set; }
        public string? UserId { get; set; }
        public string? CurrentStatus { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set;}
        public string? ProductId { get; set; }
        public string? ProductCode { get; set;}
    }
}
