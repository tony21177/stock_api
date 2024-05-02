﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 上傳檔案
/// </summary>
[Table("file_detail_info")]
[Index("AttId", Name = "AttID_UNIQUE", IsUnique = true)]
public partial class FileDetailInfo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("AttID")]
    [StringLength(100)]
    public string AttId { get; set; } = null!;

    [StringLength(200)]
    public string? FileName { get; set; }

    [StringLength(200)]
    public string? FilePath { get; set; }

    [StringLength(45)]
    public string? FileType { get; set; }

    [StringLength(45)]
    public string? FileSizeText { get; set; }

    public double? FileSizeNumber { get; set; }

    /// <summary>
    /// member.UserId
    /// </summary>
    [StringLength(45)]
    public string? CreatorId { get; set; }

    /// <summary>
    /// member.UserId
    /// </summary>
    [StringLength(45)]
    public string? CompId { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }
}