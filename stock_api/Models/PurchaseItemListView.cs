﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

[Keyless]
public partial class PurchaseItemListView
{
    [Required]
    [StringLength(100)]
    public string PurchaseMainId { get; set; }

    /// <summary>
    /// 申請日期
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime ApplyDate { get; set; }

    /// <summary>
    /// 所屬公司ID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CompId { get; set; }

    /// <summary>
    /// 目前狀態
    /// APPLY : 申請中
    /// AGREE : 同意
    /// REJECT : 拒絕
    /// CLOSE : 結案
    /// </summary>
    [Required]
    [StringLength(45)]
    public string CurrentStatus { get; set; }

    /// <summary>
    /// 需求日期
    /// </summary>
    public DateOnly? DemandDate { get; set; }

    /// <summary>
    /// 設定此單據所屬的組別，參考 Warehouse_Group
    /// </summary>
    [StringLength(2000)]
    public string GroupIds { get; set; }

    /// <summary>
    /// 備註內容
    /// </summary>
    [StringLength(300)]
    public string Remarks { get; set; }

    /// <summary>
    /// 此採購單據的建立者
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserId { get; set; }

    /// <summary>
    /// 送單到金萬林後，目前狀態
    /// NONE : 尚未收到結果
    /// DELIVERED : 金萬林已出貨
    /// IN_ACCEPTANCE_CHECK : 驗收中
    /// PART_ACCEPT : 部分驗收入庫
    /// ALL_ACCEPT : 全部驗收入庫
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ReceiveStatus { get; set; }

    /// <summary>
    /// 採購單類型
    /// GENERAL : 一般訂單
    /// URGENT : 緊急訂單
    /// </summary>
    [StringLength(45)]
    public string Type { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    [Required]
    [StringLength(100)]
    public string ItemId { get; set; }

    /// <summary>
    /// 品項備註內容
    /// </summary>
    [StringLength(300)]
    public string Comment { get; set; }

    /// <summary>
    /// 品項的PK，
    /// 參考 Product Table
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

    /// <summary>
    /// 數量
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// 已收到的數量
    /// </summary>
    public int? ReceiveQuantity { get; set; }

    /// <summary>
    /// 送單到金萬林後，目前狀態
    /// NONE : 尚未收到結果
    /// PART : 部分驗收入庫
    /// DONE : 全部驗收入庫
    /// </summary>
    [Required]
    [StringLength(45)]
    public string ItemReceiveStatus { get; set; }

    /// <summary>
    /// 品項可以設定組別ID\n在醫院端可以依照組別拆單顯示
    /// </summary>
    [StringLength(2000)]
    public string ItemGroupIds { get; set; }

    [StringLength(2000)]
    public string ItemGroupNames { get; set; }

    /// <summary>
    /// 品項的 ProductCategory, 用來醫院拆單用
    /// </summary>
    [StringLength(45)]
    public string ProductCategory { get; set; }

    /// <summary>
    /// 這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單
    /// </summary>
    public int? ArrangeSupplierId { get; set; }

    /// <summary>
    /// 這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單
    /// </summary>
    [StringLength(100)]
    public string ArrangeSupplierName { get; set; }

    /// <summary>
    /// 採購單項目在建立當下的庫存數量
    /// </summary>
    public int? CurrentInStockQuantity { get; set; }
}