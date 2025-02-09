namespace stock_api.Controllers.Request
{
    public class UpdateInstrumentRequest
    {
        public int InstrumentId { get; set; }
        public string? InstrumentName { get; set; }
        public bool? IsActive { get; set; } 
    }
}
