using stock_api.Models;

namespace stock_api.Controllers.Dto
{
    public class UnreadDto
    {
        public int UnreadHandoverCount { get; set; }
        public int UnreadAnnouncementCount { get; set; }
        public List<HandoverDetail> UnreadHandoverDetails { get; set; } = new();
        public List<Announcement> UnreadAnnouncements { get; set; } = new();
    }
}
