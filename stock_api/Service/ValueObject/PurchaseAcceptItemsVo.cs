using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using stock_api.Models;

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
            string formattedDate = ApplyDate.ToString("yyyyMMdd");
            string purchaseIdPrefix = PurchaseMainId.Substring(0, 5);

            return $"{formattedDate}{purchaseIdPrefix} {this.CurrentStatus} {this.GroupIds} {this.Remarks} {this.UserId} {this.ReceiveStatus} {this.Type}".Contains(keywords);
        }
    }

    public class ManualAcceptItem
    {
        public string PurchaseMainId { get; set; }
        public string AcceptId { get; set; }
        public float? AcceptQuantity { get; set; }
        public string? AcceptUserId { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? LotNumber { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public string ItemId { get; set; }
        public float OrderQuantity { get; set; }
        public string? PackagingStatus { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSpec { get; set; }
        public string? UdiserialCode { get; set; }
        public string? QcStatus { get; set; }
        public float? CurrentTotalQuantity { get; set; }
        public string? Comment { get; set; }
        public string? QcComment { get; set; }
        public DateTime? AcceptCreatedAt { get; set; }
        public DateTime? AcceptUpdatedAt { get; set; }
        public string? DeliverFunction { get; set; }
        public double? DeliverTemperature { get; set; }
        public string? SavingFunction { get; set; }
        public double? SavingTemperature { get; set; }
        public string? Unit { get; set; }
        public string? UDIBatchCode { get; set; }
        public string? UDICreateCode { get; set; }
        public string? UDIVerifyDateCode { get; set; }
        public DateOnly? DemandDate { get; set; }
        public string? Prod_savingFunction { get; set; }
        public string? Prod_stockLocation { get; set; }

        public string InStockStatus { get; set; }
        public bool IsContainKeywords(string keywords)
        {
            return ($"{this.AcceptId} {this.AcceptUserId} {this.LotNumberBatch} {this.LotNumber} {this.PackagingStatus} {this.ProductId} {this.ProductName} {this.ProductSpec} {this.UdiserialCode}" +
                $"{this.QcStatus} {this.Comment}").Contains(keywords);

        }
    }

    public class AcceptItem
    {
        public string? PurchaseMainId { get; set; }
        public DateTime? ApplyDate { get; set; }
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
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductSpec { get; set; }
        public string UdiserialCode { get; set; }
        public string QcStatus { get; set; }
        public int? CurrentTotalQuantity { get; set; }
        public string Comment { get; set; }
        public string QcComment { get; set; }
        public DateTime? AcceptCreatedAt { get; set; }
        public DateTime? AcceptUpdatedAt { get; set; }
        public DateTime? VerifyAt { get; set; }
        public string? DeliverFunction { get; set; }
        public double? DeliverTemperature { get; set; }
        public string? SavingFunction { get; set; }
        public double? SavingTemperature { get; set; }
        public string? Unit { get; set; }
        public string? UDIBatchCode { get; set; }
        public string? UDICreateCode { get; set; }
        public string? UDIVerifyDateCode { get; set; }
        public string? Prod_supplierName { get; set; }
        public int? ArrangeSupplierId { get; set; }
        public string? ArrangeSupplierName { get; set; }
        public string InStockStatus { get; set; }

        public PurchaseSubItem PurchaseSubItem { get; set; }
        public bool IsContainKeywords(string keywords)
        {
            // return ($"{this.ArrangeSupplierName} {this.AcceptId} {this.AcceptUserId} {this.LotNumberBatch} {this.LotNumber} {this.PackagingStatus} {this.ProductId} {this.ProductName} {this.ProductSpec} {this.UdiserialCode}" +
            //     $"{this.QcStatus} {this.Comment}").Contains(keywords);
            return ($"{this.ArrangeSupplierName}").Contains(keywords);

        }
    }
}
