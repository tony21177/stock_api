﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 當使用者提出一筆申請品項時，ApplyNewProductMain 就會新增一筆資料。
/// </summary>
[Table("apply_new_product_main")]
public partial class ApplyNewProductMain
{
    [Key]
    [StringLength(100)]
    public string ApplyId { get; set; } = null!;

    /// <summary>
    /// 申請原因
    /// </summary>
    [StringLength(300)]
    public string? ApplyReason { get; set; }

    /// <summary>
    /// 申請備註內容
    /// </summary>
    [StringLength(300)]
    public string? ApplyRemarks { get; set; }

    /// <summary>
    /// 申請者的來源組織ID
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    /// <summary>
    /// 目前狀態\\\\nAPPLY : 申請中\\\\nAGREE : 同意\\\\nREJECT : 拒絕\\\\nCLOSE : 結案\\\\DONE:申請完成
    /// </summary>
    [StringLength(45)]
    public string CurrentStatus { get; set; } = null!;

    /// <summary>
    /// 品名
    /// </summary>
    [StringLength(200)]
    public string ApplyProductName { get; set; } = null!;

    /// <summary>
    /// 規格
    /// </summary>
    [StringLength(200)]
    public string? ApplyProductSpec { get; set; }

    /// <summary>
    /// 申請數量
    /// </summary>
    public float? ApplyQuantity { get; set; }

    /// <summary>
    /// 品項組別
    /// </summary>
    [StringLength(100)]
    public string? ProductGroupId { get; set; }

    /// <summary>
    /// 品項組別名
    /// </summary>
    [StringLength(100)]
    public string? ProductGroupName { get; set; }

    /// <summary>
    /// 申請者的UserID
    /// 對應 Member Table.
    /// </summary>
    [StringLength(100)]
    public string UserId { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }
}