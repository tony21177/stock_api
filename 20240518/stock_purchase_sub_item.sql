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
  `UDISerialCode` varchar(300) DEFAULT NULL,
  `PurchaseMainId` varchar(100) NOT NULL COMMENT 'PurchaseMainSheet PK',
  `Quantity` int DEFAULT NULL COMMENT '數量',
  `ReceiveQuantity` int DEFAULT NULL COMMENT '已收到的數量',
  `ReceiveStatus` varchar(45) NOT NULL COMMENT '送單到金萬林後，目前狀態\\nNONE : 尚未收到結果\\nPART : 部分驗收入庫\\nDONE : 全部驗收入庫\\nCLOSE:金萬林不同意拆單後的採購項目',
  `GroupIds` varchar(2000) DEFAULT NULL COMMENT '品項可以設定組別ID\\n在醫院端可以依照組別拆單顯示',
  `GroupNames` varchar(2000) DEFAULT NULL,
  `ProductCategory` varchar(45) DEFAULT NULL COMMENT '品項的 ProductCategory, 用來醫院拆單用',
  `ArrangeSupplierId` int DEFAULT NULL COMMENT '這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單',
  `ArrangeSupplierName` varchar(100) DEFAULT NULL COMMENT '這部分是由得標廠商（金萬林），在收到這張單子的品項後，可以指派該品項的供應商，再進行拆單',
  `CurrentInStockQuantity` int DEFAULT NULL COMMENT '採購單項目在建立當下的庫存數量',
  `CreatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `WithCompId` varchar(100) DEFAULT NULL COMMENT 'Owner從拆單建立就會帶這個參數,表示從WithCompId的採購單拆單出來的',
  `withPurchaseMainId` varchar(100) DEFAULT NULL COMMENT 'Owner從拆單建立就會帶這個參數,對應對WithCompId的採購單purchase_main_sheet.PurchaseMainId',
  `withItemId` varchar(100) DEFAULT NULL COMMENT 'Owner從拆單建立就會帶這個參數,對應對WithCompId的purchase_sub_item.ItemId',
  `SplitProcess` varchar(45) DEFAULT 'NONE' COMMENT 'NONE(表示OWNER尚未拆單過), DONE(表示OWNER已經拆單過)\n',
  PRIMARY KEY (`ItemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='品項的採購單據列表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `purchase_sub_item`
--

LOCK TABLES `purchase_sub_item` WRITE;
/*!40000 ALTER TABLE `purchase_sub_item` DISABLE KEYS */;
INSERT INTO `purchase_sub_item` VALUES ('1475e4f6-72e8-417b-957c-47d18191885c','','63c0dbc4-fdca-47e8-b084-5880fb596de2','63c0dbc4-fdca-47e8-b084-5880fb596de2-162','TECO (RPR)梅毒檢驗試劑','組','','08a9dc9d-b252-4193-9d63-0dd843b5f8db',4,NULL,'NONE','25f9a838-503b-4b93-9975-fe8e3ad324d1','金萬林台中庫房','試劑',29,'醫信有限公司',0,'2024-05-16 08:56:26','2024-05-16 08:56:26','68c05e1d-c6f5-498a-8359-d961359e5875','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3','1c453e88-08b3-44e7-a6d6-aa6c106680ea','DONE'),('1c453e88-08b3-44e7-a6d6-aa6c106680ea','','68c05e1d-c6f5-498a-8359-d961359e5875','68c05e1d-c6f5-498a-8359-d961359e5875-162','TECO (RPR)梅毒檢驗試劑','組','','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',4,NULL,'NONE','25497d82-5ddf-4c6f-9c9b-e03a15ad30e3','血清免疫檢驗','試劑',29,'醫信有限公司',0,'2024-05-16 08:49:49','2024-05-16 08:56:26',NULL,NULL,NULL,'DONE'),('31932db5-f5dc-4344-8778-d729d035f62f','','68c05e1d-c6f5-498a-8359-d961359e5875','68c05e1d-c6f5-498a-8359-d961359e5875-160','速樂定 (TPPA)梅毒檢驗試劑','組','','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',3,NULL,'NONE','25497d82-5ddf-4c6f-9c9b-e03a15ad30e3','血清免疫檢驗','試劑',16,'醫凡企業有限公司',0,'2024-05-16 08:49:49','2024-05-16 08:49:49',NULL,NULL,NULL,'NONE'),('33837078-9a18-4896-850c-9079b6578b6c','','68c05e1d-c6f5-498a-8359-d961359e5875','68c05e1d-c6f5-498a-8359-d961359e5875-081','r滅菌96孔U型盤附蓋 _MJ96','個','','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',10,NULL,'NONE','25497d82-5ddf-4c6f-9c9b-e03a15ad30e3','血清免疫檢驗','耗材',57,'金萬林企業股份有限公司',0,'2024-05-16 08:49:49','2024-05-16 08:49:49',NULL,NULL,NULL,'NONE'),('57e3f39e-3465-4f90-a71b-dbf1dd1e7184','','68c05e1d-c6f5-498a-8359-d961359e5875','68c05e1d-c6f5-498a-8359-d961359e5875-082','r滅菌96孔U型盤附蓋 _MJ96','個','','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',10,NULL,'NONE','25497d82-5ddf-4c6f-9c9b-e03a15ad30e3','血清免疫檢驗','耗材',57,'金萬林企業股份有限公司',0,'2024-05-16 08:49:49','2024-05-16 08:49:49',NULL,NULL,NULL,'NONE');
/*!40000 ALTER TABLE `purchase_sub_item` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:13
