﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class PurchaseSubItemVo
    {
        public string ItemId { get; set; }
        public string Comment { get; set; }
        public string CompId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSpec { get; set; }
        public string PurchaseMainId { get; set; }
        public int? Quantity { get; set; }
        public int? ReceiveQuantity { get; set; }
        public string ReceiveStatus { get; set; }
        public List<string> GroupIds { get; set; }
        public List<string> GroupNames { get; set; }
        public string ProductCategory { get; set; }
        public string ArrangeSupplierId { get; set; }
        public string ArrangeSupplierName { get; set; }
        public int? CurrentInStockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}