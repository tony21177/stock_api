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
-- Dumping data for table `inventory_adjust_main`
--

LOCK TABLES `inventory_adjust_main` WRITE;
/*!40000 ALTER TABLE `inventory_adjust_main` DISABLE KEYS */;
/*!40000 ALTER TABLE `inventory_adjust_main` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:18
