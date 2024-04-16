namespace stock_api.Controllers.Request
{
    public class UpdatePurchaseItemSupplierRequest
    {
        public List<UpdateItem> UpdateItems { get; set; }
    }

    public class UpdateItem
    {
        public string ItemId { get; set; }
        public int ArrangeSupplierId { get; set; }
    }
    
}
