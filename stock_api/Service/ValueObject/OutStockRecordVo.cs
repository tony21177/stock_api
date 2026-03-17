using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class OutStockRecordVo : OutStockRecord
    {
        public List<ReturnStockInfo> ReturnStockInfoList { get; set; } = new List<ReturnStockInfo>();

        public List<string> DiscardUserNameList { get; set; } = new List<string>();
        public List<string?> DiscardReasonList { get; set; } = new List<string?>();
        public List<DateTime> DiscardTimeList { get; set; } = new List<DateTime>();
        public List<float> DiscardQuantityList { get; set; } = new List<float>();
        public string? InstrumentName { get; set; }
    }
}
