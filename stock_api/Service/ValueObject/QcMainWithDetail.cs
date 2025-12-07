using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class QcMainWithDetail 
    {

        public string MainId { get; set; } = null!;
        [JsonPropertyName("purchaseMainID")]
        public string? PurchaseMainId { get; set; }
        public DateTime ApplyDate { get; set; }
        public string? PurchaseSubItemId { get; set; }
        public string InStockId { get; set; } = null!;
        [JsonPropertyName("acceptedAt")]
        public DateTime InStockTime { get; set; }
        [JsonPropertyName("acceptUserId")]
        public string InStockUserId { get; set; } = null!;
        [JsonPropertyName("acceptUserName")]
        public string InStockUserName { get; set; } = null!;
        [JsonPropertyName("productID")]
        public string ProductId { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? ProductSpec { get; set; }
        public string CompId { get; set; } = null!;
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? ValidationType { get; set; }
        public string? ValidationMethod { get; set; }
        public string? ValidationItemName { get; set; }
        public string? Comment { get; set; }
        public string QcType { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CurrentStatus { get; set; } 
        public List<QcValidationDetail> DetailList { get; set; } = new();
        public List<QcAcceptanceDetail> AcceptanceDetails { get; set; } = new();
        public bool IsLotNumberOutStock {  get; set; }
        public bool IsLotNumberBatchOutStock { get; set; }
        // 20240910新增
        public string? ProductModel { get; set; }
        public string FinalResult { get; set; } = null!;
        public string NewLotNumberTestResult { get; set; } = null!;
        public string? NewLotNumberTestDocumentId { get; set; }
        public string? NewLotNumberTestCode { get; set; }
        public string? PreTestBarCode { get; set; }
        public string? PreTestResult { get; set; }
        public string? PreTestId { get; set; }
        public string? TeamLeader { get; set; }
        public string? SummaryRemarks { get; set; }
        public string? Tester { get; set; }
        public string? TestingDate { get; set; }
        public string? ConcentrationComment { get; set; }

        public bool IsNewLotNumber { get; set; } = true;
        public bool IsNewLotNumberBatch { get; set; } = true;

        // 新增回傳欄位
        public DateOnly? ExpirationDate { get; set; }
        public string? PrevLotNumber { get; set; }
        public DateTime? VerifyAt { get; set; }
        public List<string> GroupIdList { get; set; } = new();
        public List<string> GroupNameList { get; set; } = new();

    }


}
