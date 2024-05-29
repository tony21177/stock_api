﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 盤點和調撥單據，如果是要入庫的話，會先寫入一筆資料到這個暫時的表，等到審核通過再複製到 InStockItemRecord.
/// </summary>
[Table("temp_in_stock_item_record")]
public partial class TempInStockItemRecord
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
    /// 用來判斷暫存項目是不是已經有複製過去 InStockItemRecord,(0)false: 未複製,(1)true: 已複製
    /// </summary>
    public bool IsTransfer { get; set; }

    /// <summary>
    /// 用來判斷屬於哪一張主單
    /// </summary>
    [Column("InventoryID")]
    [StringLength(100)]
    public string InventoryId { get; set; } = null!;

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
}