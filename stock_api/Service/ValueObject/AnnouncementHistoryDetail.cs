using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class AnnouncementHistoryDetail
    {
        public string OldTitle { get; set; }
        public string NewTitle { get; set; }
        public string OldContent { get; set; }
        public string NewContent { get; set; }
        public string? OldBeginPublishTime { get; set; }
        public string? NewBeginPublishTime { get; set; }
        public string? OldEndPublishTime { get; set; }
        public string? NewEndPublishTime { get; set; }
        public string? OldBeginViewTime { get; set; }
        public string? NewBeginViewTime { get; set; }
        public string? OldEndViewTime { get; set; }
        public string? NewEndViewTime { get; set; }
        public bool? OldIsActive { get; set; }
        public bool? NewIsActive { get; set; }
        public string AnnounceId { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public List<AnnounceAttachment> OldAttachmentList { get; set; }
        public List<AnnounceAttachment> NewAttachmentList { get; set; }
        public string OldReaderNames { get; set; }
        public string NewReaderNames { get; set; }
        public string Action { get; set; }
    }


}
