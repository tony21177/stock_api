using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class PurchaseMainAndSubItemVo
    {
        public string PurchaseMainId { get; set; }
        public DateTime ApplyDate { get; set; }
        public string CompId { get; set; }
        public string CurrentStatus { get; set; }
        public DateOnly? DemandDate { get; set; }
        public List<string> GroupIds { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public string ReceiveStatus { get; set; }
        public string Type { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string?  SplitProcess { get; set; }
        public string? OwnerProcess { get; set; }
        public string? OwnerComment { get; set; }
        public List<PurchaseSubItemVo> Items {get;set;}
        public List<PurchaseFlowWithAgentsVo>? flows { get;set;}
        public List<PurchaseFlowLog>? flowLogs { get;set;}
    }
}
