using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class OutStockRecordVo : OutStockRecord
    {
        public string? Unit { get; set; }
        public int OpenDeadline { get; set; }
        public string? ProductModel { get; set; }

        public List<ReturnStockInfo> ReturnStockInfoList { get; set; } = new List<ReturnStockInfo>();
    }
}
