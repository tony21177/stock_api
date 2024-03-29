﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
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
    public string ApplyId { get; set; }

    /// <summary>
    /// 申請數量
    /// </summary>
    public int ApplyQuantity { get; set; }

    /// <summary>
    /// 申請原因
    /// </summary>
    [StringLength(300)]
    public string ApplyReason { get; set; }

    /// <summary>
    /// 申請備註內容
    /// </summary>
    [StringLength(300)]
    public string ApplyRemarks { get; set; }

    /// <summary>
    /// 申請者的來源組織ID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CompId { get; set; }

    /// <summary>
    /// 目前狀態
    /// </summary>
    [Required]
    [StringLength(45)]
    public string CurrentStatus { get; set; }

    /// <summary>
    /// 申請品項名稱
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ProductName { get; set; }

    /// <summary>
    /// 申請者的UserID
    /// 對應 Member Table.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserId { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }
}