using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class NearExpiredProductVo:WarehouseProduct
    {
        public List<InStockItemRecord> InStockItemList { get; set; } = new();
    }
}
