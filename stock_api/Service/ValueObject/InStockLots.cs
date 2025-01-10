using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class InStockLots: InStockItemRecord
    {
        public float RemainingQuantity { get; set; }
    }
}
