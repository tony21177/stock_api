﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 每一筆要更新庫存紀錄（增加）的操作，都需要寫入一筆記錄在 InStockRecord，包含採購驗收、調撥、盤點（盤盈）、退庫，類型寫在 Type 欄位。
/// </summary>
[Table("in_stock_item_record")]
public partial class InStockItemRecord
{
    [Key]
    [StringLength(100)]
    public string InStockId { get; set; } = null!;

    /// <summary>
    /// 批次
    /// </summary>
    [StringLength(100)]
    public string LotNumberBatch { get; set; } = null!;

    /// <summary>
    /// 批號
    /// </summary>
    [StringLength(100)]
    public string LotNumber { get; set; } = null!;

    /// <summary>
    /// 所屬公司ID
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    /// <summary>
    /// 現有庫存量
    /// </summary>
    public float OriginalQuantity { get; set; }

    /// <summary>
    /// 保存期限
    /// </summary>
    public DateOnly? ExpirationDate { get; set; }

    /// <summary>
    /// 對應 PurchaseSubItem 的 PK
    /// 非採購入庫，NULL
    /// </summary>
    [StringLength(100)]
    public string? ItemId { get; set; }

    /// <summary>
    /// 此次入庫數量
    /// </summary>
    public float InStockQuantity { get; set; }

    /// <summary>
    /// 品項PK
    /// </summary>
    [StringLength(100)]
    public string ProductId { get; set; } = null!;

    [StringLength(200)]
    public string ProductCode { get; set; } = null!;

    /// <summary>
    /// 品項名稱
    /// </summary>
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// 品項規格
    /// </summary>
    [StringLength(300)]
    public string ProductSpec { get; set; } = null!;

    /// <summary>
    /// 類型
    /// PURCHASE : 來源是採購
    /// SHIFT : 調撥
    /// ADJUST : 調整（盤盈）
    /// RETURN : 退庫
    /// </summary>
    [StringLength(45)]
    public string Type { get; set; } = null!;

    /// <summary>
    /// 用來產生條碼的數字，PadLeft : 7個0
    /// Example : 0000001
    /// </summary>
    [StringLength(200)]
    public string BarCodeNumber { get; set; } = null!;

    /// <summary>
    /// 執行入庫人員的UserID
    /// </summary>
    [StringLength(100)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// 執行入庫人員的UserName
    /// </summary>
    [StringLength(100)]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 入庫後數量
    /// </summary>
    public float AfterQuantity { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [StringLength(45)]
    public string? DeliverFunction { get; set; }

    public double? DeliverTemperature { get; set; }

    [StringLength(45)]
    public string? SavingFunction { get; set; }

    public double? SavingTemperature { get; set; }

    /// <summary>
    /// 出庫的狀態\\nNONE:都還沒出,PART:出部分:ALL:出完全部
    /// </summary>
    [StringLength(45)]
    public string? OutStockStatus { get; set; }

    public float? OutStockQuantity { get; set; }
}