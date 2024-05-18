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
  `GroupIds` varchar(2000) DEFAULT NULL COMMENT '設定此單據所屬的組別，參考 Warehouse_Group',
  `Remarks` varchar(300) DEFAULT NULL COMMENT '備註內容',
  `UserId` varchar(100) NOT NULL COMMENT '此採購單據的建立者',
  `ReceiveStatus` varchar(100) NOT NULL COMMENT '送單到金萬林後，目前狀態\nNONE : 尚未收到結果\nDELIVERED : 金萬林已出貨\nIN_ACCEPTANCE_CHECK : 驗收中\nPART_ACCEPT : 部分驗收入庫\nALL_ACCEPT : 全部驗收入庫',
  `Type` varchar(45) DEFAULT NULL COMMENT '採購單類型\nGENERAL : 一般訂單\nURGENT : 緊急訂單',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `IsActive` tinyint(1) NOT NULL,
  `SplitPrcoess` varchar(45) DEFAULT 'NONE' COMMENT 'NONE(所有sub_item都尚未經過OWNER拆單),PART(部分sub_item經過OWNER拆單),DONE(所有sub_item經過OWNER拆單)',
  PRIMARY KEY (`PurchaseMainId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='品項的採購單據主體';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `purchase_main_sheet`
--

LOCK TABLES `purchase_main_sheet` WRITE;
/*!40000 ALTER TABLE `purchase_main_sheet` DISABLE KEYS */;
INSERT INTO `purchase_main_sheet` VALUES ('08a9dc9d-b252-4193-9d63-0dd843b5f8db','2024-05-16 08:56:26','63c0dbc4-fdca-47e8-b084-5880fb596de2','AGREE','2024-05-30','25f9a838-503b-4b93-9975-fe8e3ad324d1,25f9a838-503b-4b93-9975-fe8e3ad324d2','','c3f28016-9e85-441c-826a-743ff9314433','NONE','GENERAL','2024-05-16 08:56:26','2024-05-16 08:56:35',1,'DONE'),('bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3','2024-05-16 08:49:49','68c05e1d-c6f5-498a-8359-d961359e5875','AGREE','2024-05-30','d513d7e7-d661-49aa-b1ec-132a932030ec,a8a9fdd1-5f80-42c1-a52a-e1cbabc4471d','','b34f1c15-f459-4c7c-b460-8198c7aeac06','PART_ACCEPT','URGENT','2024-05-16 08:49:49','2024-05-16 08:58:47',1,'PART');
/*!40000 ALTER TABLE `purchase_main_sheet` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:09
