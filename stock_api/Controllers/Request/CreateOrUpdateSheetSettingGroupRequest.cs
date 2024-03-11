using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateOrUpdateSheetSettingGroupRequest
    {

        public int? MainSheetId { get; set; }
        public int? SheetGroupId { get; set; }

        public string? GroupTitle { get; set; }

        public string? GroupDescription { get; set; }

        public int? GroupRank { get; set; }

        public bool? IsActive { get; set; }

    }
}
