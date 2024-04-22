namespace stock_api.Controllers.Request
{
    public class ListNotEnoughProductsRequest
    {
        public string? CompId { get; set; }
        public string? GroupId { get; set; }

        public string? ProductMachine { get; set; }
        public int? SupplierId { get; set;}
       
    }
}
