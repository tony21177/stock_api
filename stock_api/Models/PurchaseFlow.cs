﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 品項的採購單據流程審核紀錄
/// </summary>
[Table("purchase_flow")]
[Index("VerifyUserId", Name = "verify_user_idx")]
public partial class PurchaseFlow
{
    [Key]
    [StringLength(100)]
    public string FlowId { get; set; } = null!;

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
    /// 流程備註內容
    /// </summary>
    [StringLength(300)]
    public string? Reason { get; set; }

    /// <summary>
    /// 當下該單據狀態WAIT,AGREE,REJECT
    /// </summary>
    [StringLength(45)]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 審核人員所屬公司ID
    /// </summary>
    [Column("VerifyCompID")]
    [StringLength(100)]
    public string VerifyCompId { get; set; } = null!;

    /// <summary>
    /// 審核人員的UserID
    /// </summary>
    [StringLength(100)]
    public string VerifyUserId { get; set; } = null!;

    /// <summary>
    /// 審核人員的UserName
    /// </summary>
    [StringLength(100)]
    public string VerifyUserName { get; set; } = null!;

    /// <summary>
    /// 回覆結果
    /// AGREE : 同意
    /// REJECT : 不同意
    /// 空白 : 未回應
    /// </summary>
    [StringLength(45)]
    public string Answer { get; set; } = null!;

    /// <summary>
    /// 流程順序
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// 讀取時間
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 送出時間
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime? SubmitAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }
}