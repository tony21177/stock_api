# 從「庫存總覽表」Excel 批次新增 warehouse_product

把客戶／單位整理的「庫存總覽表」Excel（檔名通常是 `<單位>｜<單位> - 新增品項 - YYYYMMDD.xlsx`）轉成
`warehouse_product` 的 INSERT SQL，並批次匯入指定組織（CompId）與組別。

對應腳本：[gen_product_insert.py](gen_product_insert.py)

---

## 1. 前置作業

| 項目 | 說明 |
|---|---|
| Python 套件 | `python -m pip install pandas openpyxl` |
| MySQL client | 預設用 `C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe`（腳本會連 DB 查供應商／組別對應）|
| DB 連線 | 預設 `127.0.0.1 / root / root / stock`，可在腳本「設定區」改 |
| **組別需先存在** | Excel 的「組別」名稱必須已存在於 `warehouse_group`（且 CompId 相符），否則腳本中止 |
| **供應商** | Excel 的「供應商」名稱查 `supplier.Name`。查無時：`AUTO_CREATE_MISSING_SUPPLIER=True`（預設）會自動產生其 `INSERT`；設 `False` 則該筆 `DefaultSupplierID` 留 `NULL` 並印警告 |

---

## 2. 操作步驟

```bash
# 1. 編輯 gen_product_insert.py 最上方「設定區」的 JOBS：
#    每個 Excel 一筆：file（檔路徑）、compId、compName（顯示用）、groupName（要掛的組別）

# 2. 在專案根目錄執行（Excel 檔放這裡）
cd d:/lis/Health/stock_api
python sql/tools/gen_product_insert.py
#   → 產出 sql/tools/_generated_product_insert.sql

# 3. 正式匯入前先 dry-run 驗證（包在交易內並 rollback，不會真的寫入）
{ echo "START TRANSACTION;"; cat sql/tools/_generated_product_insert.sql; echo "ROLLBACK;"; } \
  | "/c/Program Files/MySQL/MySQL Server 8.0/bin/mysql.exe" \
      -h127.0.0.1 -uroot -proot stock --default-character-set=utf8mb4
#   無錯誤即代表 SQL 合法、無 unique key／型別衝突

# 4. 確認無誤後正式匯入（拿掉 rollback）
"/c/Program Files/MySQL/MySQL Server 8.0/bin/mysql.exe" \
  -h127.0.0.1 -uroot -proot stock --default-character-set=utf8mb4 \
  < sql/tools/_generated_product_insert.sql
```

### 缺少的供應商

`AUTO_CREATE_MISSING_SUPPLIER`（設定區，預設 `True`）控制行為：

- **True**：腳本先掃描所有 Excel，找出 `supplier` 表查無的供應商名稱，於輸出檔最前面自動產生
  `INSERT INTO supplier ...`，Id 接在現有最大 Id 之後遞增、`CompId` 用 `NEW_SUPPLIER_COMPID`
  （預設＝金萬林 `63c0dbc4-…`，供應商主檔慣例掛在得標廠商 OWNER）、`Code` 留空。品項列會直接引用新配發的 Id。
- **False**：查無的供應商，該筆 `DefaultSupplierID` 留 `NULL` 並於執行後印出警告清單，需自行補。

> 自動產生的供應商 `Code` 為空字串，若該供應商之後有實際代碼，再到 `supplier` 表補上即可。

---

## 3. Excel 欄位 → warehouse_product 對應

Excel 第一個工作表為「庫存總覽表」，資料從標題列（第一格＝`編碼`）的下一列開始。

