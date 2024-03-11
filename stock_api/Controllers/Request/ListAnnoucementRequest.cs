namespace stock_api.Controllers.Request
{
    public class ListAnnoucementRequest
    {
        public bool IsPagination { get; set; } = false;
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }

        public string OrderBy { get; set; } = "id";
        public bool IsAsc { get; set; } = false;
        public string? CreatorID { get; set; }
        public bool? IsActive { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
