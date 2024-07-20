namespace stock_api.Controllers.Request
{
    public class ListApplyNewProductMainRequest : BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? ApplyId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? ProductGroupId { get; set; }
        public string? CurrentStatus { get; set; }
        

     }
}
