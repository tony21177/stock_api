using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class WarehouseProductStockOutViewWithInstruments: WarehouseProductStockOutView
    {
        public List<int> InstrumentIdList { get; set; } = new List<int>();
        public List<string> InstrumentNameList { get; set; } = new List<string>();
    }
}
