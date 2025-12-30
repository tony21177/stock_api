namespace stock_api.Controllers.Request
{
    public class OwnerListPurchasesRequest:BaseSearchRequest
    {
        // Filter by ApplyDate (format: yyyy/MM/dd)
        public string? ApplyDateStart { get; set; }
        public string? ApplyDateEnd { get; set; }
    }
}
