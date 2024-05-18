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
  `PurchaseMainId` varchar(100) NOT NULL COMMENT 'PurchaseMainSheet 的 PK',
  `AcceptQuantity` int DEFAULT NULL COMMENT '驗收接受數量，不可大於 OrderQuantity',
  `AcceptUserID` varchar(100) DEFAULT NULL COMMENT '驗收允收者的UserID',
  `LotNumberBatch` varchar(100) DEFAULT NULL COMMENT '批次',
  `LotNumber` varchar(100) DEFAULT NULL COMMENT '批號',
  `CompId` varchar(100) NOT NULL COMMENT '所屬組織ID',
  `ExpirationDate` date DEFAULT NULL COMMENT '保存期限',
  `ItemId` varchar(100) NOT NULL COMMENT '對應 PurchaseSubItem 的 PK',
  `OrderQuantity` int NOT NULL COMMENT '訂購數量，對應 PurchaseSubItem 的 Quantity',
  `PackagingStatus` varchar(45) DEFAULT NULL COMMENT '外觀包裝\nNORMAL : 完成\nBREAK : 破損',
  `ProductId` varchar(100) NOT NULL COMMENT '品項PK',
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `UDISerialCode` varchar(300) DEFAULT NULL,
  `QcStatus` varchar(45) DEFAULT NULL COMMENT '驗收測試品管結果\nPASS : 通過\nFAIL : 不通過\nNONEED : 不需側\nOTHER : 其他',
  `CurrentTotalQuantity` int DEFAULT NULL COMMENT '驗收入庫後，當下該品項的總庫存數量',
  `Comment` varchar(500) DEFAULT NULL COMMENT '初驗驗收填寫相關原因',
  `QcComment` varchar(500) DEFAULT NULL COMMENT '二次驗收填寫相關原因',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `DeliverFunction` varchar(45) DEFAULT NULL,
  `DeliverTemperature` double DEFAULT NULL,
  `SavingFunction` varchar(45) DEFAULT NULL,
  `SavingTemperature` double DEFAULT NULL,
  `isInStocked` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`AcceptId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='各採購項目驗收紀錄';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `acceptance_item`
--

LOCK TABLES `acceptance_item` WRITE;
/*!40000 ALTER TABLE `acceptance_item` DISABLE KEYS */;
INSERT INTO `acceptance_item` VALUES ('2de1863e-3ef6-4279-9991-9f8bbd3e2b3a','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',10,'b34f1c15-f459-4c7c-b460-8198c7aeac06','8873_2024516165847','8873','68c05e1d-c6f5-498a-8359-d961359e5875','2025-05-16','33837078-9a18-4896-850c-9079b6578b6c',10,'NORMAL','68c05e1d-c6f5-498a-8359-d961359e5875-081','r滅菌96孔U型盤附蓋 _MJ96','個','','PASS',10,'',NULL,'2024-05-16 08:54:02','2024-05-16 08:58:47',NULL,NULL,NULL,NULL,1),('670d7cad-ecd3-471b-a3dc-57fbed02158a','08a9dc9d-b252-4193-9d63-0dd843b5f8db',NULL,NULL,NULL,NULL,'63c0dbc4-fdca-47e8-b084-5880fb596de2',NULL,'1475e4f6-72e8-417b-957c-47d18191885c',4,NULL,'63c0dbc4-fdca-47e8-b084-5880fb596de2-162','TECO (RPR)梅毒檢驗試劑','組','',NULL,NULL,NULL,NULL,'2024-05-16 08:56:35','2024-05-16 08:56:35',NULL,NULL,NULL,NULL,0),('6f25562e-b3e2-4655-b733-0daabfb71bfd','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',NULL,NULL,NULL,NULL,'68c05e1d-c6f5-498a-8359-d961359e5875',NULL,'31932db5-f5dc-4344-8778-d729d035f62f',3,NULL,'68c05e1d-c6f5-498a-8359-d961359e5875-160','速樂定 (TPPA)梅毒檢驗試劑','組','',NULL,NULL,NULL,NULL,'2024-05-16 08:54:02','2024-05-16 08:54:02',NULL,NULL,NULL,NULL,0),('7d4a7d3b-8bfd-4131-af8b-dfe3a2ac3284','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',NULL,NULL,NULL,NULL,'68c05e1d-c6f5-498a-8359-d961359e5875',NULL,'1c453e88-08b3-44e7-a6d6-aa6c106680ea',4,NULL,'68c05e1d-c6f5-498a-8359-d961359e5875-162','TECO (RPR)梅毒檢驗試劑','組','',NULL,NULL,NULL,NULL,'2024-05-16 08:54:02','2024-05-16 08:54:02',NULL,NULL,NULL,NULL,0),('a3635796-dc26-4aba-9c50-65f0c59be1d2','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3',NULL,NULL,NULL,NULL,'68c05e1d-c6f5-498a-8359-d961359e5875',NULL,'57e3f39e-3465-4f90-a71b-dbf1dd1e7184',10,NULL,'68c05e1d-c6f5-498a-8359-d961359e5875-082','r滅菌96孔U型盤附蓋 _MJ96','個','',NULL,NULL,NULL,NULL,'2024-05-16 08:54:02','2024-05-16 08:54:02',NULL,NULL,NULL,NULL,0);
/*!40000 ALTER TABLE `acceptance_item` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:07
