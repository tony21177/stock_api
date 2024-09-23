using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class PurchaseSubItemWithUnit:PurchaseSubItem
    {
        public string? ProductUnit {  get; set; }
    }
}
