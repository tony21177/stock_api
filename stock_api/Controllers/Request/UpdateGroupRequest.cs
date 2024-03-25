using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class UpdateGroupRequest
    {
        public string GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupDescription { get; set; }
        public bool? IsActive { get; set; }

    }
}
