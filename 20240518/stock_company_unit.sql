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
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='醫院院區關係資料';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `company_unit`
--

LOCK TABLES `company_unit` WRITE;
/*!40000 ALTER TABLE `company_unit` DISABLE KEYS */;
INSERT INTO `company_unit` VALUES (1,'KimForest','金萬林','63c0dbc4-fdca-47e8-b084-5880fb596de2','金萬林','2024-04-22 05:04:13','2024-04-22 05:04:13'),(2,'Nantou-unit','南投醫院院區','894364a3-4a13-4f15-9d9c-f455dad30094','檢驗科','2024-04-22 05:04:13','2024-04-22 05:04:13'),(3,'Caotun-unit','草屯療養院院區','49b98d1b-6df5-4d1d-9250-ba82b0fe2fa0','檢驗科','2024-04-22 05:04:13','2024-04-22 05:04:13'),(4,'Nantou-unit','南投醫院院區','12bd040e-fda2-4e8e-af7d-66438a9cc771','外科','2024-04-22 05:04:13','2024-04-22 05:04:13'),(5,'Nantou-unit','南投醫院院區','0838569e-d00c-457e-ba95-54e00ec61cb5','婦產科','2024-04-22 05:04:13','2024-04-22 05:04:13'),(6,'Nantou-unit','南投醫院院區','f9edcb14-e68b-4f13-8d45-09bf8ac24e59','內科','2024-04-22 05:04:13','2024-04-22 05:04:13'),(7,'Nantou-unit','南投醫院院區','675dd6dc-0be2-4fa6-8740-58d26b8016a7','洗腎室','2024-04-22 05:04:13','2024-04-22 05:04:13'),(8,'Nantou-unit','南投醫院院區','9ab52f4a-9929-4778-ab16-7a3c8d77c90c','開刀房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(9,'Nantou-unit','南投醫院院區','c27e4287-6fa4-4595-ab9b-169de41fe09c','急診','2024-04-22 05:04:13','2024-04-22 05:04:13'),(10,'Nantou-unit','南投醫院院區','5931f510-a1d7-4825-a581-322a25e30d97','21W病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(11,'Nantou-unit','南投醫院院區','60a98281-6689-4bd7-84eb-525dc59f78c6','31W病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(12,'Nantou-unit','南投醫院院區','b5674454-60f1-4d63-9140-38084376e089','32W病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(13,'Nantou-unit','南投醫院院區','4fd02b90-db89-4c16-a4b8-532ee999c256','3樓ICU病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(14,'Nantou-unit','南投醫院院區','051feb29-84f4-450b-b8bc-81e506c1e104','51W病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(15,'Nantou-unit','南投醫院院區','29501aa7-e278-46f5-a649-fb00913dd9b2','52W病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(16,'Nantou-unit','南投醫院院區','525cdeb4-dd59-411f-bb52-5df63a8afb22','呼吸照護病房(RCW)','2024-04-22 05:04:13','2024-04-22 05:04:13'),(17,'Nantou-unit','南投醫院院區','b9066a59-6d3a-4afc-8e39-c1fb42ae4888','5樓ICU病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(18,'Nantou-unit','南投醫院院區','47a82751-da83-42dc-8519-56365825ce03','61W病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(19,'Nantou-unit','南投醫院院區','04f64416-4b0d-4ff5-b7fe-44d9283e93cd','6樓ICU病房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(20,'Changhua-unit','彰化醫院院區','68c05e1d-c6f5-498a-8359-d961359e5875','檢驗科','2024-04-22 05:04:13','2024-04-22 05:04:13'),(21,'Changhua-unit','彰化醫院院區','e0abc025-f2e7-4278-8107-c415d150643d','健康管理中心','2024-04-22 05:04:13','2024-04-22 05:04:13'),(22,'Changhua-unit','彰化醫院院區','99234bab-1674-4b55-b890-d5ad1879f8a8','開刀房','2024-04-22 05:04:13','2024-04-22 05:04:13'),(23,'Changhua-unit','彰化醫院院區','ec31f714-25fc-4288-9b6b-ec0f9885fc99','麻醉科','2024-04-22 05:04:14','2024-04-22 05:04:14'),(24,'Changhua-unit','彰化醫院院區','85172964-61da-43f5-a822-898c387b100e','6C病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(25,'Changhua-unit','彰化醫院院區','ed007671-7ce8-44b1-a975-51992fa892ed','6B病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(26,'Changhua-unit','彰化醫院院區','9744b1b3-c974-4f58-9737-a683b4da92a1','6A病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(27,'Changhua-unit','彰化醫院院區','d16f8149-4a39-4de7-9c29-869bb85b2080','5B病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(28,'Changhua-unit','彰化醫院院區','0401357f-1efa-4cc1-8464-c99b37007e69','5A病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(29,'Changhua-unit','彰化醫院院區','848acd80-a39d-4754-803a-3f1f3ac3f8e5','3C病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(30,'Changhua-unit','彰化醫院院區','4d2bcd0a-14f5-4970-ab9d-d33f3b1c4b9e','7C病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(31,'Changhua-unit','彰化醫院院區','fa17486a-e545-4327-80e7-d1d48effe82b','8C病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(32,'Changhua-unit','彰化醫院院區','fe69334c-6244-4df0-84fb-83df2dfef30a','急診室','2024-04-22 05:04:14','2024-04-22 05:04:14'),(33,'Changhua-unit','彰化醫院院區','40d85630-e124-4c85-bf5b-c34c8b52ee1d','洗腎室','2024-04-22 05:04:14','2024-04-22 05:04:14'),(34,'Changhua-unit','彰化醫院院區','382a11ad-5b14-47ae-a228-bbb9486281a4','呼吸照護病房(RCW)','2024-04-22 05:04:14','2024-04-22 05:04:14'),(35,'Changhua-unit','彰化醫院院區','a406712e-d538-4a47-ba4b-33682e4dde91','呼吸照護中心(RCC)','2024-04-22 05:04:14','2024-04-22 05:04:14'),(36,'Changhua-unit','彰化醫院院區','c71faa4e-d872-4eb8-8860-9648b4bad41a','2樓ICU','2024-04-22 05:04:14','2024-04-22 05:04:14'),(37,'Changhua-unit','彰化醫院院區','ebc59f28-eae7-44ff-bba2-ee9ac993e701','1樓ICU','2024-04-22 05:04:14','2024-04-22 05:04:14'),(38,'Changhua-unit','彰化醫院院區','125833ba-43d1-4b97-b561-78f72a8ba521','5C病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(39,'Changhua-unit','彰化醫院院區','68c3aaac-ca31-4aa4-8f2c-a38e6cd11fcb','身心整合病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(40,'Changhua-unit','彰化醫院院區','4fd956d8-c876-4f50-8526-1e38d1c1cb92','8A病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(41,'Changhua-unit','彰化醫院院區','e0d9bddf-e7c2-4716-9ad2-4661f9ddfd7f','耳鼻喉科','2024-04-22 05:04:14','2024-04-22 05:04:14'),(42,'Changhua-unit','彰化醫院院區','e16fbcfa-eb87-41c4-81e2-a442c229ce9b','胃鏡室','2024-04-22 05:04:14','2024-04-22 05:04:14'),(43,'Changhua-unit','彰化醫院院區','d3847bc5-65c9-4c9c-9948-d7f80ee5f296','放射科','2024-04-22 05:04:14','2024-04-22 05:04:14'),(44,'Changhua-unit','彰化醫院院區','e67b64e3-28cd-491f-91a7-cb98edafb6a1','牙科','2024-04-22 05:04:14','2024-04-22 05:04:14'),(45,'Changhua-unit','彰化醫院院區','82a9e120-2f41-4306-b59b-cfb0995e26cf','肺功能室','2024-04-22 05:04:14','2024-04-22 05:04:14'),(46,'Changhua-unit','彰化醫院院區','177f0bbc-623e-460a-960a-0a2054c070a8','7B病房','2024-04-22 05:04:14','2024-04-22 05:04:14'),(47,'Changhua-unit','彰化醫院院區','431b1c87-f1b2-4ff3-98f2-0c6bb29f4d75','公關室','2024-04-22 05:04:14','2024-04-22 05:04:14'),(48,'Changhua-unit','彰化醫院院區','63c0dbc4-fdca-47e8-b084-5880fb596de0','病房A','2024-04-22 14:45:40','2024-04-22 14:45:40');
/*!40000 ALTER TABLE `company_unit` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:15
