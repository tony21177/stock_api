﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 當使用者填寫品質確效時，qc_flow 就會新增一筆資料，且每一次變動就會在此表格寫入一筆資料留下審核紀錄。
/// </summary>
[Table("qc_flow")]
public partial class QcFlow
{
    [Key]
    [StringLength(100)]
    public string FlowId { get; set; } = null!;

    /// <summary>
    /// 對應 qc_validation_main PK
    /// </summary>
    [StringLength(100)]
    public string MainId { get; set; } = null!;

    /// <summary>
    /// 回覆結果\nAGREE : 同意\nREJECT : 不同意\n空白 : 未回應
    /// </summary>
    [StringLength(45)]
    public string Answer { get; set; } = null!;

    [StringLength(300)]
    public string? Reason { get; set; }

    /// <summary>
    /// 申請者的來源組織ID
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    /// <summary>
    /// 當下該單據狀態
    /// </summary>
    [StringLength(45)]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 審核此單據的組織ID
    /// </summary>
    [Column("ReviewCompID")]
    [StringLength(100)]
    public string ReviewCompId { get; set; } = null!;

    /// <summary>
    /// 審核此單據的UserID
    /// </summary>
    [StringLength(100)]
    public string? ReviewUserId { get; set; }

    /// <summary>
    /// 審核此單據的UserName
    /// </summary>
    [StringLength(100)]
    public string? ReviewUserName { get; set; }

    /// <summary>
    /// 負責簽核的組別
    /// </summary>
    [StringLength(100)]
    public string? ReviewGroupId { get; set; }

    [StringLength(100)]
    public string? ReviewGroupName { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 送出時間
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime? SubmitAt { get; set; }

    public int Sequence { get; set; }
}