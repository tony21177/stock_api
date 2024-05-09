using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class OutStockRecordVo : OutStockRecord
    {
        public string? Unit { get; set; }
    }
}
