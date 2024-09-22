using stock_api.Models;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class InStockItemRecordNewLotNumberVo
    {
        public string ProductName { get; set; } = null!;
        public DateTime? InStockTime { get; set; }
        public float InStockQuantity { get; set; }
        public string LotNumberBatch { get; set; } = null!;
        public string? LotNumber { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? GroupIds { get; set; }
        public string? GroupNames { get; set; }
        public bool IsNewLotNumber { get; set; }
    }
}
