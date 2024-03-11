namespace stock_api.Controllers.Request
{
    public class SearchHandoverDetailRequest : BaseSearchRequest
    {
        public int? MainSheetId { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }
    }
}
