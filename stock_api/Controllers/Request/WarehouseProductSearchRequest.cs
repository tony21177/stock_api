namespace stock_api.Controllers.Request
{
    public class WarehouseProductSearchRequest: BaseSearchRequest
    {
        public string? ProductCategory { get; set; }
        public string? ProductMachine { get; set;}
        public int? OpenDeadline { get; set;}
        public string? GroupIds { get; set;}

        public string? Keywords { get; set;}
    }
}
