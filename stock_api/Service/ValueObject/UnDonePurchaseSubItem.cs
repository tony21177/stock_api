using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class UnDonePurchaseSubItem:PurchaseItemListView
    {
        public PurchaseMainSheet PurchaseMain { get; set; } = null!;
        public string? Unit {  get; set; }
    }
}
