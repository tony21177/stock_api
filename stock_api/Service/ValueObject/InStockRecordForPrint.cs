using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class InStockRecordForPrint : InStockItemRecord
    {
        public string? Unit {  get; set; }
    }
}
