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
  `Status` varchar(45) NOT NULL COMMENT '當下該單據狀態WAIT,AGREE,REJECT',
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
-- Dumping data for table `purchase_flow`
--

LOCK TABLES `purchase_flow` WRITE;
/*!40000 ALTER TABLE `purchase_flow` DISABLE KEYS */;
INSERT INTO `purchase_flow` VALUES ('36552c17-6550-45ca-b0be-56ea73549aeb','68c05e1d-c6f5-498a-8359-d961359e5875','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3','','AGREE','68c05e1d-c6f5-498a-8359-d961359e5875','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任','AGREE',2,'2024-05-16 10:02:48','2024-05-16 08:54:02','2024-05-16 08:49:49','2024-05-16 10:02:47'),('74412696-bcd3-41da-8ad5-2fa88d28e438','68c05e1d-c6f5-498a-8359-d961359e5875','bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3','','WAIT','68c05e1d-c6f5-498a-8359-d961359e5875','f7923e87-a5fb-45b2-9c3a-d24f36b42e31','檢驗科品質主管','AGREE',1,'2024-05-16 08:51:19','2024-05-16 08:51:19','2024-05-16 08:49:49','2024-05-16 08:51:19'),('a037ca43-4b50-45d2-a002-ac1524c5b933','63c0dbc4-fdca-47e8-b084-5880fb596de2','08a9dc9d-b252-4193-9d63-0dd843b5f8db','','AGREE','63c0dbc4-fdca-47e8-b084-5880fb596de2','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員','AGREE',1,'2024-05-16 10:02:07','2024-05-16 08:56:36','2024-05-16 08:56:26','2024-05-16 10:02:07');
/*!40000 ALTER TABLE `purchase_flow` ENABLE KEYS */;
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
