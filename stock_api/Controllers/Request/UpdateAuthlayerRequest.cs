namespace stock_api.Controllers.Request
{
    public class UpdateAuthlayerRequest
    {
        public int AuthId { get; set; }
        public string? AuthName { get; set; }
        public short? AuthValue { get; set; }
        public bool? IsCreateAnnouce { get; set; }
        public bool? IsUpdateAnnouce { get; set; }
        public bool? IsDeleteAnnouce { get; set; }
        public bool? IsHideAnnouce { get; set; }
        public bool? IsCreateHandover { get; set; }
        public bool? IsUpdateHandover { get; set; }
        public bool? IsDeleteHandover { get; set; }
        public bool? IsMemberControl { get; set; }
        public bool? IsCheckReport { get; set; }
        public string? AuthDescription { get; set; }
    }
}
