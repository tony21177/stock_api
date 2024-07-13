namespace stock_api.Controllers.Request
{
    public class SearchPurchaseAcceptItemRequest
    {
        public string? CompId { get; set; }
        public string? ReceiveStatus { get; set; }
        public List<string>? ReceiveStatusList { get; set; }
        public List<string>? InStockStatusList { get; set; }
        public string? DemandDateStart { get; set; }
        public string? DemandDateEnd { get; set; }
        public string? ApplyDateStart { get; set; }
        public string? ApplyDateEnd { get; set; }
        public string? GroupId { get; set; }
        public string? Type { get; set; }
        public string? Keywords { get; set; }
        public string? PurchaseMainId { get; set; }

        public bool? IsGroupBySupplier { get; set; } = false;
        public int? SupplierId { get; set; }

    }
}
