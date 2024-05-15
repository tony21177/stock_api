using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class WarehouseProductVo : WarehouseProduct
    {
        public string? AnotherUnit {  get; set; }
        public int? AnotherUnitConversion { get; set; }
    }
}
