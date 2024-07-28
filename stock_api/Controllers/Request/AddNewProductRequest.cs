using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class AddNewProductRequest
    {

        /// <summary>
        /// 品項所屬的組織ID
        /// </summary>
        public List<string> CompIds { get; set; } = null!;

        /// <summary>
        /// 品項所屬的製造商ID
        /// </summary>
        public string? ManufacturerId { get; set; }


        /// <summary>
        /// 有效期限規範
        /// </summary>
        public int? DeadlineRule { get; set; }

        /// <summary>
        /// 運送備註
        /// </summary>
        public string? DeliverRemarks { get; set; }

        /// <summary>
        /// 庫存數量
        /// </summary>
        public float? InStockQuantity { get; set; }

        /// <summary>
        /// 管理者
        /// </summary>
        public string? Manager { get; set; }

        /// <summary>
        /// 最高安庫量
        /// </summary>
        public float? MaxSafeQuantity { get; set; }



        /// <summary>
        /// 開封有效期限
        /// 數字（開封後可以用幾天），檢查資料庫是不是int
        /// </summary>
        public int? OpenDeadline { get; set; }

        /// <summary>
        /// 開封列印名稱
        /// </summary>
        public string? OpenedSealName { get; set; }

        /// <summary>
        /// 原始有效期限
        /// </summary>
        public String? OriginalDeadline { get; set; }

        /// <summary>
        /// 包裝方式
        /// </summary>
        public string? PackageWay { get; set; }

        /// <summary>
        /// 已入庫未使用，當前日期在末效日期前幾天通知用
        /// </summary>
        public int? PreDeadline { get; set; }

        /// <summary>
        /// 前置天數
        /// </summary>
        public int? PreOrderDays { get; set; }

        /// <summary>
        /// 產品類別
        /// [耗材, 試劑, 其他]
        /// </summary>
        public string? ProductCategory { get; set; }


        /// <summary>
        /// 品項型號
        /// </summary>
        public string? ProductModel { get; set; }

        /// <summary>
        /// 品項名稱
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 品項備註
        /// </summary>
        public string? ProductRemarks { get; set; }

        /// <summary>
        /// 品項規格
        /// </summary>
        public string? ProductSpec { get; set; }

        /// <summary>
        /// 最小安庫量
        /// </summary>
        public float? SafeQuantity { get; set; }

        /// <summary>
        /// UDI 碼
        /// </summary>
        public string? UdibatchCode { get; set; }

        /// <summary>
        /// UDI 碼
        /// </summary>
        public string? UdicreateCode { get; set; }

        /// <summary>
        /// UDI 碼
        /// </summary>
        public string? UdiserialCode { get; set; }

        /// <summary>
        /// UDI 碼
        /// </summary>
        public string? UdiverifyDateCode { get; set; }

        /// <summary>
        /// 單位
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// 重量
        /// </summary>
        public string? Weight { get; set; }

        /// <summary>
        /// 品項所屬儀器
        /// </summary>
        public string? ProductMachine { get; set; } = string.Empty;

        /// <summary>
        /// 預設供應商
        /// </summary>
        public int? DefaultSupplierId { get; set; }


        /// <summary>
        /// 該品項出庫時，是否需要經過二次驗收
        /// </summary>
        public bool? IsNeedAcceptProcess { get; set; }

        /// <summary>
        /// NONE,LOT_NUMBER,LOT_NUMBER_BATCH
        /// </summary>
        public string? QcType { get; set; }

        /// <summary>
        /// 該品項期限距離現在的最小天數
        /// </summary>
        public int? AllowReceiveDateRange { get; set; }

        /// <summary>
        /// 用來在訂購時將最小單位轉為訂購規格及驗收時 將訂購規格轉為最小單位數量用的欄位
        /// </summary>
        public float? UnitConversion { get; set; }

        /// <summary>
        /// 在總覽表與目前庫存數量(InStockQuantity)相乘顯示給使用者知道目前可用的數量用的欄位
        /// </summary>
        public float? TestCount { get; set; }

        public bool? IsActive { get; set; }

        public string? StockLocation { get; set; }

        /// <summary>
        /// VENDOR:廠商直寄,
        /// OWNER:得標廠商(金萬林)供貨
        /// </summary>
        public string? Delievery { get; set; }

        public float? SupplierUnitConvertsion { get; set; }

        public string? SupplierUnit { get; set; }

        public string? DeliverFunction { get; set; }

        public double? DeliverTemperature { get; set; }

        public string? SavingFunction { get; set; }

        public double? SavingTemperature { get; set; }
        public bool? IsPrintSticker { get; set; }

    }
}
