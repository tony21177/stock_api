# Denormalize Product Fields to Stock Records Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Snapshot warehouse_product master data into in_stock_item_record and out_stock_record at insert time, so historical records are preserved when product data changes.

**Architecture:** 入庫時從 WarehouseProduct 快照品項欄位到 InStockItemRecord；出庫時從對應的 InStockItemRecord 取得快照欄位寫入 OutStockRecord（無對應入庫紀錄時才用 WarehouseProduct）。查詢時改用紀錄自身欄位，移除 join warehouse_product。

**Tech Stack:** C# / ASP.NET Core 6 / EF Core 7 / MySQL 8

---

## File Structure

| File | Action | Responsibility |
|------|--------|---------------|
| `sql/20260317_denormalize_product_fields.sql` | Create | SQL migration: ALTER TABLE + backfill |
| `stock_api/Models/InStockItemRecord.cs` | Modify | Add 10 new properties |
| `stock_api/Models/OutStockRecord.cs` | Modify | Add 9 new properties |
| `stock_api/Service/ValueObject/InStockItemRecordVo.cs` | Modify | Remove 3 properties now in base |
| `stock_api/Service/ValueObject/OutStockRecordVo.cs` | Modify | Remove 6 properties now in base |
| `stock_api/Service/StockInService.cs` | Modify | 2 InStockItemRecord INSERT + 1 OutStockRecord INSERT (直接出庫) |
| `stock_api/Service/StockOutService.cs` | Modify | 3 OutStockRecord INSERT + refactor SearchByOpenDeadline |
| `stock_api/Service/AdjustService.cs` | Modify | 2 InStockItemRecord INSERT + 2 OutStockRecord INSERT |
| `stock_api/Controllers/StockInController.cs` | Modify | ListStockInRecords: remove join |
| `stock_api/Controllers/StockOutController.cs` | Modify | ListStockOutRecords + ListStockOutRecordsForOwner: remove join |
| `stock_api/Service/ReportService.cs` | Modify | GetProductInAndOutRecords: remove join |

---

## 新增欄位一覽

### InStockItemRecord 新增 10 個欄位（與 OutStockRecord 完全一致）

| 欄位 | 型別 | DB Column | 來源 |
|------|------|-----------|------|
| ProductModel | string? | varchar(200) | WarehouseProduct.ProductModel |
| Unit | string? | varchar(100) | WarehouseProduct.Unit |
| GroupIds | string? | varchar(2000) | WarehouseProduct.GroupIds |
| GroupNames | string? | varchar(2000) | WarehouseProduct.GroupNames |
| PreDeadline | int? | int | WarehouseProduct.PreDeadline |
| OpenDeadline | int? | int | WarehouseProduct.OpenDeadline |
| IsAllowDiscard | bool? | tinyint(1) | WarehouseProduct.IsAllowDiscard |
| DefaultSupplierId | int? | int (DefaultSupplierID) | WarehouseProduct.DefaultSupplierId |
| DefaultSupplierName | string? | varchar(200) | WarehouseProduct.DefaultSupplierName |
| PackageWay | string? | varchar(100) | WarehouseProduct.PackageWay |

### OutStockRecord 新增 9 個欄位

| 欄位 | 型別 | DB Column | 來源 |
|------|------|-----------|------|
| Unit | string? | varchar(100) | InStockItemRecord.Unit |
| OpenDeadline | int? | int | InStockItemRecord.OpenDeadline |
| ProductModel | string? | varchar(200) | InStockItemRecord.ProductModel |
| GroupIds | string? | varchar(2000) | InStockItemRecord.GroupIds |
| GroupNames | string? | varchar(2000) | InStockItemRecord.GroupNames |
| IsAllowDiscard | bool? | tinyint(1) | InStockItemRecord.IsAllowDiscard |
| DefaultSupplierId | int? | int (DefaultSupplierID) | InStockItemRecord.DefaultSupplierId |
| DefaultSupplierName | string? | varchar(200) | InStockItemRecord.DefaultSupplierName |
| PackageWay | string? | varchar(100) | InStockItemRecord.PackageWay |

---

## INSERT 邏輯：資料來源原則

### 入庫 InStockItemRecord — 從 WarehouseProduct 快照

| # | 檔案 | 方法 | 場景 | 資料來源 |
|---|------|------|------|----------|
| 1 | StockInService.cs | UpdateAcceptItem | 採購入庫 (PURCHASE) | `product` (WarehouseProduct) |
| 2 | StockInService.cs | OwnerStockInService | 業主直接入庫 (OWNER_DIRECT_IN) | `product` (WarehouseProduct) |
| 3 | AdjustService.cs | AdjustItems | 盤盈(有批次) (ADJUST) | `matchedProduct` (WarehouseProduct) |
| 4 | AdjustService.cs | AdjustItems | 盤盈(無批次) (ADJUST) | `matchedProduct` (WarehouseProduct) |

### 出庫 OutStockRecord — 優先從對應的 InStockItemRecord 取得

| # | 檔案 | 方法 | 場景 | 資料來源 | 說明 |
|---|------|------|------|----------|------|
| 1 | StockOutService.cs | OutStock | 一般出庫 | `inStockItem` (InStockItemRecord) | 有對應入庫 |
| 2 | StockInService.cs | UpdateAcceptItem | 採購直接出庫 | `inStockItemRecord` (剛建立的 InStockItemRecord) | 有對應入庫 |
| 3 | StockOutService.cs | OwnerDirectBatchOut | 業主直接出庫 FIFO | `inStockRecord` (InStockItemRecord) | 有對應入庫 |
| 4 | StockOutService.cs | OwnerPurchaseSubItemsBatchOut | 業主子項出庫 | `matchedProduct` (WarehouseProduct) | 無對應入庫 |
| 5 | AdjustService.cs | AdjustItems | 盤虧(有批次) | `matchedInStockRecord` (InStockItemRecord) | 有對應入庫 |
| 6 | AdjustService.cs | AdjustItems | 盤虧(無批次) | `matchedProduct` (WarehouseProduct) | 無對應入庫 |

---

## READ 邏輯：移除 join warehouse_product

| # | 檔案 | 方法 | 修改內容 |
|---|------|------|----------|
| 1 | StockInController.cs | ListStockInRecords | 移除 join，直接用 record 欄位 |
| 2 | StockOutController.cs | ListStockOutRecords | 移除 join，保留 ReturnStock/Discard/Instrument 處理 |
| 3 | StockOutController.cs | ListStockOutRecordsForOwner | 移除 join，保留 Discard 處理 |
| 4 | StockOutService.cs | SearchByOpenDeadline | 改用 OutStockRecord 欄位（LastAbleDate 仍查 WarehouseProduct） |
| 5 | ReportService.cs | GetProductInAndOutRecords | 移除 join，指定 group 時仍覆蓋 GroupIds/GroupNames |
| 6 | StockInService.cs | GetNearExpiredProductList | **不修改** — 查當前品項設定，非歷史快照需求 |

---

## Summary of All Changes

| Category | Count | Description |
|----------|-------|-------------|
| SQL Migration | 1 file | ALTER TABLE (in_stock +10 cols, out_stock +9 cols) + backfill |
| EF Core Models | 2 files | 19 new properties total (10 + 9) |
| Value Objects | 2 files | Remove 9 redundant properties |
| INSERT Logic | 3 files, 10 locations | 入庫從 product 快照，出庫從入庫紀錄取得 |
| READ Logic | 4 files, 5 locations | Remove WarehouseProduct joins |
