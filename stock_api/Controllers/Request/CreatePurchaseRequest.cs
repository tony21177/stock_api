namespace stock_api.Controllers.Request
{
    public class CreatePurchaseRequest
    {
        public string? CompId { get; set; }

        public string? DemandDate { get; set; }
        public List<string>? GroupIds { get; set; }

        public string Remarks { get; set; }

        public string Type { get; set; }

        public List<PurchaseSubItem> PurchaseSubItems { get; set; }
    }


    public class PurchaseSubItem
    {
        public string? Comment { get; set; }

        public string ProductId { get; set; }

        public int Quantity { get; set; }

        public List<string>? GroupIds { get; set; }
    }
}
