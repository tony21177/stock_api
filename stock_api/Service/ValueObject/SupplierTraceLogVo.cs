using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class SupplierTraceLogVo: SupplierTraceLog
    {
        public string? PurchaseMainId { get; set; }
    }
}
