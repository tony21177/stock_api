using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class SupplierTraceLogWithInStock:SupplierTraceLog
    {
        public List<InStockItemRecord> InStockItems { get; set; } = new();
    }
}
