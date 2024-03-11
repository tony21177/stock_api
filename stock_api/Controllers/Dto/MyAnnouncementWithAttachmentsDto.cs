using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using stock_api.Models;
using Microsoft.VisualBasic;

namespace stock_api.Controllers.Dto
{
    public class MyAnnouncementWithAttachmentsDto
    {
 

        public string Title { get; set; }


        public string Content { get; set; }


        public DateTime? BeginPublishTime { get; set; }

        public DateTime? EndPublishTime { get; set; }

        public DateTime? BeginViewTime { get; set; }


        public DateTime? EndViewTime { get; set; }

        public bool? IsActive { get; set; }


        public string AnnounceId { get; set; }

        public string CreatorId { get; set; }

        public string UserId { get; set; }

        public bool IsBookToTop { get; set; }

        public bool IsRemind { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public List<AnnounceAttachment> AnnounceAttachments { get; set; } = new List<AnnounceAttachment>();
        public string? CreatorName { get; set; }

        public bool? IsRead { get; set; }
    }
}
