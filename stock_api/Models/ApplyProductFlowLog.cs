﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// *假設今天有人員異動，造成申請單據的審核流程卡住，系統會提供一個畫面給該院區管理者，管理者進入後得強制進行審核。
/// </summary>
[Table("apply_product_flow_log")]
public partial class ApplyProductFlowLog
{
    [Key]
    [StringLength(100)]
    public string LogId { get; set; }

    /// <summary>
    /// 申請者的來源組織ID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CompId { get; set; }

    /// <summary>
    /// 對應 ApplyNewProductMain PK
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ApplyId { get; set; }

    /// <summary>
    /// 操作者的UserID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserId { get; set; }

    /// <summary>
    /// 操作者名稱
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserName { get; set; }

    /// <summary>
    /// 動作
    /// NEXT : 下一步
    /// PREV : 上一步
    /// CLOSE : 結案
    /// </summary>
    [Required]
    [StringLength(45)]
    public string Action { get; set; }

    /// <summary>
    /// 備註內容
    /// </summary>
    [Required]
    [StringLength(300)]
    public string Remarks { get; set; }

    /// <summary>
    /// 流程順序
    /// </summary>
    public int Sequence { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; }
}