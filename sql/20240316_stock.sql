-- MySQL dump 10.13  Distrib 8.0.33, for Win64 (x86_64)
--
-- Host: 211.22.182.205    Database: stock
-- ------------------------------------------------------
-- Server version	8.0.33

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `acceptance_item`
--

DROP TABLE IF EXISTS `acceptance_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `acceptance_item` (
  `AcceptId` varchar(100) NOT NULL,
  `AcceptQuantity` int NOT NULL COMMENT '驗收接受入量，不可大於 OrderQuantity',
  `AcceptUserID` varchar(100) NOT NULL COMMENT '驗收允收者的UserID',
  `LotNumberBatch` varchar(100) NOT NULL COMMENT '批次',
  `LotNumber` varchar(100) NOT NULL COMMENT '批號',
  `CompId` varchar(100) NOT NULL COMMENT '所屬組織ID',
  `ExpirationDate` date NOT NULL COMMENT '保存期限',
  `ItemId` varchar(100) NOT NULL COMMENT '對應 PurchaseSubItem 的 PK',
  `OrderQuantity` int NOT NULL COMMENT '訂購數量，對應 PurchaseSubItem 的 Quantity',
  `PackagingStatus` varchar(45) DEFAULT NULL COMMENT '外觀包裝\nNORMAL : 完成\nBREAK : 破損',
  `ProductId` varchar(100) NOT NULL COMMENT '品項PK',
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `PurchaseMainId` varchar(100) NOT NULL COMMENT 'PurchaseMainSheet 的 PK',
  `QcStatus` varchar(45) DEFAULT NULL COMMENT '驗收測試品管結果\nPASS : 通過\nFAIL : 不通過\nNONEED : 不需側\nOTHER : 其他',
  `CurrentTotalQuantity` int NOT NULL COMMENT '驗收入庫後，當下該品項的總庫存數量',
  `Comment` varchar(500) DEFAULT NULL COMMENT '初驗驗收填寫相關原因',
  `QcComment` varchar(500) DEFAULT NULL COMMENT '二次驗收填寫相關原因',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`AcceptId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='各採購項目驗收紀錄';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `apply_new_product_flow`
--

