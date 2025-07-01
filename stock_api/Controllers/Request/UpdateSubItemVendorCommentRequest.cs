namespace stock_api.Controllers.Request
{
    public class UpdateSubItemVendorCommentRequest
    {
        public string ItemId { get; set; } = null!; 

        public string VendorComment { get; set; } = null!;
    }
}
