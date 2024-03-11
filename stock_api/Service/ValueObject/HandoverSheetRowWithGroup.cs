using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class HandoverSheetRowWithGroup
    {
        [Column("id")]
        public string Id { get; set; }

        
        [Column("MainSheetID")]
        public int? MainSheetId { get; set; }

        
        [Column("SheetGroupID")]
        public int? SheetGroupId { get; set; }

        [Column("SheetRowID")]
        public int SheetRowId { get; set; }

        
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

        public DateTime? UpdatedTime { get; set; }

        public bool? IsGroupActive { get; set; }
    }
}
