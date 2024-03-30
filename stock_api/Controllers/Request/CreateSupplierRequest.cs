using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateSupplierRequest
    {
        
        
        public string Code { get; set; }
        public string Name { get; set; }
        public string CompId { get; set; }

        public string? CompanyPhone { get; set; }
        public string? ContactUser { get; set; }
        public string? ContactUserPhone { get; set; }


        public string? Remark { get; set; }

        public bool IsActive { get; set; }

    }
}
