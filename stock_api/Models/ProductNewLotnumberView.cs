﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

[Keyless]
public partial class ProductNewLotnumberView
{
    /// <summary>
    /// 品項PK
    /// </summary>
    [StringLength(100)]
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 批號
    /// </summary>
    [StringLength(100)]
    public string? LotNumber { get; set; }

    /// <summary>
    /// 批次
    /// </summary>
    [StringLength(100)]
    public string LotNumberBatch { get; set; } = null!;

    /// <summary>
    /// 所屬公司ID
    /// </summary>
    [StringLength(100)]
    public string CompId { get; set; } = null!;

    [StringLength(100)]
    public string InStockId { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }
}