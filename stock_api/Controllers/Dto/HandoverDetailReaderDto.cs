namespace stock_api.Controllers.Dto
{
    public class HandoverDetailReaderDto
    {
        public int Id { get; set; }

        public string HandoverDetailId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }
        public string? PhotoUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadTime { get; set; }

        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
