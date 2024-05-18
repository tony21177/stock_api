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
-- Dumping data for table `apply_new_product_flow`
--

LOCK TABLES `apply_new_product_flow` WRITE;
/*!40000 ALTER TABLE `apply_new_product_flow` DISABLE KEYS */;
/*!40000 ALTER TABLE `apply_new_product_flow` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:17
