using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class InStockLots: InStockItemRecord
    {
        public float RemainingQuantity { get; set; }
        public string? ProductUnit { get; set; }
        public int? OpenDeadline { get; set; }
        public string? GroupName { get; set; }
        public string? ProductModel { get; set; }

    }
}
