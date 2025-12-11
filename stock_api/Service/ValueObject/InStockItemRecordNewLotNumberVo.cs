using stock_api.Models;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class InStockItemRecordNewLotNumberVo
    {
        public string ProductCode {get;set;}
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
        public string ProductId { get; set; }
        public string? ProductUnit {get;set;}
        public string? ProductModel { get; set; }
        public int? OpenDeadline { get; set; }
        public string? SavingFunction { get; set; }
        public double? SavingTemperature { get; set; }
        public string? ExpirationDate { get; set; }
        public string? PackagingStatus { get; set; }
        public float? OrderQuantity { get; set; }
        public int? DeadlineRule { get; set; }

        public string? ItemId { get; set; }
    }
}
