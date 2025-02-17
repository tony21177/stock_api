﻿using stock_api.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using stock_api.Service.ValueObject;

namespace stock_api.Controllers.Dto
{
    public class PurchaseHistoryDto
    {
        public int Id { get; set; }

        public string Action { get; set; } = null!;

        public string ItemId { get; set; } = null!;
        public string PurchaseMainId { get; set; } = null!;
        public string PurchaseOrderNo { get; set; } = null!;
        public PurchaseSubItemWithUnit? ItemBeforeValues { get; set; }

        public PurchaseSubItemWithUnit? ItemAfterValues { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
    }
}
