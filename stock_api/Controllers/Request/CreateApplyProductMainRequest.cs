using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateApplyProductMainRequest
    {


        public string? ApplyReason { get; set; }
        public string? ApplyRemarks { get; set; }
        public string? CompId { get; set; } = null!;
        public string ApplyProductName { get; set; } = null!;
        public string? ApplyProductSpec { get; set; }
        public float? ApplyQuantity { get; set; } = 0;
        public string? ProductGroupId { get; set; }
    }
   
}
