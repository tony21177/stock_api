namespace stock_api.Controllers.Request
{
    public class CreateInstrumentRequest
    {
        public string? CompId { get; set; }
        public string InstrumentName { get; set; } = null!;
        public bool? IsActive { get; set; } = true;
    }
}
