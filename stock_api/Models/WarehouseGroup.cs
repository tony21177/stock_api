﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 主要是用來設定「人員」及「品項」可以分類為哪些組別來使用的，在新增品項和新增人員的時候，都可以從下拉選單中，選擇已建立的組別來加以管理。
/// </summary>
[Table("warehouse_group")]
public partial class WarehouseGroup
{
    [Key]
    [StringLength(100)]
    public string GroupId { get; set; } = null!;

    /// <summary>
    /// 群組名稱
    /// </summary>
    [StringLength(45)]
    public string? GroupName { get; set; }

    /// <summary>
    /// 群組描述
    /// </summary>
    [StringLength(100)]
    public string? GroupDescription { get; set; }

    /// <summary>
    /// 是否激活狀態
    /// </summary>
    [Required]
    public bool? IsActive { get; set; }

    /// <summary>
    /// 屬於庫存系統裡面的哪一個組織所有
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }
}