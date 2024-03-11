namespace stock_api.Controllers.Request
{
    public class CreateOrUpdateSheetSettingMainRequest
    {

        public int? SheetId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool? IsActive { get; set; }
        public string? Version { get; set; }
        public string? SerialCode { get; set; }
        public List<string> AttIdList { get; set; } = new List<string>();
    }
}
