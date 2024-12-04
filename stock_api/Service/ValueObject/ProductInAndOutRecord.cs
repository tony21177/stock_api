using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class ProductInAndOutRecord
    {
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string? ProductModel { get; set; } = null!;
        public List<InStockItemRecordVo> InStockItemRecords { get; set; } = new();
        public List<OutStockRecordVo> OutStockRecords { get; set; } = new();
    }
}
