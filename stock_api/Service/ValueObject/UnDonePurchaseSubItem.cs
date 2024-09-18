using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class UnDonePurchaseSubItem:PurchaseSubItem
    {
        public PurchaseMainSheet PurchaseMain { get; set; } = null!;
        public string? Unit {  get; set; }
    }
}
