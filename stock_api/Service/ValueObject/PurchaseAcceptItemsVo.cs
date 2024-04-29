using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class PurchaseAcceptItemsVo
    {
        public string PurchaseMainId { get; set; }
        public DateTime ApplyDate { get; set; }
        public string CompId { get; set; }
        public string CurrentStatus { get; set; }
        public DateOnly? DemandDate { get; set; }
        public string GroupIds { get; set; }
        public string Remarks { get; set; }
        public string UserId { get; set; }
        public string ReceiveStatus { get; set; }
        public string Type { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<AcceptItem> AcceptItems { get; set; }

        public bool IsContainKeywords(string keywords)
        {
            return $"{this.CurrentStatus} {this.GroupIds} {this.Remarks} {this.UserId} {this.ReceiveStatus} {this.Type}".Contains(keywords);
            
        }
    }
    public class AcceptItem
    {
        public string AcceptId { get; set; }
        public int? AcceptQuantity { get; set; }
        public string AcceptUserId { get; set; }
        public string LotNumberBatch { get; set; }
        public string LotNumber { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public string ItemId { get; set; }
        public int OrderQuantity { get; set; }
        public string PackagingStatus { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSpec { get; set; }
        public string UdiserialCode { get; set; }
        public string QcStatus { get; set; }
        public int? CurrentTotalQuantity { get; set; }
        public string Comment { get; set; }
        public string QcComment { get; set; }
        public DateTime? AcceptCreatedAt { get; set; }
        public DateTime? AcceptUpdatedAt { get; set; }

        public bool IsContainKeywords(string keywords)
        {
            return ($"{this.AcceptId} {this.AcceptUserId} {this.LotNumberBatch} {this.LotNumber} {this.PackagingStatus} {this.ProductId} {this.ProductName} {this.ProductSpec} {this.UdiserialCode}" +
                $"{this.QcStatus} {this.Comment}").Contains(keywords);

        }
    }
}
