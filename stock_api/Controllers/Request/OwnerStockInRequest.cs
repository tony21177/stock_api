using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
   

    public class OwnerStockInRequest
    {
        public string? CompId { get; set; }
        public string ProductId { get; set; } = null!;
        public float Quantity { get; set; }

        public string? ExpirationDate { get; set; }
        public string? Comment { get; set; }
    }
}
