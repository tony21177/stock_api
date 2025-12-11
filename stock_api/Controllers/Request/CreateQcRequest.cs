using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateQcRequest
    {
        public string? MainId { get; set; }
        public string? CompId {  get; set; }
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? ValidationType { get; set; }

        public string? ValidationMethod { get; set; }

        public string? ValidationItemName { get; set; }

        public string? Comment { get; set; }
        public string? QcType {  get; set; }
        // 20240910新增
        public string FinalResult { get; set; } = null!;
        public string NewLotNumberTestResult { get; set; } = null!;
        public string? NewLotNumberTestDocumentId { get; set; }
        public string? NewLotNumberTestCode { get; set; }
        public string? PreTestBarCode { get; set; }
        public string? PreTestResult { get; set; }
        public string? PreTestId { get; set; }
        public string? Remark { get; set; }
        public string? TeamLeader { get; set; }
        public string? SummaryRemarks { get; set; }
        public string? Tester { get; set; }
        public string? TestingDate { get; set; }
        public string? ConcentrationComment { get; set; }


        public List<QcDetail> Details { get; set; } = null!;
        public List<AcceptanceDetail> AcceptanceDetails { get; set; } = null!;

    }
    public class QcDetail
    {
        public int ItemNumber { get; set; }
      
        public string? NewLotResult { get; set; }

        public string? OldLotResult { get; set; }

        public string? QuantityDiff
        {
            get
            {
                if (float.TryParse(NewLotResult, out float newLot) && float.TryParse(OldLotResult, out float oldLot))
                {
                    return (newLot - oldLot).ToString();
                }
                return null;
            }
            set { } // 保留 setter 以支援 JSON 反序列化，但不實際儲存值
        }
        public string? AcceptableRange { get; set; }
        public string? ValidationResult { get; set; }
    }

    public class AcceptanceDetail
    {
        public int ItemNumber { get; set; }

        public string? NewLotResult { get; set; }

        public string? AcceptRange { get; set; }
        public string? ValidationResult { get; set; }
    }
}
