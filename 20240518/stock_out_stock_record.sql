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
  `ProductCode` varchar(200) NOT NULL,
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
-- Dumping data for table `out_stock_record`
--

LOCK TABLES `out_stock_record` WRITE;
/*!40000 ALTER TABLE `out_stock_record` DISABLE KEYS */;
INSERT INTO `out_stock_record` VALUES ('64348dc5-b41a-4608-8f01-db81ff270630','',1,'8873_2024516165847','8873','68c05e1d-c6f5-498a-8359-d961359e5875','2025-05-16',0,'68c05e1d-c6f5-498a-8359-d961359e5875-081','081','r滅菌96孔U型盤附蓋 _MJ96','個','PURCHASE_OUT','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',10,9,'33837078-9a18-4896-850c-9079b6578b6c','8873_2024516165847','2024-05-16 09:22:49','2024-05-16 09:22:49');
/*!40000 ALTER TABLE `out_stock_record` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:06
