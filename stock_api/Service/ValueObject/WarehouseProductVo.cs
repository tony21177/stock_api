﻿using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class WarehouseProductVo : WarehouseProduct
    {
        public string? AnotherUnit {  get; set; }
        public float? AnotherUnitConversion { get; set; }
        public List<InStockItemRecord> InStockRecords { get; set; } = new List<InStockItemRecord>();
        public List<OutStockRecord> OutStockRecords { get; set; } = new List<OutStockRecord>();
    }
}
