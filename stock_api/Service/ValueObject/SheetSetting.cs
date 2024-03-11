using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class SheetSetting
    {
        public int SheetId { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public DateTime UpdatedTime { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool? IsActive { get; set; }

        public string Version { get; set; }

        public string SerialCode { get; set; }

        public string CreatorName { get; set; }

        public DateTime? CreatedTime { get; set; }

        public List<HandoverSheetGroupDto>? HandoverSheetGroupList { get; set; } = new List<HandoverSheetGroupDto>();
    }

    public class HandoverSheetGroupDto
    {
        public string Id { get; set; }

        public int MainSheetId { get; set; }

        public int SheetGroupId { get; set; }

        public string GroupTitle { get; set; }

        public string GroupDescription { get; set; }

        public int GroupRank { get; set; }

        public bool? IsActive { get; set; }

        public string CreatorName { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public List<HandoverSheetRow> HandoverSheetRowList { get; set; } = new List<HandoverSheetRow>();
    }
}
