using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateCompanyRequest
    {

        public string Name { get; set; }


        public bool IsActive { get; set; }



    }
}
