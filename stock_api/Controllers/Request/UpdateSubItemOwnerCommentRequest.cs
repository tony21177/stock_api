namespace stock_api.Controllers.Request
{
    public class UpdateSubItemOwnerCommentRequest
    {
        public string ItemId { get; set; } = null!; 

        public string OwnerComment { get; set; } = null!;
    }
}
