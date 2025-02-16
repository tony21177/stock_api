namespace stock_api.Controllers.Request
{

    public class OwnerPurchaseSubItemsOutRequest
    {
        public List<PurchaseSubOutItems> PurchaseSubOutItems { get; set; } = null!;
        public string? CompId { get; set; }

    }

    public class PurchaseSubOutItems
    {
        public string SubItemId { get; set; } = null!;
        public int OutQuantity { get; set; }
        public string? Remark { get; set; }
    }
}
