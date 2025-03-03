﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 退貨紀錄
/// </summary>
[Table("reject_item_records")]
public partial class RejectItemRecord
{
    [Key]
    [StringLength(100)]
    public string RejectId { get; set; } = null!;

    [StringLength(100)]
    public string? PurchaseMainId { get; set; }

    [StringLength(100)]
    public string? SubItemId { get; set; }

    [StringLength(100)]
    public string InStockId { get; set; } = null!;

    [StringLength(100)]
    public string? LotNumberBatch { get; set; }

    [StringLength(100)]
    public string LotNumber { get; set; } = null!;

    [StringLength(100)]
    public string CompId { get; set; } = null!;

    /// <summary>
    /// 退庫前庫存數量
    /// </summary>
    public float StockQuantityBefore { get; set; }

    /// <summary>
    /// 退庫後庫存數量
    /// </summary>
    public float StockQuantityAfter { get; set; }

    /// <summary>
    /// 當初入庫數量
    /// </summary>
    public float InStockQuantity { get; set; }

    public float RejectQuantity { get; set; }

    [StringLength(100)]
    public string ProductId { get; set; } = null!;

    [StringLength(200)]
    public string ProductCode { get; set; } = null!;

    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [StringLength(200)]
    public string? ProductSpec { get; set; }

    [StringLength(100)]
    public string RejectUserId { get; set; } = null!;

    [StringLength(100)]
    public string RejectUserName { get; set; } = null!;

    [StringLength(100)]
    public string? InStockUserId { get; set; }

    [StringLength(100)]
    public string? InStockUserName { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [StringLength(200)]
    public string? SupplierName { get; set; }

    public DateOnly? RejectDate { get; set; }

    [StringLength(1000)]
    public string? RejectReason { get; set; }
}