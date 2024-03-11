namespace stock_api.Controllers.Request
{
    public class UpdateAnnouncementRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? BeginPublishTime { get; set; }
        public string? EndPublishTime { get; set; }
        public string? BeginViewTime { get; set; }
        public string? EndViewTime { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? ReaderUserIdList { get; set; }
        public List<string>? AttIdList { get; set; }
    }
}
