using stock_api.Models;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class OutStockItemForOpenDeadline
    {
        public float? OutStockQuantity { get; set; } 
        public string? LotNumberBatch { get; set; }
        public string? LotNumber { get; set; }
        public string? ProductName { get; set; }
        public string? ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductSpec { get; set; }    
        public string? Type { get; set; }
        public DateTime? OutStockDate { get; set; }
        public int? OpenDeadline { get; set; }
        public int? RemainingDays { get; set; }
        public string? GroupIds { get; set; }
        public string? GroupNames { get; set; }
        public int? DefaultSupplierId { get; set; }
        public string? DefaultSupplierName { get; set; }
        public string? PackageWay { get; set; }
    }
}
