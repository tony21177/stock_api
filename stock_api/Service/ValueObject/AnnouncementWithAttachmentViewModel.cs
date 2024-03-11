namespace stock_api.Service.ValueObject
{
    public class AnnouncementWithAttachmentViewModel
    {
        // Announcement 屬性
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? BeginPublishTime { get; set; }
        public DateTime? EndPublishTime { get; set; }
        public DateTime? BeginViewTime { get; set; }
        public DateTime? EndViewTime { get; set; }
        public bool? IsActive { get; set; }
        public string AnnounceId { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }

        // AnnounceAttachment 屬性
        public string AttId { get; set; }
        public int? Index { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string FileSizeText { get; set; }
        public double? FileSizeNumber { get; set; }
        public DateTime? AttachmentCreatedTime { get; set; }
        public DateTime? AttachmentUpdatedTime { get; set; }
        public bool? AttachmentIsActive { get; set; }
        public string AttachmentCreatorId { get; set; }
    }

}
