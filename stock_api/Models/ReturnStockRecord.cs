﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

[Table("return_stock_record")]
public partial class ReturnStockRecord
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(100)]
    public string InStockId { get; set; } = null!;

    [StringLength(100)]
    public string OutStockId { get; set; } = null!;

    public float InStockQuantityBefore { get; set; }

    public float InStockQuantityAfter { get; set; }

    public float OutStockApplyQuantityBefore { get; set; }

    public float OutStockApplyQuantityAfter { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? LotNumberBatch { get; set; }

    [StringLength(100)]
    public string? LotNumber { get; set; }

    [StringLength(100)]
    public string? CompId { get; set; }

    [StringLength(100)]
    public string? ProductId { get; set; }

    [StringLength(200)]
    public string? ProductCode { get; set; }

    [StringLength(200)]
    public string? ProductName { get; set; }

    [StringLength(100)]
    public string? UserId { get; set; }

    [StringLength(100)]
    public string? UserName { get; set; }
}