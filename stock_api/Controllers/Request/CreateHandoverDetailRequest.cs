using stock_api.Models;

namespace stock_api.Controllers.Request
{
    public class CreateHandoverDetailRequest
    {
        public string Title { get; set; }

        public string? Content { get; set; }
        public List<RowDetail> RowDetails { get; set; } = new List<RowDetail>();

        public List<string> ReaderUserIds { get; set; } = new List<String>();
        public List<string> FileAttIds { get; set; } = new List<String>();
    }

    public class RowDetail
    {
        public int SheetRowId { get; set; }
        public string Status { get; set; }
        public string? Comment { get; set; }

        public HandoverSheetRow? HandoverSheetRowSetting { get; set; }
    }
}
