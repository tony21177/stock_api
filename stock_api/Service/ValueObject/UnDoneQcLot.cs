using Org.BouncyCastle.Utilities;

namespace stock_api.Service.ValueObject
{
    public class UnDoneQcLot
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string QcType { get; set; } = null!;
        public string? QcTestStatus { get; set; }
        public string? PurchaseMainId { get; set; } = null!;
        public DateTime? ApplyDate { get; set; } 
        public String InStockId { get; set; } = null!;
        public DateTime? AcceptedAt { get; set; }
        public string AcceptUserName { get; set; }
        public string AcceptUserId { get; set; }

        public string ProductSpec { get; set; } = null!;
        public bool IsLotNumberOutStock { get; set; } 
        public bool IsLotNumberBatchOutStock { get; set; } 
        // 20240910新增
        public string? ProductModel { get; set; } 
        public DateTime? InStockTime { get; set; }
        public string? InStockUserId {  get; set; }
        public string? InStockUserName { get; set; }
        public bool IsNewLotNumber { get; set; } = true;
        public bool IsNewLotNumberBatch { get; set; } = true;
        public List<string>? GroupIdList { get; set; }
        public List<string>? GroupNameList { get; set; }

        // 上次QC相關資料
        public string? LastFinalResult { get; set; }
        public string? LastInStockLotNumberBatch { get; set; }
        public string? LastMainId { get; set; }

        // 新增回傳欄位
        public DateOnly? ExpirationDate { get; set; }
        public string? PrevLotNumber { get; set; }
        public DateTime? VerifyAt { get; set; }

        public bool IsContainKeywords(string keywords)
        {
            string formattedDate = "";
            if (ApplyDate.HasValue)
            {
                formattedDate = ApplyDate.Value.ToString("yyyyMMdd");
            }
            string purchaseIdPrefix = "";
            if (PurchaseMainId!=null)
            {
                purchaseIdPrefix = PurchaseMainId.Substring(0, 5);
            }


            //string searchedString = $"{formattedDate}{purchaseIdPrefix} {this.ProductName} {this.ProductCode} {this.LotNumber} {this.LotNumberBatch} {this.ProductModel} {string.Join(" ",this.GroupNameList)}";

            // 20251211彰化醫院婉君只想搜尋批次
            string searchedString = $"{this.LotNumberBatch}";
            bool isContainsString = searchedString.Contains(keywords);
            return isContainsString;
        }
    }
}
