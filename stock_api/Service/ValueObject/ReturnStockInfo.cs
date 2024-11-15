namespace stock_api.Service.ValueObject
{
    public class ReturnStockInfo
    {
        public float ReturnQuantity { get; set; }
        public float OutStockApplyQuantityBefore { get; set; }

        public float OutStockApplyQuantityAfter { get; set; }

        public float AfterQuantityBefore { get; set; }

        public float AfterQuantityAfter { get; set; }

        public DateTime ReturnStockDateTime { get; set; }
    }
}
