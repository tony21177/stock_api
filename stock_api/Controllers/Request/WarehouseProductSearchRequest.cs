namespace stock_api.Controllers.Request
{
    public class WarehouseProductSearchRequest: BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductMachine { get; set;}
        public int? OpenDeadline { get; set;}
        public string? GroupId { get; set;}
        public int? DefaultSupplierId { get; set; }
        public string? Keywords { get; set;}
    }
}
