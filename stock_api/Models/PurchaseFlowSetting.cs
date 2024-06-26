﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 系統先行設定採購的審核流程
/// </summary>
[Table("purchase_flow_setting")]
public partial class PurchaseFlowSetting
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
    /// 流程名稱
    /// </summary>
    [StringLength(100)]
    public string FlowName { get; set; } = null!;

    /// <summary>
    /// 流程順序
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// 此審核流程的審核者
    /// </summary>
    [StringLength(100)]
    public string UserId { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [Required]
    public bool? IsActive { get; set; }
}