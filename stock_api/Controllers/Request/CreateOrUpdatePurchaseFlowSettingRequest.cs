﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stock_api.Controllers.Request
{
    public class CreateOrUpdateApplyProductFlowSettingRequest
    {
        public string? SettingId { get; set; }
        public string? CompId { get; set; }
        public string? FlowName { get; set; }
        public int? Sequence { get; set; }
        public string? ReviewUserId { get; set; }
        public string? ReviewGroupId { get; set; }
        public bool? IsActive { get; set; }
        
    }
}
