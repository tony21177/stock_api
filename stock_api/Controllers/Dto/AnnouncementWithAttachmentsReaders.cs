using stock_api.Models;

namespace stock_api.Controllers.Dto
{
    public class AnnouncementWithAttachmentsReaders : AnnouncementWithAttachments
    {
        

        public List<AnnouceReaderMemberDto> ReaderUserList { get; set; } = new List<AnnouceReaderMemberDto>();

    }
}
