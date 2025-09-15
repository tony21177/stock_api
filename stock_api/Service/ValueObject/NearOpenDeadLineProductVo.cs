using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class NearOpenDeadLineProductVo : WarehouseProduct
    {
        public List<OutStockItemForOpenDeadline> OutStockItemList { get; set; } = new();
        
    }
}
