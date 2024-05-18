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
-- Dumping data for table `apply_product_flow_log`
--

LOCK TABLES `apply_product_flow_log` WRITE;
/*!40000 ALTER TABLE `apply_product_flow_log` DISABLE KEYS */;
/*!40000 ALTER TABLE `apply_product_flow_log` ENABLE KEYS */;
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
