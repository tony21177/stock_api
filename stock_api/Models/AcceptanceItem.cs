﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 各採購項目驗收紀錄
/// </summary>
[Table("acceptance_item")]
public partial class AcceptanceItem
{
    [Key]
    [StringLength(100)]
    public string AcceptId { get; set; }

    /// <summary>
    /// PurchaseMainSheet 的 PK
    /// </summary>
    [Required]
    [StringLength(100)]
    public string PurchaseMainId { get; set; }

    /// <summary>
    /// 驗收接受數量，不可大於 OrderQuantity
    /// </summary>
    public int? AcceptQuantity { get; set; }

    /// <summary>
    /// 驗收允收者的UserID
    /// </summary>
    [Column("AcceptUserID")]
    [StringLength(100)]
    public string AcceptUserId { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [StringLength(100)]
    public string LotNumberBatch { get; set; }

    /// <summary>
    /// 批號
    /// </summary>
    [StringLength(100)]
    public string LotNumber { get; set; }

    /// <summary>
    /// 所屬組織ID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CompId { get; set; }

    /// <summary>
    /// 保存期限
    /// </summary>
    public DateOnly? ExpirationDate { get; set; }

    /// <summary>
    /// 對應 PurchaseSubItem 的 PK
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ItemId { get; set; }

    /// <summary>
    /// 訂購數量，對應 PurchaseSubItem 的 Quantity
    /// </summary>
    public int OrderQuantity { get; set; }

    /// <summary>
    /// 外觀包裝
    /// NORMAL : 完成
    /// BREAK : 破損
    /// </summary>
    [StringLength(45)]
    public string PackagingStatus { get; set; }

    /// <summary>
    /// 品項PK
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ProductId { get; set; }

    /// <summary>
    /// 品項名稱
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ProductName { get; set; }

    /// <summary>
    /// 品項規格
    /// </summary>
    [Required]
    [StringLength(300)]
    public string ProductSpec { get; set; }

    [Column("UDISerialCode")]
    [StringLength(300)]
    public string UdiserialCode { get; set; }

    /// <summary>
    /// 驗收測試品管結果
    /// PASS : 通過
    /// FAIL : 不通過
    /// NONEED : 不需側
    /// OTHER : 其他
    /// </summary>
    [StringLength(45)]
    public string QcStatus { get; set; }

    /// <summary>
    /// 驗收入庫後，當下該品項的總庫存數量
    /// </summary>
    public int? CurrentTotalQuantity { get; set; }

    /// <summary>
    /// 初驗驗收填寫相關原因
    /// </summary>
    [StringLength(500)]
    public string Comment { get; set; }

    /// <summary>
    /// 二次驗收填寫相關原因
    /// </summary>
    [StringLength(500)]
    public string QcComment { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }
}