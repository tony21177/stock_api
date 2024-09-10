﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

[Table("qc_acceptance_detail")]
public partial class QcAcceptanceDetail
{
    [Key]
    public int AcceptanceDetailId { get; set; }

    [StringLength(100)]
    public string MainId { get; set; } = null!;

    public int ItemNumber { get; set; }

    /// <summary>
    /// 新批號結果
    /// </summary>
    [StringLength(300)]
    public string? NewLotResult { get; set; }

    [StringLength(300)]
    public string? AcceptRange { get; set; }

    /// <summary>
    /// PASS,FAIL,ND
    /// </summary>
    [StringLength(300)]
    public string? ValidationResult { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }
}