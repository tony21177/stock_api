namespace stock_api.Controllers.Request
{
    public class ListInstrumentRequest: BaseSearchRequest
    {
        public int? InstrumentId { get; set; }
        public string? CompId { get; set; }
        public string? InstrumentName { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
