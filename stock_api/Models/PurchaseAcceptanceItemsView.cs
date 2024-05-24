﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

[Keyless]
public partial class PurchaseAcceptanceItemsView
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
    /// 設定此單據所屬的組別，參考 Warehouse_Group
    /// </summary>
    [StringLength(2000)]
    public string? GroupIds { get; set; }

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
    /// 送單到金萬林後，目前狀態
    /// NONE : 尚未收到結果
    /// DELIVERED : 金萬林已出貨
    /// IN_ACCEPTANCE_CHECK : 驗收中
    /// PART_ACCEPT : 部分驗收入庫
    /// ALL_ACCEPT : 全部驗收入庫
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

    [StringLength(100)]
    public string AcceptId { get; set; } = null!;

    /// <summary>
    /// 驗收接受數量，不可大於 OrderQuantity
    /// </summary>
    public int? AcceptQuantity { get; set; }

    /// <summary>
    /// 驗收允收者的UserID
    /// </summary>
    [Column("AcceptUserID")]
    [StringLength(100)]
    public string? AcceptUserId { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [StringLength(100)]
    public string? LotNumberBatch { get; set; }

    /// <summary>
    /// 批號
    /// </summary>
    [StringLength(100)]
    public string? LotNumber { get; set; }

    /// <summary>
    /// 保存期限
    /// </summary>
    public DateOnly? ExpirationDate { get; set; }

    /// <summary>
    /// 對應 PurchaseSubItem 的 PK
    /// </summary>
    [StringLength(100)]
    public string ItemId { get; set; } = null!;

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
    public string? PackagingStatus { get; set; }

    /// <summary>
    /// 品項PK
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
    /// 驗收測試品管結果
    /// PASS : 通過
    /// FAIL : 不通過
    /// NONEED : 不需側
    /// OTHER : 其他
    /// </summary>
    [StringLength(45)]
    public string? QcStatus { get; set; }

    /// <summary>
    /// 驗收入庫後，當下該品項的總庫存數量
    /// </summary>
    public int? CurrentTotalQuantity { get; set; }

    /// <summary>
    /// 初驗驗收填寫相關原因
    /// </summary>
    [StringLength(500)]
    public string? Comment { get; set; }

    /// <summary>
    /// 二次驗收填寫相關原因
    /// </summary>
    [StringLength(500)]
    public string? QcComment { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? AcceptCreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? AcceptUpdatedAt { get; set; }

    [StringLength(45)]
    public string? DeliverFunction { get; set; }

    public double? DeliverTemperature { get; set; }

    [StringLength(45)]
    public string? SavingFunction { get; set; }

    public double? SavingTemperature { get; set; }

    /// <summary>
    /// 入庫狀態
    /// NONE,PART,DONE
    /// </summary>
    [StringLength(45)]
    public string? InStockStatus { get; set; }
}