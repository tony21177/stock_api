﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

[Keyless]
public partial class PurchaseDetailView
{
    [StringLength(100)]
    public string PurchaseMainId { get; set; } = null!;

    /// <summary>
    /// 申請日期
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime ApplyDate { get; set; }

    /// <summary>
    /// 所屬公司ID
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    /// <summary>
    /// 目前狀態
    /// APPLY : 申請中
    /// AGREE : 同意
    /// REJECT : 拒絕
    /// CLOSE : 結案
    /// </summary>
    [StringLength(45)]
    public string CurrentStatus { get; set; } = null!;

    /// <summary>
    /// 需求日期
    /// </summary>
    public DateOnly? DemandDate { get; set; }

    /// <summary>
    /// 備註內容
    /// </summary>
    [StringLength(300)]
    public string? Remarks { get; set; }

    /// <summary>
    /// 此採購單據的建立者
    /// </summary>
    [StringLength(100)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// 送單到金萬林後，目前狀態\\nNONE : 系統處理中\\nDELIVERED : 得標廠商處理中\\nIN_ACCEPTANCE_CHECK :單位 驗收中\\nPART_ACCEPT : 部分驗收入庫\\nALL_ACCEPT : 全部驗收入庫
    /// </summary>
    [StringLength(100)]
    public string ReceiveStatus { get; set; } = null!;

    /// <summary>
    /// 採購單類型
    /// GENERAL : 一般訂單
    /// URGENT : 緊急訂單
    /// </summary>
    [StringLength(45)]
    public string? Type { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// NONE(所有sub_item都尚未經過OWNER拆單),PART(部分sub_item經過OWNER拆單),DONE(所有sub_item經過OWNER拆單)
    /// </summary>
    [StringLength(45)]
    public string? SplitPrcoess { get; set; }

    /// <summary>
    /// NONE,NOT_AGREE,PART_AGREE,AGREE
    /// </summary>
    [StringLength(45)]
    public string? OwnerProcess { get; set; }

    [StringLength(100)]
    public string ItemId { get; set; } = null!;

    /// <summary>
    /// 品項備註內容
    /// </summary>
    [StringLength(300)]
    public string? Comment { get; set; }

    /// <summary>
    /// 品項的PK，
    /// 參考 Product Table
    /// </summary>
    [StringLength(100)]
    public string ProductId { get; set; } = null!;

    [StringLength(200)]
    public string? ProductCode { get; set; }

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

    [Column("UDISerialCode")]
    [StringLength(300)]
    public string? UdiserialCode { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    public float? Quantity { get; set; }

    /// <summary>
    /// 已收到的數量
    /// </summary>
    public float? ReceiveQuantity { get; set; }

    /// <summary>
    /// 送單到金萬林後，目前狀態\\\\nNONE : 尚未收到結果\\\\nPART : 部分驗收入庫\\\\nDONE : 全部驗收入庫\\\\nCLOSE:金萬林不同意拆單後的採購項目\n
    /// </summary>
    [StringLength(45)]
    public string SubReceiveStatus { get; set; } = null!;

    /// <summary>
    /// 品項可以設定組別ID\n在醫院端可以依照組別拆單顯示
    /// </summary>
    [StringLength(2000)]
    public string? GroupIds { get; set; }

    [StringLength(2000)]
    public string? GroupNames { get; set; }

    /// <summary>
    /// 品項的 ProductCategory, 用來醫院拆單用
    /// </summary>
    [StringLength(45)]
    public string? ProductCategory { get; set; }

    /// <summary>
    /// 這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單
    /// </summary>
    public int? ArrangeSupplierId { get; set; }

    /// <summary>
    /// 這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單
    /// </summary>
    [StringLength(100)]
    public string? ArrangeSupplierName { get; set; }

    /// <summary>
    /// 採購單項目在建立當下的庫存數量
    /// </summary>
    public float? CurrentInStockQuantity { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime SubCreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime SubUpdatedAt { get; set; }

    /// <summary>
    /// Owner從拆單建立就會帶這個參數,表示從WithCompId的採購單拆單出來的
    /// </summary>
    [StringLength(100)]
    public string? WithCompId { get; set; }

    /// <summary>
    /// Owner從拆單建立就會帶這個參數,對應對WithCompId的採購單purchase_main_sheet.PurchaseMainId
    /// </summary>
    [Column("withPurchaseMainId")]
    [StringLength(100)]
    public string? WithPurchaseMainId { get; set; }

    /// <summary>
    /// Owner從拆單建立就會帶這個參數,對應對WithCompId的purchase_sub_item.ItemId
    /// </summary>
    [Column("withItemId")]
    [StringLength(100)]
    public string? WithItemId { get; set; }

    /// <summary>
    /// NONE(表示OWNER尚未拆單過), DONE(表示OWNER已經拆單過)
    /// 
    /// </summary>
    [StringLength(45)]
    public string? SubSplitProcess { get; set; }

    [StringLength(45)]
    public string? SubOwnerProcess { get; set; }

    public float? InStockQuantity { get; set; }
}