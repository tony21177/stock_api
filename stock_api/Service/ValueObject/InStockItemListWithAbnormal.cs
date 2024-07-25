using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class InStockItemListWithAbnormal:InStockItemRecord
    {
        public SupplierTraceLog? SupplierTraceLog { get; set; }
    }
}
