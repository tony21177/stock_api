﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 品項的採購單據主體
/// </summary>
[Table("purchase_main_sheet")]
public partial class PurchaseMainSheet
{
    [Key]
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

    [StringLength(300)]
    public string? OwnerComment { get; set; }
}