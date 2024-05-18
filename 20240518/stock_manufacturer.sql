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
-- Dumping data for table `manufacturer`
--

LOCK TABLES `manufacturer` WRITE;
/*!40000 ALTER TABLE `manufacturer` DISABLE KEYS */;
INSERT INTO `manufacturer` VALUES ('0c5c0fe7-a0f9-48a6-a757-7b0500e34af8','BEC2','beckmen2','beckmen2',0,'2024-03-28 15:02:22','2024-03-28 15:08:31'),('39bfead1-c049-4d14-a787-47c4d2b7b953','BEC3','beckmen3','beckmen3',1,'2024-04-03 04:40:44','2024-04-03 06:16:59'),('3bed5d53-5ec3-4ab2-946f-b2d454d0fadd','M102','製造商2','備註2',1,'2024-04-03 06:48:52','2024-04-03 06:48:59'),('675502f6-e746-4a8d-b9d5-4701a833892b','M103','製造商3','備註3',1,'2024-04-03 06:49:16','2024-04-03 06:49:16'),('e8165e4f-5d2c-4898-8065-d5d5489dff50','M101','永裕製造商','這是製造商備註內容',1,'2024-04-03 06:46:24','2024-04-03 06:46:24');
/*!40000 ALTER TABLE `manufacturer` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:12
