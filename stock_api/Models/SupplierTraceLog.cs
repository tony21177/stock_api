﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

[Table("supplier_trace_log")]
public partial class SupplierTraceLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(100)]
    public string CompId { get; set; } = null!;

    /// <summary>
    /// RECEIVE_ABNORMAL:收貨異常,
    /// VERIFY_ABNORMAL:驗收異常,
    /// QA_ABNORMAL:品管異常,
    /// OTHER_ABNORMAL:其他異常
    /// 
    /// </summary>
    [StringLength(45)]
    public string AbnormalType { get; set; } = null!;

    public int? SupplierId { get; set; }

    [StringLength(100)]
    public string? SupplierName { get; set; }

    [Column(TypeName = "text")]
    public string? AbnormalContent { get; set; }

    [StringLength(45)]
    public string? UserId { get; set; }

    [StringLength(45)]
    public string? UserName { get; set; }

    [StringLength(200)]
    public string? SourceId { get; set; }

    /// <summary>
    /// IN_STOCK(入庫),QA(品質確效),MANUAL(手動登陸)
    /// </summary>
    [StringLength(100)]
    public string? SourceType { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? AbnormalDate { get; set; }

    [StringLength(100)]
    public string? ProductId { get; set; }

    [StringLength(200)]
    public string? ProductName { get; set; }

    [StringLength(100)]
    public string? PurchaseMainId { get; set; }

    [StringLength(100)]
    public string? LotNumber { get; set; }

    [StringLength(100)]
    public string? LotNumberBatch { get; set; }
}