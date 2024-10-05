﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 主要是用來設定系統人員登入的相關資料。
/// </summary>
[Table("warehouse_member")]
[Index("Account", Name = "Account_UNIQUE", IsUnique = true)]
public partial class WarehouseMember
{
    [StringLength(100)]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 權限值
    /// </summary>
    public short AuthValue { get; set; }

    /// <summary>
    /// 顯示名稱
    /// </summary>
    [StringLength(100)]
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// 屬於數個組別
    /// </summary>
    [StringLength(2000)]
    public string? GroupIds { get; set; }

    /// <summary>
    /// 登入密碼
    /// </summary>
    [StringLength(100)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 大頭貼
    /// </summary>
    [Column("PhotoURL")]
    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// 屬於庫存系統裡面的哪一個組織所有
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    [Key]
    [StringLength(100)]
    public string UserId { get; set; } = null!;

    [Required]
    public bool? IsActive { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsAdmin { get; set; }

    /// <summary>
    /// 代理人userId,以逗號為分隔
    /// </summary>
    [StringLength(2000)]
    public string? Agents { get; set; }

    /// <summary>
    /// 代理人userName,以逗號為分隔
    /// </summary>
    [StringLength(2000)]
    public string? AgentNames { get; set; }

    public bool? IsNoStockReviewer { get; set; }
}