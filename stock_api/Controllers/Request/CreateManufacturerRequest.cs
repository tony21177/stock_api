﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateManufacturerRequest
    {
        
        
        public string Code { get; set; }

        
        public string Name { get; set; }

        public string? Remark { get; set; }

        public bool IsActive { get; set; }

    }
}
