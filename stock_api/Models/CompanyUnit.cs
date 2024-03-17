﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Models;

/// <summary>
/// 醫院院區關係資料
/// </summary>
[Table("company_unit")]
[Index("UnitId", "CompId", Name = "ComId_UnitId_UNIQUE", IsUnique = true)]
[Index("CompId", Name = "CompId_UNIQUE", IsUnique = true)]
public partial class CompanyUnit
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 如果是同一單位，就會是一樣的 UnitID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UnitId { get; set; }

    /// <summary>
    /// 單位名稱
    /// </summary>
    [StringLength(100)]
    public string UnitName { get; set; }

    /// <summary>
    /// 組織ID
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CompId { get; set; }

    /// <summary>
    /// 組織名稱
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CompName { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }
}