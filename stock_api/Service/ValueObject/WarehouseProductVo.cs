using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class WarehouseProductVo : WarehouseProduct
    {
        public string? AnotherUnit {  get; set; }
        public float? AnotherUnitConversion { get; set; }
    }
}
