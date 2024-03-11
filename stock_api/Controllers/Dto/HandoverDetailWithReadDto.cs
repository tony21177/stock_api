using stock_api.Models;
using System.Text.Json.Serialization;

namespace stock_api.Controllers.Dto
{
    public class HandoverDetailWithReadDto
    {
        public string HandoverDetailId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int MainSheetId { get; set; }
        public string JsonContent { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public bool? IsActive { get; set; }
        [JsonIgnore]
        public string FileAttIds { get; set; }
        public List<FileDetailInfo> Files { get; set; } = new List<FileDetailInfo>();
        public bool? IsRead { get; set; }
    }
}
