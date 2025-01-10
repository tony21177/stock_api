using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class NotEnoughQuantityProduct:WarehouseProduct
    {
        public float InProcessingOrderQuantity { get; set; } = 0;
        public float NeedOrderedQuantity { get; set; } = 0;

        public float NeedUnorderedQuantityUnit { get; set; } = 0;
        public double ThisYearAverageMonthUsageQuantity { get; set; } = 0;
    }
}
