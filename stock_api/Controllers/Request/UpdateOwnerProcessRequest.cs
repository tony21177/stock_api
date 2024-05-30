namespace stock_api.Controllers.Request
{
    public class UpdateOwnerProcessRequest
    {
        public string PurchaseMainId { get; set; }

        public string OwnerProcess { get; set; }

        public List<string>? ItemIds { get; set; }
    }
}