DROP TABLE IF EXISTS `apply_new_product_flow`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `apply_new_product_flow` (
  `FlowId` varchar(100) NOT NULL,
  `ApplyId` varchar(100) NOT NULL COMMENT '對應 ApplyNewProductMain PK',
  `Answer` varchar(45) NOT NULL COMMENT '回覆結果\\nAGREE : 同意\\nREJECT : 不同意\\n空白 : 未回應',
  `CompId` varchar(100) NOT NULL COMMENT '申請者的來源組織ID',
  `Status` varchar(45) NOT NULL COMMENT '當下該單據狀態',
  `VerifyCompID` varchar(100) NOT NULL COMMENT '審核此單據的組織ID',
  `VerifyUserId` varchar(100) NOT NULL COMMENT '審核此單據的UserID',
  `VerifyUserName` varchar(100) NOT NULL COMMENT '審核此單據的UserName',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`FlowId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='當使用者提出一筆申請品項時，ApplyNewProductFlow 就會新增一筆資料，且每一次變動就會在此表格寫入一筆資料留下申請的審核紀錄。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `apply_new_product_main`
--

DROP TABLE IF EXISTS `apply_new_product_main`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `apply_new_product_main` (
  `ApplyId` varchar(100) NOT NULL,
  `ApplyQuantity` int NOT NULL COMMENT '申請數量',
  `ApplyReason` varchar(300) DEFAULT NULL COMMENT '申請原因',
  `ApplyRemarks` varchar(300) DEFAULT NULL COMMENT '申請備註內容',
  `CompId` varchar(100) NOT NULL COMMENT '申請者的來源組織ID',
  `CurrentStatus` varchar(45) NOT NULL COMMENT '目前狀態',
  `ProductName` varchar(100) NOT NULL COMMENT '申請品項名稱',
  `UserId` varchar(100) NOT NULL COMMENT '申請者的UserID\n對應 Member Table.',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`ApplyId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='當使用者提出一筆申請品項時，ApplyNewProductMain 就會新增一筆資料。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `apply_product_flow_log`
--

DROP TABLE IF EXISTS `apply_product_flow_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `apply_product_flow_log` (
  `LogId` varchar(100) NOT NULL,
  `CompId` varchar(100) NOT NULL COMMENT '申請者的來源組織ID',
  `ApplyId` varchar(100) NOT NULL COMMENT '對應 ApplyNewProductMain PK',
  `UserId` varchar(100) NOT NULL COMMENT '操作者的UserID',
  `UserName` varchar(100) NOT NULL COMMENT '操作者名稱',
  `Action` varchar(45) NOT NULL COMMENT '動作\nNEXT : 下一步\nPREV : 上一步\nCLOSE : 結案',
  `Remarks` varchar(300) NOT NULL COMMENT '備註內容',
  `Sequence` int NOT NULL COMMENT '流程順序',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`LogId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='*假設今天有人員異動，造成申請單據的審核流程卡住，系統會提供一個畫面給該院區管理者，管理者進入後得強制進行審核。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `apply_product_flow_setting`
--

DROP TABLE IF EXISTS `apply_product_flow_setting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `apply_product_flow_setting` (
  `SettingId` varchar(100) NOT NULL,
  `CompId` varchar(100) NOT NULL COMMENT '所屬組織ID',
  `FlowName` varchar(100) NOT NULL COMMENT '審核流程顯示名稱',
  `Sequence` int NOT NULL COMMENT '順序',
  `UserId` varchar(100) NOT NULL COMMENT '此審核流程的審核者',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`SettingId`,`CompId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='系統先行設定申請的審核流程';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `company`
--

DROP TABLE IF EXISTS `company`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `company` (
  `CompId` varchar(100) NOT NULL COMMENT 'PK, 公司ID',
  `Name` varchar(100) NOT NULL COMMENT '顯示名稱',
  `IsActive` tinyint(1) DEFAULT '1' COMMENT '是否激活狀態',
  `Type` varchar(20) NOT NULL COMMENT '類型\nOWNER : 系統擁有者（廠商）\nORGANIZATION : 機構（醫院）',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`CompId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Company 規範目前庫存系統內的組織，透過 Type 欄位來區分組織是屬於一般組織，或是系統擁有者（廠商），廠商可以具有權限查閱所有機構的庫存資料。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `company_unit`
--

DROP TABLE IF EXISTS `company_unit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `company_unit` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UnitId` varchar(100) NOT NULL COMMENT '如果是同一單位，就會是一樣的 UnitID',
  `UnitName` varchar(100) DEFAULT NULL COMMENT '單位名稱',
  `CompId` varchar(100) NOT NULL COMMENT '組織ID',
  `CompName` varchar(100) NOT NULL COMMENT '組織名稱',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `CompId_UNIQUE` (`CompId`),
  UNIQUE KEY `ComId_UnitId_UNIQUE` (`UnitId`,`CompId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='醫院院區關係資料';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `in_stock_item_record`
--

DROP TABLE IF EXISTS `in_stock_item_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `in_stock_item_record` (
  `InStockId` varchar(100) NOT NULL,
  `LotNumberBatch` varchar(100) NOT NULL COMMENT '批次',
  `LotNumber` varchar(100) NOT NULL COMMENT '批號',
  `CompId` varchar(100) NOT NULL COMMENT '所屬公司ID',
  `OriginalQuantity` int NOT NULL COMMENT '現有庫存量',
  `ExpirationDate` date DEFAULT NULL COMMENT '保存期限',
  `ItemId` varchar(100) DEFAULT NULL COMMENT '對應 PurchaseSubItem 的 PK\n非採購入庫，NULL',
  `InStockQuantity` int NOT NULL COMMENT '此次入庫數量',
  `ProductId` varchar(100) NOT NULL COMMENT '品項PK',
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `Type` varchar(45) NOT NULL COMMENT '類型\nPURCHASE : 來源是採購\nSHIFT : 調撥\nADJUST : 調整（盤盈）\nRETURN : 退庫',
  `BarCodeNumber` varchar(200) NOT NULL COMMENT '用來產生條碼的數字，PadLeft : 7個0\nExample : 0000001',
  `UserId` varchar(100) NOT NULL COMMENT '執行入庫人員的UserID',
  `UserName` varchar(100) NOT NULL COMMENT '執行入庫人員的UserName',
  `AfterQuantity` int NOT NULL COMMENT '入庫後數量',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`InStockId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='每一筆要更新庫存紀錄（增加）的操作，都需要寫入一筆記錄在 InStockRecord，包含採購驗收、調撥、盤點（盤盈）、退庫，類型寫在 Type 欄位。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inventory_adjust_main`
--

DROP TABLE IF EXISTS `inventory_adjust_main`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory_adjust_main` (
  `InventoryId` int NOT NULL AUTO_INCREMENT,
  `CompId` varchar(100) NOT NULL COMMENT '該單據所屬的公司ID',
  `AdjustCompId` varchar(100) DEFAULT NULL COMMENT '如果單據屬於調撥單，則填入被調撥的公司ID。\n非調撥單，保持空值',
  `Type` varchar(45) NOT NULL COMMENT 'SHIFT : 調撥\nADJUST : 盤點調整\nRETURN : 退庫\nRETURN_OUT : 退貨',
  `UserId` varchar(100) NOT NULL COMMENT '此單據的建立者',
  `CurrentStatus` varchar(45) NOT NULL COMMENT '目前狀態\nAPPLY : 申請中\nAGREE : 同意\nREJECT : 拒絕\nCLOSE : 結案',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`InventoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='盤點及調撥處理主要單據\n每一筆要更新庫存紀錄（增加）的操作，都需要寫入一筆記錄在 InStockRecord，包含採購驗收、調撥、盤點（盤盈）、退庫，類型寫在 Type 欄位。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `manufacturer`
--

DROP TABLE IF EXISTS `manufacturer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `manufacturer` (
  `Id` varchar(100) NOT NULL,
  `Code` varchar(100) NOT NULL COMMENT '製造商編號',
  `Name` varchar(100) DEFAULT NULL COMMENT '製造商名稱',
  `Remark` varchar(100) DEFAULT NULL COMMENT '備註訊息',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否激活狀態',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Code_UNIQUE` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Manufacturer 這一個表格主要是用來記錄「庫存品項的製造商」，建立此清單後，當使用者在新增庫存品項時就可直接自動下拉選擇。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `out_stock_record`
--

DROP TABLE IF EXISTS `out_stock_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `out_stock_record` (
  `OutStockId` varchar(100) NOT NULL,
  `AbnormalReason` varchar(300) DEFAULT NULL COMMENT '異常原因，如果沒有異常則保持空',
  `ApplyQuantity` int NOT NULL COMMENT '出庫數量',
  `LotNumberBatch` varchar(100) NOT NULL COMMENT '批次',
  `LotNumber` varchar(100) NOT NULL COMMENT '批號',
  `CompId` varchar(100) NOT NULL COMMENT '所屬公司ID',
  `ExpirationDate` date DEFAULT NULL COMMENT '保存期限',
  `IsAbnormal` tinyint(1) NOT NULL COMMENT '是否為出庫異常\n0為false,1為true',
  `ProductId` varchar(100) NOT NULL COMMENT '品項PK',
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `Type` varchar(45) NOT NULL COMMENT '類型\nPURCHASE_OUT : 來源是採購\nSHIFT_OUT : 被調撥\nADJUST_OUT : 調整（盤虧）\nRETURN_OUT : 退貨',
  `UserId` varchar(100) NOT NULL COMMENT '執行出庫人員的UserID',
  `UserName` varchar(100) NOT NULL COMMENT '執行出庫人員的UserName',
  `OriginalQuantity` int NOT NULL COMMENT '現有庫存量',
  `AfterQuantity` int NOT NULL COMMENT '入庫後數量',
  `ItemId` varchar(100) DEFAULT NULL COMMENT '對應 PurchaseSubItem 的 PK\n非採購入庫，NULL',
  `BarCodeNumber` varchar(200) NOT NULL COMMENT '用來產生條碼的數字，PadLeft : 7個0\nExample : 0000001',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`OutStockId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='每一筆要更新庫存紀錄（增加）的操作，都需要寫入一筆記錄在 InStockRecord，包含採購驗收、調撥、盤點（盤盈）、退庫，類型寫在 Type 欄位。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `purchase_flow`
--

DROP TABLE IF EXISTS `purchase_flow`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `purchase_flow` (
  `FlowId` varchar(100) NOT NULL,
  `CompId` varchar(100) NOT NULL COMMENT '所屬組織ID',
  `PurchaseMainId` varchar(100) NOT NULL COMMENT 'PurchaseMainSheet PK',
  `Reason` varchar(300) DEFAULT NULL COMMENT '流程備註內容',
  `Status` varchar(45) NOT NULL COMMENT '當下該單據狀態',
  `VerifyCompID` varchar(100) NOT NULL COMMENT '審核人員所屬公司ID',
  `VerifyUserId` varchar(100) NOT NULL COMMENT '審核人員的UserID',
  `VerifyUserName` varchar(100) NOT NULL COMMENT '審核人員的UserName',
  `Answer` varchar(45) NOT NULL COMMENT '回覆結果\nAGREE : 同意\nREJECT : 不同意\n空白 : 未回應',
  `Sequence` int NOT NULL COMMENT '流程順序',
  `ReadAt` timestamp NULL DEFAULT NULL COMMENT '讀取時間',
  `SubmitAt` timestamp NULL DEFAULT NULL COMMENT '送出時間',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`FlowId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='品項的採購單據流程審核紀錄';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `purchase_flow_log`
--

DROP TABLE IF EXISTS `purchase_flow_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `purchase_flow_log` (
  `LogId` varchar(100) NOT NULL,
  `CompId` varchar(100) NOT NULL COMMENT '所屬組織ID',
  `PurchaseMainId` varchar(100) NOT NULL COMMENT 'PurchaseMainSheet PK',
  `UserId` varchar(100) NOT NULL COMMENT '紀錄操作者的UserID',
  `UserName` varchar(100) NOT NULL COMMENT '紀錄操作者的UserName',
  `Sequence` int NOT NULL COMMENT '流程順序',
  `Action` varchar(45) NOT NULL COMMENT '動作\nNEXT : 下一步\nPREV : 上一步\nCLOSE : 結案',
  `Remarks` varchar(300) DEFAULT NULL COMMENT '備註內容',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`LogId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='*假設今天有人員異動，造成採購單據的審核流程卡住，系統會提供一個畫面給該院區管理者，管理者進入後得強制進行審核。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `purchase_flow_setting`
--

DROP TABLE IF EXISTS `purchase_flow_setting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `purchase_flow_setting` (
  `FlowId` varchar(100) NOT NULL,
  `CompId` varchar(100) NOT NULL COMMENT '所屬組織ID',
  `FlowName` varchar(100) NOT NULL COMMENT '流程名稱',
  `Sequence` int NOT NULL COMMENT '流程順序',
  `UserId` varchar(100) NOT NULL COMMENT '此審核流程的審核者',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`FlowId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='系統先行設定採購的審核流程';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `purchase_main_sheet`
--

DROP TABLE IF EXISTS `purchase_main_sheet`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `purchase_main_sheet` (
  `PurchaseMainId` varchar(100) NOT NULL,
  `ApplyDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '申請日期',
  `CompId` varchar(100) NOT NULL COMMENT '所屬公司ID',
  `CurrentStatus` varchar(45) NOT NULL COMMENT '目前狀態\nAPPLY : 申請中\nAGREE : 同意\nREJECT : 拒絕\nCLOSE : 結案',
  `DemandDate` date DEFAULT NULL COMMENT '需求日期',
  `GroupId` json DEFAULT NULL COMMENT '設定此單據所屬的組別，參考 Warehouse_Group',
  `Remarks` varchar(300) DEFAULT NULL COMMENT '備註內容',
  `UserId` varchar(100) NOT NULL COMMENT '此採購單據的建立者',
  `ReceiveStatus` varchar(100) NOT NULL COMMENT '送單到金萬林後，目前狀態\nNONE : 尚未收到結果\nDELIVERED : 金萬林已出貨\nIN_ACCEPTANCE_CHECK : 驗收中\nPART_ACCEPT : 部分驗收入庫\nALL_ACCEPT : 全部驗收入庫',
  `Type` varchar(45) DEFAULT NULL COMMENT '採購單類型\nGENERAL : 一般訂單\nURGENT : 緊急訂單',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PurchaseMainId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='品項的採購單據主體';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `purchase_sub_item`
--

DROP TABLE IF EXISTS `purchase_sub_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `purchase_sub_item` (
  `ItemId` varchar(100) NOT NULL,
  `Comment` varchar(300) DEFAULT NULL COMMENT '品項備註內容',
  `CompId` varchar(100) NOT NULL COMMENT '所屬組織ID',
  `ProductId` varchar(100) NOT NULL COMMENT '品項的PK，\n參考 Product Table',
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `PurchaseMainId` varchar(100) NOT NULL COMMENT 'PurchaseMainSheet PK',
  `Quantity` int DEFAULT NULL COMMENT '數量',
  `ReceiveQuantity` int DEFAULT NULL COMMENT '已收到的數量',
  `ReceiveStatus` varchar(45) NOT NULL COMMENT '送單到金萬林後，目前狀態\nNONE : 尚未收到結果\nPART : 部分驗收入庫\nDONE : 全部驗收入庫',
  `GroupId` json DEFAULT NULL COMMENT '品項可以設定組別ID\n在醫院端可以依照組別拆單顯示',
  `GroupName` json DEFAULT NULL,
  `ProductCategory` varchar(45) DEFAULT NULL COMMENT '品項的 ProductCategory, 用來醫院拆單用',
  `ArrangeSupplierId` varchar(100) DEFAULT NULL COMMENT '這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單',
  `ArrangeSupplierName` varchar(100) DEFAULT NULL COMMENT '這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單',
  `CurrentInStockQuantity` int DEFAULT NULL COMMENT '採購單項目在建立當下的庫存數量',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`ItemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='品項的採購單據列表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `supplier`
--

DROP TABLE IF EXISTS `supplier`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `supplier` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CompId` varchar(100) NOT NULL COMMENT '屬於庫存系統裡面的哪一個組織所有',
  `Code` varchar(45) NOT NULL COMMENT '供應商編號',
  `CompanyPhone` varchar(100) DEFAULT NULL COMMENT '供應商公司電話',
  `ContactUser` varchar(100) DEFAULT NULL COMMENT '供應商聯絡人',
  `ContactUserPhone` varchar(100) DEFAULT NULL COMMENT '供應商聯絡人電話',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否激活狀態',
  `Name` varchar(100) DEFAULT NULL COMMENT '供應商名稱',
  `Remark` varchar(100) DEFAULT NULL COMMENT '備註訊息',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Supplier 這一個表格主要是用來記錄「庫存品項的供應商」，建立此清單後，當使用者在新增庫存品項時就可直接自動下拉選擇。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `temp_in_stock_item_record`
--

DROP TABLE IF EXISTS `temp_in_stock_item_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `temp_in_stock_item_record` (
  `InStockId` varchar(100) NOT NULL,
  `LotNumberBatch` varchar(100) NOT NULL COMMENT '批次',
  `LotNumber` varchar(100) NOT NULL COMMENT '批號',
  `CompId` varchar(100) NOT NULL COMMENT '所屬公司ID',
  `OriginalQuantity` int NOT NULL COMMENT '現有庫存量',
  `ExpirationDate` date DEFAULT NULL COMMENT '保存期限',
  `InStockQuantity` int NOT NULL COMMENT '此次入庫數量',
  `ProductId` varchar(100) NOT NULL COMMENT '品項PK',
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `Type` varchar(45) NOT NULL COMMENT '類型\nPURCHASE : 來源是採購\nSHIFT : 調撥\nADJUST : 調整（盤盈）\nRETURN : 退庫',
  `UserId` varchar(100) NOT NULL COMMENT '執行入庫人員的UserID',
  `UserName` varchar(100) NOT NULL COMMENT '執行入庫人員的UserName',
  `IsTransfer` tinyint(1) NOT NULL COMMENT '用來判斷暫存項目是不是已經有複製過去 InStockItemRecord,(0)false: 未複製,(1)true: 已複製',
  `InventoryID` varchar(100) NOT NULL COMMENT '用來判斷屬於哪一張主單',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`InStockId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='盤點和調撥單據，如果是要入庫的話，會先寫入一筆資料到這個暫時的表，等到審核通過再複製到 InStockItemRecord.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `temp_out_stock_record`
--

DROP TABLE IF EXISTS `temp_out_stock_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `temp_out_stock_record` (
  `OutStockId` varchar(100) NOT NULL,
  `AbnormalReason` varchar(300) DEFAULT NULL COMMENT '異常原因，如果沒有異常則保持空',
  `ApplyQuantity` int NOT NULL COMMENT '出庫數量',
  `IsAbnormal` tinyint(1) NOT NULL COMMENT '是否為出庫異常\n0為false,1為true',
  `LotNumberBatch` varchar(100) NOT NULL COMMENT '批次',
  `LotNumber` varchar(100) NOT NULL COMMENT '批號',
  `CompId` varchar(100) NOT NULL COMMENT '所屬公司ID',
  `ExpirationDate` date DEFAULT NULL COMMENT '保存期限',
  `ProductId` varchar(100) NOT NULL COMMENT '品項PK',
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `Type` varchar(45) NOT NULL COMMENT '類型\nPURCHASE_OUT : 來源是採購\nSHIFT_OUT : 被調撥\nADJUST_OUT : 調整（盤虧）\nRETURN_OUT : 退貨',
  `UserId` varchar(100) NOT NULL COMMENT '執行出庫人員的UserID',
  `UserName` varchar(100) NOT NULL COMMENT '執行出庫人員的UserName',
  `OriginalQuantity` int NOT NULL COMMENT '現有庫存量',
  `IsTransfer` tinyint(1) NOT NULL COMMENT '用來判斷暫存項目是不是已經有複製過去 InStockItemRecord,(0)false: 未複製,(1)true: 已複製',
  `InventoryID` varchar(100) NOT NULL COMMENT '用來判斷屬於哪一張主單',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`OutStockId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='盤點和調撥單據，如果是要出庫的話，會先寫入一筆資料到這個暫時的表，等到審核通過再複製到 OutStockRecord.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `warehouse_authlayer`
--

DROP TABLE IF EXISTS `warehouse_authlayer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `warehouse_authlayer` (
  `AuthId` int NOT NULL AUTO_INCREMENT,
  `AuthDescription` varchar(100) DEFAULT NULL COMMENT '權限描述',
  `AuthName` varchar(100) NOT NULL COMMENT '權限名稱',
  `AuthValue` smallint NOT NULL COMMENT '權限值\n1: 得標廠商\n3: 最高層級\n5: 第一層級\n7: 第二層級\n9: 第三層級',
  `IsApplyItemManage` tinyint(1) NOT NULL COMMENT '是否可以申請新增品項',
  `IsGroupManage` tinyint(1) NOT NULL COMMENT '是否可以進行群組管理',
  `IsInBoundManage` tinyint(1) NOT NULL COMMENT '是否可以進行入庫作業',
  `IsOutBoundManage` tinyint(1) NOT NULL COMMENT '是否可以進行出庫作業',
  `IsInventoryManage` tinyint(1) NOT NULL COMMENT '是否可以進行庫存管理',
  `IsItemManage` tinyint(1) NOT NULL COMMENT '是否可以進行品項管理',
  `IsMemberManage` tinyint(1) NOT NULL COMMENT '是否可以進行成員管理',
  `IsRestockManage` tinyint(1) NOT NULL COMMENT '是否可以進行盤點',
  `IsVerifyManage` tinyint(1) NOT NULL COMMENT '是否可以進行抽點',
  `CompId` varchar(100) NOT NULL COMMENT '屬於庫存系統裡面的哪一個公司內所有\\\\n對應 -> Company Table"',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`AuthId`),
  UNIQUE KEY `CompID_UNIQUE` (`CompId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='主要是用來設定登入人員的權限。\n一般組之內，設定人員權限時，僅有 3, 5, 7, 9 的 AuthValue 選項。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `warehouse_group`
--

DROP TABLE IF EXISTS `warehouse_group`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `warehouse_group` (
  `GroupId` varchar(100) NOT NULL,
  `GroupName` varchar(45) DEFAULT NULL COMMENT '群組名稱',
  `GroupDescription` varchar(100) DEFAULT NULL COMMENT '群組描述',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否激活狀態',
  `CompId` varchar(100) NOT NULL COMMENT '屬於庫存系統裡面的哪一個組織所有',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`GroupId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='主要是用來設定「人員」及「品項」可以分類為哪些組別來使用的，在新增品項和新增人員的時候，都可以從下拉選單中，選擇已建立的組別來加以管理。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `warehouse_member`
--

DROP TABLE IF EXISTS `warehouse_member`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `warehouse_member` (
  `Account` varchar(100) NOT NULL DEFAULT '登入帳號',
  `AuthValue` smallint NOT NULL COMMENT '權限值',
  `DisplayName` varchar(100) NOT NULL COMMENT '顯示名稱',
  `GroupId` json DEFAULT NULL COMMENT '屬於數個組別',
  `Password` varchar(100) NOT NULL COMMENT '登入密碼',
  `PhotoURL` varchar(500) DEFAULT NULL COMMENT '大頭貼',
  `CompId` varchar(100) NOT NULL COMMENT '屬於庫存系統裡面的哪一個組織所有',
  `UserId` varchar(100) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Account_UNIQUE` (`Account`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='主要是用來設定系統人員登入的相關資料。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `warehouse_product`
--

DROP TABLE IF EXISTS `warehouse_product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `warehouse_product` (
  `LotNumberBatch` varchar(100) NOT NULL COMMENT '批次',
  `LotNumber` varchar(100) NOT NULL COMMENT '批號',
  `CompId` varchar(45) NOT NULL COMMENT '品項所屬的組織ID',
  `ManufacturerId` varchar(100) NOT NULL COMMENT '品項所屬的製造商ID',
  `ManufacturerName` varchar(100) NOT NULL COMMENT '品項所屬的製造商名稱',
  `DeadlineRule` varchar(500) DEFAULT NULL COMMENT '有效期限規範',
  `DeliverFunction` varchar(500) DEFAULT NULL COMMENT '運送條件',
  `DeliverRemarks` varchar(500) DEFAULT NULL COMMENT '運送備註',
  `GroupId` json DEFAULT NULL COMMENT '屬於數個組別',
  `GroupName` varchar(100) DEFAULT NULL COMMENT '組別名稱',
  `InStockQuantity` double DEFAULT NULL COMMENT '庫存數量',
  `Manager` varchar(45) DEFAULT NULL COMMENT '管理者',
  `MaxSafeQuantity` double DEFAULT NULL COMMENT '最高安庫量',
  `LastAbleDate` date DEFAULT NULL COMMENT '最後可使用日期',
  `LastOutStockDate` date DEFAULT NULL COMMENT '最後出庫日期',
  `OpenDeadline` int DEFAULT NULL COMMENT '開封有效期限\n數字（開封後可以用幾天），檢查資料庫是不是int',
  `OpenedSealName` varchar(100) DEFAULT NULL COMMENT '開封列印名稱',
  `OriginalDeadline` date DEFAULT NULL COMMENT '原始有效期限',
  `PackageWay` varchar(100) DEFAULT NULL COMMENT '包裝方式',
  `PreDeadline` int DEFAULT NULL,
  `PreOrderDays` int DEFAULT NULL COMMENT '前置天數',
  `ProductCategory` varchar(100) DEFAULT NULL COMMENT '產品類別\n[耗材, 試劑, 其他]',
  `ProductCode` varchar(200) DEFAULT NULL COMMENT '產品編碼',
  `ProductId` varchar(100) NOT NULL,
  `ProductModel` varchar(200) DEFAULT NULL COMMENT '品項型號',
  `ProductName` varchar(200) DEFAULT NULL COMMENT '品項名稱',
  `ProductRemarks` varchar(300) DEFAULT NULL COMMENT '品項備註',
  `ProductSpec` varchar(300) DEFAULT NULL COMMENT '品項規格',
  `SafeQuantity` double DEFAULT NULL COMMENT '最小安庫量',
  `SavingFunction` varchar(300) DEFAULT NULL COMMENT '儲存環境條件',
  `UDIBatchCode` varchar(300) DEFAULT NULL COMMENT 'UDI 碼',
  `UDICreateCode` varchar(300) DEFAULT NULL COMMENT 'UDI 碼',
  `UDISerialCode` varchar(300) DEFAULT NULL COMMENT 'UDI 碼',
  `UDIVerifyDateCode` varchar(300) DEFAULT NULL COMMENT 'UDI 碼',
  `Unit` varchar(100) DEFAULT NULL COMMENT '單位',
  `Weight` varchar(100) DEFAULT NULL COMMENT '重量',
  `ProductMachine` varchar(200) DEFAULT NULL COMMENT '品項所屬儀器',
  `DefaultSupplierID` varchar(200) DEFAULT NULL COMMENT '預設供應商',
  `DefaultSupplierName` varchar(200) DEFAULT NULL COMMENT '預設供應商名稱',
  `IsNeedAcceptProcess` tinyint(1) DEFAULT '0' COMMENT '該品項出庫時，是否需要經過二次驗收',
  `AllowReceiveDateRange` int DEFAULT NULL COMMENT '該品項期限距離現在的最小天數',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`ProductId`),
  UNIQUE KEY `ManufacturerId_UNIQUE` (`ManufacturerId`),
  UNIQUE KEY `UDISerialCode_UNIQUE` (`UDISerialCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='庫存品項基本資料，因為要考慮到調撥這件事情，也就是從醫院單位A把項目移動部分庫存量到醫院單位B；所以，任一相同品項，在不同的醫院單位內的 ProductID 應該是一制，在這條件下，Product 表格的 PK 應是 CompID + ProductID';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-03-16 23:24:59
