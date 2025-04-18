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
        public string? CompName { get; set; }
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

        public bool IsContainKeywords(string keywords)
        {
            string formattedDate = ApplyDate.ToString("yyyyMMdd");
            string purchaseIdPrefix = PurchaseMainId.Substring(0, 5);
            string supplierNames = string.Join(",", Items.Select(i => i.ArrangeSupplierName));
            string productNames = string.Join(",", Items.Select(i => i.ProductName));
            string productCodes = string.Join(",", Items.Select(i => i.ProductCode));
            string productModels = string.Join(",", Items.Select(i => i.ProductModel));
            string productSpecs = string.Join(",", Items.Select(i => i.ProductSpec));
            string productMachines = string.Join(",", Items.Select(i => i.ProductMachine));
            string searchedString = $"{formattedDate}{purchaseIdPrefix} {this.CurrentStatus} {this.Remarks} {this.UserId} {this.ReceiveStatus} {this.Type} {supplierNames} {supplierNames} {productNames} {productCodes} {productModels} {productSpecs} {productMachines}";
            bool isContainsString = searchedString.Contains(keywords);
            return isContainsString;
        }
    }
}
