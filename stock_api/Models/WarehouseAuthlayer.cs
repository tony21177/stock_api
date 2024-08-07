﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 主要是用來設定登入人員的權限。
/// 一般組之內，設定人員權限時，僅有 3, 5, 7, 9 的 AuthValue 選項。
/// </summary>
[Table("warehouse_authlayer")]
public partial class WarehouseAuthlayer
{
    [Key]
    public int AuthId { get; set; }

    /// <summary>
    /// 權限描述
    /// </summary>
    [StringLength(100)]
    public string? AuthDescription { get; set; }

    /// <summary>
    /// 權限名稱
    /// </summary>
    [StringLength(100)]
    public string AuthName { get; set; } = null!;

    /// <summary>
    /// 權限值
    /// 1: 得標廠商
    /// 3: 最高層級
    /// 5: 第一層級
    /// 7: 第二層級
    /// 9: 第三層級
    /// </summary>
    public short AuthValue { get; set; }

    /// <summary>
    /// 是否可以申請新增品項
    /// </summary>
    [Required]
    public bool? IsApplyItemManage { get; set; }

    /// <summary>
    /// 是否可以進行群組管理
    /// </summary>
    [Required]
    public bool? IsGroupManage { get; set; }

    /// <summary>
    /// 是否可以進行入庫作業
    /// </summary>
    [Required]
    public bool? IsInBoundManage { get; set; }

    /// <summary>
    /// 是否可以進行出庫作業
    /// </summary>
    [Required]
    public bool? IsOutBoundManage { get; set; }

    /// <summary>
    /// 是否可以進行庫存管理
    /// </summary>
    [Required]
    public bool? IsInventoryManage { get; set; }

    /// <summary>
    /// 是否可以進行品項管理
    /// </summary>
    [Required]
    public bool? IsItemManage { get; set; }

    /// <summary>
    /// 是否可以進行成員管理
    /// </summary>
    [Required]
    public bool? IsMemberManage { get; set; }

    /// <summary>
    /// 是否可以進行盤點
    /// </summary>
    [Required]
    public bool? IsRestockManage { get; set; }

    /// <summary>
    /// 是否可以進行抽點
    /// </summary>
    [Required]
    public bool? IsVerifyManage { get; set; }

    /// <summary>
    /// 屬於庫存系統裡面的哪一個公司內所有\\n對應 -&gt; Company Table&quot;
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }
}