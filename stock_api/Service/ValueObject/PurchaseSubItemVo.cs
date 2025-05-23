﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class PurchaseSubItemVo
    {
        public string ItemId { get; set; }
        public string Comment { get; set; }
        public string CompId { get; set; }
        public string? CompName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSpec { get; set; }
        public string PurchaseMainId { get; set; }
        public float? Quantity { get; set; }
        public float? ReceiveQuantity { get; set; }
        public string ReceiveStatus { get; set; }
        public List<string> GroupIds { get; set; }
        public List<string> GroupNames { get; set; }
        public string ProductCategory { get; set; }
        public int? ArrangeSupplierId { get; set; }
        public string ArrangeSupplierName { get; set; }
        public float? CurrentInStockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public float? MaxSafeQuantity { get; set; }
        public string ProductModel { get; set; } 
        public string ManufacturerName { get; set; }
        public string ProductMachine { get; set; }
        public string ProductUnit { get; set; }
        public float? UnitConversion { get; set; }
        public float? TestCount { get; set; }
        public string PackageWay { get; set; }
        public string ProductCode { get; set; }
        public float? SupplierUnitConvertsion { get; set; }
        public string? SupplierUnit { get; set; }
        public string? SupplierSpec { get; set; }
        public string? Delivery { get; set; }

        public string? SplitProcess { get; set; }
        public string? OwnerProcess { get; set; }
        public string? OpenedSealName { get; set; }
        public string? WithCompId { get; set; }
        public string? WithPurchaseMainId { get; set; }

        public string? WithCompName { get; set; }
        public string? WithItemId { get; set; }

        public string? StockLocation { get; set;}
        public string? Manager {  get; set; }
        public double? LastMonthUsageQuantity { get; set; } = 0;
        public double? ThisYearAverageMonthUsageQuantity { get; set; } = 0;
        public string? OwnerComment { get; set; }
    }
}
