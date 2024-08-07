﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// *假設今天有人員異動，造成採購單據的審核流程卡住，系統會提供一個畫面給該院區管理者，管理者進入後得強制進行審核。
/// </summary>
[Table("purchase_flow_log")]
public partial class PurchaseFlowLog
{
    [Key]
    [StringLength(100)]
    public string LogId { get; set; } = null!;

    /// <summary>
    /// 所屬組織ID
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    /// <summary>
    /// PurchaseMainSheet PK
    /// </summary>
    [StringLength(100)]
    public string PurchaseMainId { get; set; } = null!;

    /// <summary>
    /// 紀錄操作者的UserID
    /// </summary>
    [StringLength(100)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// 紀錄操作者的UserName
    /// </summary>
    [StringLength(100)]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 流程順序
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// 動作
    /// NEXT : 下一步
    /// PREV : 上一步
    /// CLOSE : 結案
    /// </summary>
    [StringLength(45)]
    public string Action { get; set; } = null!;

    /// <summary>
    /// 備註內容
    /// </summary>
    [StringLength(300)]
    public string? Remarks { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }

    [Column(TypeName = "json")]
    public string? BeforeSubItems { get; set; }

    [Column(TypeName = "json")]
    public string? AfterSubItems { get; set; }
}