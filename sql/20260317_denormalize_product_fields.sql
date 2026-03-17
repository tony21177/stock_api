-- ============================================================
-- in_stock_item_record: 新增品項基本資料快照欄位
-- ============================================================
ALTER TABLE `in_stock_item_record`
  ADD COLUMN IF NOT EXISTS `ProductModel` varchar(200) DEFAULT NULL COMMENT '品項型號',
  ADD COLUMN IF NOT EXISTS `Unit` varchar(100) DEFAULT NULL COMMENT '庫存單位',
  ADD COLUMN IF NOT EXISTS `GroupIds` varchar(2000) DEFAULT NULL COMMENT '組別IDs',
  ADD COLUMN IF NOT EXISTS `GroupNames` varchar(2000) DEFAULT NULL COMMENT '組別名稱',
  ADD COLUMN IF NOT EXISTS `PreDeadline` int DEFAULT NULL COMMENT '末效期前幾天通知',
  ADD COLUMN IF NOT EXISTS `OpenDeadline` int DEFAULT NULL COMMENT '開封有效期限(天數)',
  ADD COLUMN IF NOT EXISTS `IsAllowDiscard` tinyint(1) DEFAULT NULL COMMENT '是否允許丟棄',
  ADD COLUMN IF NOT EXISTS `DefaultSupplierID` int DEFAULT NULL COMMENT '預設供應商ID',
  ADD COLUMN IF NOT EXISTS `DefaultSupplierName` varchar(200) DEFAULT NULL COMMENT '預設供應商名稱',
  ADD COLUMN IF NOT EXISTS `PackageWay` varchar(100) DEFAULT NULL COMMENT '包裝方式';

-- ============================================================
-- out_stock_record: 新增品項基本資料快照欄位
-- ============================================================
ALTER TABLE `out_stock_record`
  ADD COLUMN IF NOT EXISTS `Unit` varchar(100) DEFAULT NULL COMMENT '庫存單位',
  ADD COLUMN IF NOT EXISTS `OpenDeadline` int DEFAULT NULL COMMENT '開封有效期限(天數)',
  ADD COLUMN IF NOT EXISTS `ProductModel` varchar(200) DEFAULT NULL COMMENT '品項型號',
  ADD COLUMN IF NOT EXISTS `GroupIds` varchar(2000) DEFAULT NULL COMMENT '組別IDs',
  ADD COLUMN IF NOT EXISTS `GroupNames` varchar(2000) DEFAULT NULL COMMENT '組別名稱',
  ADD COLUMN IF NOT EXISTS `IsAllowDiscard` tinyint(1) DEFAULT NULL COMMENT '是否允許丟棄',
  ADD COLUMN IF NOT EXISTS `DefaultSupplierID` int DEFAULT NULL COMMENT '預設供應商ID',
  ADD COLUMN IF NOT EXISTS `DefaultSupplierName` varchar(200) DEFAULT NULL COMMENT '預設供應商名稱',
  ADD COLUMN IF NOT EXISTS `PackageWay` varchar(100) DEFAULT NULL COMMENT '包裝方式';

-- ============================================================
-- 回填歷史資料 (用當前 warehouse_product 值)
-- ============================================================
UPDATE `in_stock_item_record` r
  JOIN `warehouse_product` p ON r.ProductId = p.ProductId AND r.CompId = p.CompId
SET r.ProductModel = p.ProductModel,
    r.Unit = p.Unit,
    r.GroupIds = p.GroupIds,
    r.GroupNames = p.GroupNames,
    r.PreDeadline = p.PreDeadline,
    r.OpenDeadline = p.OpenDeadline,
    r.IsAllowDiscard = p.IsAllowDiscard,
    r.DefaultSupplierID = p.DefaultSupplierID,
    r.DefaultSupplierName = p.DefaultSupplierName,
    r.PackageWay = p.PackageWay;

UPDATE `out_stock_record` r
  JOIN `warehouse_product` p ON r.ProductId = p.ProductId AND r.CompId = p.CompId
SET r.Unit = p.Unit,
    r.OpenDeadline = p.OpenDeadline,
    r.ProductModel = p.ProductModel,
    r.GroupIds = p.GroupIds,
    r.GroupNames = p.GroupNames,
    r.IsAllowDiscard = p.IsAllowDiscard,
    r.DefaultSupplierID = p.DefaultSupplierID,
    r.DefaultSupplierName = p.DefaultSupplierName,
    r.PackageWay = p.PackageWay;
