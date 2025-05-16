using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class NearExpiredProductVo:WarehouseProduct
    {
        public List<InStockItemRecord> InStockItemList { get; set; } = new();

        public double? NearExpiredQuantity { get; set; } = 0;
        public List<string> NearExpiredLotNumber { get; set; } = new();
        public List<string> NearExpiredLotNumberBatch { get; set; } = new();
    }
}
