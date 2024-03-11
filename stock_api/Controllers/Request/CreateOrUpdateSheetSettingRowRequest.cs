using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateOrUpdateSheetSettingRowRequest
    {

        public int? MainSheetId { get; set; }
        public int? SheetGroupId { get; set; }
        public int? SheetRowId { get; set; }
        public string? WeekDays { get; set; }
        public string? RowCategory { get; set; }
        public string? MachineBrand { get; set; }
        public string? MachineCode { get; set; }
        public string? MachineSpec { get; set; }
        public string? MaintainItemName { get; set; }
        public string? MaintainItemDescription { get; set; }
        public string? MaintainItemType { get; set; }
        public string? MaintainAnswerType { get; set; }
        public string? Remarks { get; set; }
        public bool? IsActive { get; set; }
    }
}