| Excel 欄位 | warehouse_product 欄位 | 轉換規則 |
|---|---|---|
| 編碼 | `ProductCode` | 直接沿用（同時組成 ProductId）|
| 品號 | `ProductModel` | 例：CMP0202750 |
| 品名 | `ProductName` | |
| 庫存量 | `InStockQuantity` | |
| 單位 | `Unit` | |
| 包裝方式 | `PackageWay` | |
| 供應商 | `DefaultSupplierID` / `DefaultSupplierName` | 用名稱查 `supplier.Id`；查無→NULL |
| 儲存位置 | `StockLocation` | |
| 組別 | `GroupIds` / `GroupNames` | 用名稱+CompId 查 `warehouse_group.GroupId` |
| 產品規格 | `ProductSpec` | |
| 目前使用中批次／批號 | `LotNumberBatch` / `LotNumber` | 新品通常空白 |
| 品項關聯儀器 | `ProductMachine` | |
| 品項備註 | `ProductRemarks` | |
| 品項類別 | `ProductCategory` | 試劑／耗材 |
| 製造商 | `ManufacturerName` | 多數為空（`ManufacturerId` 留空字串）|
| 開封列印名稱 | `OpenedSealName` | |
| 最小／最大安庫量 | `SafeQuantity` / `MaxSafeQuantity` | 去除單位後綴（`170支`→`170`）|
| 管理人員 | `Manager` | 例：CH-B／KF-B |
| 是否需要驗收 | `IsNeedAcceptProcess` | 是→1、否→0 |
| 品質確效類型 | `QcType` | 批次→`LOT_NUMBER_BATCH`、批號→`LOT_NUMBER`、無→`NONE` |
| 出庫是否列印貼紙 | `IsPrintSticker` | 是→1 |
| 品項是否啟用 | `IsActive` | 是→1 |
| 有效期限規範 | `DeadlineRule` | int；空白→NULL |
| 開封有效期限 | `OpenDeadline` | int；空白→NULL |
| 訂購前置天數 | `PreOrderDays` | |
| 允收期限日範圍 | `AllowReceiveDateRange` | |
| 預告預期之期限 | `PreDeadline` | |
| 重量 | `Weight` | |
| 運送條件 | `DeliverFunction` | 存中文（冷藏／室溫）|
| 運送備註 | `DeliverRemarks` | |
| 儲存環境條件 | `SavingFunction` | 存中文（冷藏保存／常溫保存）|
| 寄送類型 | `Delievery` | 廠商直送→`VENDOR`、得標廠商供貨→`OWNER` |
| UDI 品項條碼 | `UDISerialCode` | |
| 包裝單位轉換 | `UnitConversion` | |

**不入庫的欄位**（Excel 為檢視／計算用，DB 無對應）：`缺額數量`、`訂購中數量`、`平均月用量`。

**走 DB 預設值的欄位**：`TestCount`、`SupplierUnitConvertsion`、`SupplierUnit`、`CreatedAt`、`UpdatedAt`、
`IsAllowDiscard`、`DeliverTemperature`、`SavingTemperature`、`ValidationMethod`、`UDIBatchCode/CreateCode/VerifyDateCode`。

---

## 4. 重要規則與注意事項

- **主鍵與唯一鍵**：`warehouse_product` PK = `(CompId, ProductId)`，另有唯一鍵 `(CompId, ProductCode)`。
  `ProductModel`（品號）**不限重複** → 同一個品號可在同一 CompId 下以不同 ProductCode 登錄多筆
  （例如同品項掛到不同請購組別）。這是系統既有慣例，不是錯誤資料。
- **ProductId 格式**：`{CompId}-{編碼}`（與本專案既有資料慣例一致；近期經程式新增的雖是隨機 GUID，
  但批次匯入採可讀的 `CompId-編碼` 較好追）。
- **ProductCode 來源**：直接沿用 Excel 的「編碼」。匯入前確認該編碼在目標 CompId 尚未使用
  （查 `SELECT MAX(CAST(ProductCode AS UNSIGNED)) FROM warehouse_product WHERE CompId=...`）。
- **務必先 dry-run**：步驟 3 的交易包 rollback 可在不寫入的情況下驗證整批 SQL 合法性。

---

## 5. 範例：20260615 彰化醫事檢驗科 + 金萬林

| 項目 | 彰化醫院院區｜醫事檢驗科 | 金萬林｜金萬林 |
|---|---|---|
| CompId | `68c05e1d-c6f5-498a-8359-d961359e5875` | `63c0dbc4-fdca-47e8-b084-5880fb596de2` |
| 掛入組別 | 二樓代檢請購組(不轉訂單) `5b89ec61-…` | 結核菌代檢 `cac55639-…` |
| Manager | CH-B | KF-B |
| 筆數 | 27（代碼 850–876）| 27（代碼 850–876）|

- 該批含一個原本不存在的供應商「保吉」（供彰化 871/875/876 綁定）。當時 supplier 最大 Id=95，
  故配發 Id=96（CompId=金萬林）；現行腳本 `AUTO_CREATE_MISSING_SUPPLIER=True` 會自動產生這段。
- 產出檔：[../20260615_add_products_changhua_kimforest.sql](../20260615_add_products_changhua_kimforest.sql)
