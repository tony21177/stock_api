namespace stock_api.Service.ValueObject
{
    public class HandoverSheetRowDetailAndSettings
    {
        public int SheetId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public bool? IsActive { get; set; }

        public string Version { get; set; }

        public string SerialCode { get; set; }

        public string CreatorName { get; set; }

        public DateTime? CreatedTime { get; set; }

        public List<GroupSetting>? HandoverSheetGroupList { get; set; } = new List<GroupSetting>();

        public List<Reader> readers { get; set; }
    }

    public class Reader
    {
        public string UserId { get; set; }
        public string Name { get; set; }

        public bool IsRead { get; set; }
    }




    public class GroupSetting
    {

        public int MainSheetId { get; set; }

        public int SheetGroupId { get; set; }

        public string GroupTitle { get; set; }

        public string GroupDescription { get; set; }

        public int GroupRank { get; set; }

        public bool? IsActive { get; set; }

        public string CreatorName { get; set; }

        public DateTime? CreatedTime { get; set; }

        public List<RowSettingAndDetail> RowSettingAndDetailList { get; set; } = new List<RowSettingAndDetail>();
    }

    public class RowSettingAndDetail
    {
        public int? MainSheetId { get; set; }
        public int? SheetGroupId { get; set; }
        public int? SheetRowId { get; set; }
        public string WeekDays { get; set; }
        public string SheetGroupTitle { get; set; }
        public string RowCategory { get; set; }
        public string MachineBrand { get; set; }
        public string MachineCode { get; set; }
        public string MachineSpec { get; set; }
        public string MaintainItemName { get; set; }
        public string MaintainItemDescription { get; set; }
        public string MaintainItemType { get; set; }
        public string MaintainAnswerType { get; set; }
        public string Remarks { get; set; }
        public string CreatorName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedTime { get; set; }
        //底下為detail
        public string? Status { get; set; }
        public string? Comment { get; set; }
    }

}
