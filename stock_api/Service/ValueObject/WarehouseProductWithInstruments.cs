using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class WarehouseProductWithInstruments:WarehouseProduct
    {
        public List<int> InstrumentIdList { get; set; } = new List<int>();
        public List<string> InstrumentNameList { get; set; } = new List<string>();  
    }
}
