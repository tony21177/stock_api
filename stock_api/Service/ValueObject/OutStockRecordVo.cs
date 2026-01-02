using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class OutStockRecordVo : OutStockRecord
    {
        public string? Unit { get; set; }
        public int? OpenDeadline { get; set; }
        public string? ProductModel { get; set; }

        public List<ReturnStockInfo> ReturnStockInfoList { get; set; } = new List<ReturnStockInfo>();

        public List<string> DiscardUserNameList { get; set; } = new List<string>();
        public List<string?> DiscardReasonList { get; set; } = new List<string?>();
        public List<DateTime> DiscardTimeList { get; set; } = new List<DateTime>();
        public List<float> DiscardQuantityList { get; set; } = new List<float>();
        public  bool? IsAllowDiscard { get; set; } 
        public string? InstrumentName { get; set; } 
        public string? GroupIds { get; set; }
        public string? GroupNames { get; set; }
    }
}
