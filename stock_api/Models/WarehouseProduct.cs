﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 1. 庫存品項基本資料，因為要考慮到調撥這件事情，也就是從醫院單位A把項目移動部分庫存量到醫院單位B；所以，任一相同品項，在不同的醫院單位內的 ProductID 應該是一制，在這條件下，Product 表格的 PK 應是 CompID + ProductID
/// 2. 目前最大庫存量與最小安庫量跟庫存數量(InStockQuantity)這個欄位去做判斷就好 不要牽扯到TestCount 這樣感覺比較單純 ，TestCount這個欄位僅用來呈現目前尚存TEST用就好
/// 3. 用無條件進位的方式去轉換成訂購數量
/// 例如 單位訂購某品項10組 但UnitCoonversion欄位設定為4 則換算結果10/4=2.5 則無條件進位 意即訂購此品項變為3
/// </summary>
[PrimaryKey("ProductId", "CompId")]
[Table("warehouse_product")]
public partial class WarehouseProduct
{
    /// <summary>
    /// 批次
    /// </summary>
    [Required]
    [StringLength(100)]
    public string LotNumberBatch { get; set; }

    /// <summary>
    /// 批號
    /// </summary>
    [Required]
    [StringLength(100)]
    public string LotNumber { get; set; }

    /// <summary>
    /// 品項所屬的組織ID
    /// </summary>
    [Key]
    [StringLength(45)]
    public string CompId { get; set; }

    /// <summary>
    /// 品項所屬的製造商ID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ManufacturerId { get; set; }

    /// <summary>
    /// 品項所屬的製造商名稱
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ManufacturerName { get; set; }

    /// <summary>
    /// 有效期限規範
    /// </summary>
    [StringLength(500)]
    public string DeadlineRule { get; set; }

    /// <summary>
    /// 運送條件
    /// </summary>
    [StringLength(500)]
    public string DeliverFunction { get; set; }

    /// <summary>
    /// 運送備註
    /// </summary>
    [StringLength(500)]
    public string DeliverRemarks { get; set; }

    /// <summary>
    /// 屬於數個組別
    /// </summary>
    [StringLength(2000)]
    public string GroupIds { get; set; }

    /// <summary>
    /// 組別名稱
    /// </summary>
    [StringLength(2000)]
    public string GroupNames { get; set; }

    /// <summary>
    /// 庫存數量
    /// </summary>
    public int? InStockQuantity { get; set; }

    /// <summary>
    /// 管理者
    /// </summary>
    [StringLength(45)]
    public string Manager { get; set; }

    /// <summary>
    /// 最高安庫量
    /// </summary>
    public int? MaxSafeQuantity { get; set; }

    /// <summary>
    /// 最後可使用日期
    /// </summary>
    public DateOnly? LastAbleDate { get; set; }

    /// <summary>
    /// 最後出庫日期
    /// </summary>
    public DateOnly? LastOutStockDate { get; set; }

    /// <summary>
    /// 開封有效期限
    /// 數字（開封後可以用幾天），檢查資料庫是不是int
    /// </summary>
    public int? OpenDeadline { get; set; }

    /// <summary>
    /// 開封列印名稱
    /// </summary>
    [StringLength(100)]
    public string OpenedSealName { get; set; }

    /// <summary>
    /// 原始有效期限
    /// </summary>
    public DateOnly? OriginalDeadline { get; set; }

    /// <summary>
    /// 包裝方式
    /// </summary>
    [StringLength(100)]
    public string PackageWay { get; set; }

    public int? PreDeadline { get; set; }

    /// <summary>
    /// 前置天數
    /// </summary>
    public int? PreOrderDays { get; set; }

    /// <summary>
    /// 產品類別
    /// [耗材, 試劑, 其他]
    /// </summary>
    [StringLength(100)]
    public string ProductCategory { get; set; }

    /// <summary>
    /// 產品編碼
    /// </summary>
    [StringLength(200)]
    public string ProductCode { get; set; }

    [Key]
    [StringLength(100)]
    public string ProductId { get; set; }

    /// <summary>
    /// 品項型號
    /// </summary>
    [StringLength(200)]
    public string ProductModel { get; set; }

    /// <summary>
    /// 品項名稱
    /// </summary>
    [StringLength(200)]
    public string ProductName { get; set; }

    /// <summary>
    /// 品項備註
    /// </summary>
    [StringLength(300)]
    public string ProductRemarks { get; set; }

    /// <summary>
    /// 品項規格
    /// </summary>
    [StringLength(300)]
    public string ProductSpec { get; set; }

    /// <summary>
    /// 最小安庫量
    /// </summary>
    public int? SafeQuantity { get; set; }

    /// <summary>
    /// 儲存環境條件
    /// </summary>
    [StringLength(300)]
    public string SavingFunction { get; set; }

    /// <summary>
    /// UDI 碼
    /// </summary>
    [Column("UDIBatchCode")]
    [StringLength(300)]
    public string UdibatchCode { get; set; }

    /// <summary>
    /// UDI 碼
    /// </summary>
    [Column("UDICreateCode")]
    [StringLength(300)]
    public string UdicreateCode { get; set; }

    /// <summary>
    /// UDI 碼
    /// </summary>
    [Column("UDISerialCode")]
    [StringLength(300)]
    public string UdiserialCode { get; set; }

    /// <summary>
    /// UDI 碼
    /// </summary>
    [Column("UDIVerifyDateCode")]
    [StringLength(300)]
    public string UdiverifyDateCode { get; set; }

    /// <summary>
    /// 單位
    /// </summary>
    [StringLength(100)]
    public string Unit { get; set; }

    /// <summary>
    /// 重量
    /// </summary>
    [StringLength(100)]
    public string Weight { get; set; }

    /// <summary>
    /// 品項所屬儀器
    /// </summary>
    [StringLength(200)]
    public string ProductMachine { get; set; }

    /// <summary>
    /// 預設供應商
    /// </summary>
    [Column("DefaultSupplierID")]
    public int? DefaultSupplierId { get; set; }

    /// <summary>
    /// 預設供應商名稱
    /// </summary>
    [StringLength(200)]
    public string DefaultSupplierName { get; set; }

    /// <summary>
    /// 該品項出庫時，是否需要經過二次驗收
    /// </summary>
    public bool? IsNeedAcceptProcess { get; set; }

    /// <summary>
    /// 該品項期限距離現在的最小天數
    /// </summary>
    public int? AllowReceiveDateRange { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 用來在訂購時將最小單位轉為訂購規格及驗收時 將訂購規格轉為最小單位數量用的欄位
    /// </summary>
    public int UnitConversion { get; set; }

    /// <summary>
    /// 在總覽表與目前庫存數量(InStockQuantity)相乘顯示給使用者知道目前可用的數量用的欄位
    /// </summary>
    public int TestCount { get; set; }

    [Required]
    public bool? IsActive { get; set; }

    [StringLength(200)]
    public string StockLocation { get; set; }
}