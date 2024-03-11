namespace stock_api.Controllers.Request
{
    public class UpdateHandoverDetailRequest
    {
        public string HandoverDetailId { get; set; }
        public string Title { get; set; }

        public string? Content { get; set; }
        public List<RowDetail> RowDetails { get; set; } = new List<RowDetail>();

        public List<string> ReaderUserIds { get; set; } = new List<String>();
        public List<string> FileAttIds { get; set; } = new List<String>();
    }


}
