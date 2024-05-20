namespace stock_api.Controllers.Request
{
    public class UpdateOrDeleteSubItemInFlowRequest
    {
        public string PurchaseMainId { get; set; } = null!;

        public List<UpdateSubItem> UpdateSubItemList { get; set; } = null!;
        public List<string> DeleteSubItemIdList { get; set; } = null!;

    }

    public class UpdateSubItem
    {
        public string ItemId { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
