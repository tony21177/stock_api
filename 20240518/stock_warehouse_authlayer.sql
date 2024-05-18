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
-- Table structure for table `warehouse_authlayer`
--

DROP TABLE IF EXISTS `warehouse_authlayer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `warehouse_authlayer` (
  `AuthId` int NOT NULL AUTO_INCREMENT,
  `AuthDescription` varchar(100) DEFAULT NULL COMMENT '權限描述',
  `AuthName` varchar(100) NOT NULL COMMENT '權限名稱',
  `AuthValue` smallint NOT NULL COMMENT '權限值\n1: 得標廠商\n3: 最高層級\n5: 第一層級\n7: 第二層級\n9: 第三層級',
  `IsApplyItemManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以申請新增品項',
  `IsGroupManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行群組管理',
  `IsInBoundManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行入庫作業',
  `IsOutBoundManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行出庫作業',
  `IsInventoryManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行庫存管理',
  `IsItemManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行品項管理',
  `IsMemberManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行成員管理',
  `IsRestockManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行盤點',
  `IsVerifyManage` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否可以進行抽點',
  `CompId` varchar(100) NOT NULL COMMENT '屬於庫存系統裡面的哪一個公司內所有\\\\n對應 -> Company Table"',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`AuthId`)
) ENGINE=InnoDB AUTO_INCREMENT=103 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='主要是用來設定登入人員的權限。\n一般組之內，設定人員權限時，僅有 3, 5, 7, 9 的 AuthValue 選項。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `warehouse_authlayer`
--

LOCK TABLES `warehouse_authlayer` WRITE;
/*!40000 ALTER TABLE `warehouse_authlayer` DISABLE KEYS */;
INSERT INTO `warehouse_authlayer` VALUES (1,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'63c0dbc4-fdca-47e8-b084-5880fb596de2','2024-05-06 13:25:39','2024-05-06 13:25:39'),(2,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'894364a3-4a13-4f15-9d9c-f455dad30094','2024-05-06 13:25:39','2024-05-06 13:25:39'),(3,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'894364a3-4a13-4f15-9d9c-f455dad30094','2024-05-06 13:25:39','2024-05-06 13:25:39'),(4,'適用管理階層','第二層級',5,1,1,1,1,1,0,0,1,1,'894364a3-4a13-4f15-9d9c-f455dad30094','2024-05-06 13:25:39','2024-05-06 13:25:39'),(5,'適用一般醫檢師','第三層級',7,1,0,1,1,1,0,0,0,0,'894364a3-4a13-4f15-9d9c-f455dad30094','2024-05-06 13:25:39','2024-05-06 13:25:39'),(6,'適用行政人員','第四層級',9,0,0,1,1,0,0,0,0,0,'894364a3-4a13-4f15-9d9c-f455dad30094','2024-05-06 13:25:39','2024-05-06 13:25:39'),(7,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'49b98d1b-6df5-4d1d-9250-ba82b0fe2fa0','2024-05-06 13:25:39','2024-05-06 13:25:39'),(8,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'49b98d1b-6df5-4d1d-9250-ba82b0fe2fa0','2024-05-06 13:25:39','2024-05-06 13:25:39'),(9,'適用管理階層','第二層級',5,1,1,1,1,1,0,0,1,1,'49b98d1b-6df5-4d1d-9250-ba82b0fe2fa0','2024-05-06 13:25:39','2024-05-06 13:25:39'),(10,'適用一般醫檢師','第三層級',7,1,0,1,1,1,0,0,0,0,'49b98d1b-6df5-4d1d-9250-ba82b0fe2fa0','2024-05-06 13:25:39','2024-05-06 13:25:39'),(11,'適用行政人員','第四層級',9,0,0,1,1,0,0,0,0,0,'49b98d1b-6df5-4d1d-9250-ba82b0fe2fa0','2024-05-06 13:25:39','2024-05-06 13:25:39'),(12,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'12bd040e-fda2-4e8e-af7d-66438a9cc771','2024-05-06 13:25:39','2024-05-06 13:25:39'),(13,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'12bd040e-fda2-4e8e-af7d-66438a9cc771','2024-05-06 13:25:39','2024-05-06 13:25:39'),(14,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'0838569e-d00c-457e-ba95-54e00ec61cb5','2024-05-06 13:25:39','2024-05-06 13:25:39'),(15,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'0838569e-d00c-457e-ba95-54e00ec61cb5','2024-05-06 13:25:39','2024-05-06 13:25:39'),(16,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'f9edcb14-e68b-4f13-8d45-09bf8ac24e59','2024-05-06 13:25:39','2024-05-06 13:25:39'),(17,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'f9edcb14-e68b-4f13-8d45-09bf8ac24e59','2024-05-06 13:25:39','2024-05-06 13:25:39'),(18,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'675dd6dc-0be2-4fa6-8740-58d26b8016a7','2024-05-06 13:25:39','2024-05-06 13:25:39'),(19,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'675dd6dc-0be2-4fa6-8740-58d26b8016a7','2024-05-06 13:25:39','2024-05-06 13:25:39'),(20,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'9ab52f4a-9929-4778-ab16-7a3c8d77c90c','2024-05-06 13:25:39','2024-05-06 13:25:39'),(21,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'9ab52f4a-9929-4778-ab16-7a3c8d77c90c','2024-05-06 13:25:39','2024-05-06 13:25:39'),(22,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'c27e4287-6fa4-4595-ab9b-169de41fe09c','2024-05-06 13:25:39','2024-05-06 13:25:39'),(23,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'c27e4287-6fa4-4595-ab9b-169de41fe09c','2024-05-06 13:25:39','2024-05-06 13:25:39'),(24,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'5931f510-a1d7-4825-a581-322a25e30d97','2024-05-06 13:25:39','2024-05-06 13:25:39'),(25,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'5931f510-a1d7-4825-a581-322a25e30d97','2024-05-06 13:25:39','2024-05-06 13:25:39'),(26,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'60a98281-6689-4bd7-84eb-525dc59f78c6','2024-05-06 13:25:39','2024-05-06 13:25:39'),(27,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'60a98281-6689-4bd7-84eb-525dc59f78c6','2024-05-06 13:25:39','2024-05-06 13:25:39'),(28,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'b5674454-60f1-4d63-9140-38084376e089','2024-05-06 13:25:39','2024-05-06 13:25:39'),(29,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'b5674454-60f1-4d63-9140-38084376e089','2024-05-06 13:25:39','2024-05-06 13:25:39'),(30,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'4fd02b90-db89-4c16-a4b8-532ee999c256','2024-05-06 13:25:39','2024-05-06 13:25:39'),(31,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'4fd02b90-db89-4c16-a4b8-532ee999c256','2024-05-06 13:25:39','2024-05-06 13:25:39'),(32,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'051feb29-84f4-450b-b8bc-81e506c1e104','2024-05-06 13:25:39','2024-05-06 13:25:39'),(33,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'051feb29-84f4-450b-b8bc-81e506c1e104','2024-05-06 13:25:39','2024-05-06 13:25:39'),(34,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'29501aa7-e278-46f5-a649-fb00913dd9b2','2024-05-06 13:25:39','2024-05-06 13:25:39'),(35,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'29501aa7-e278-46f5-a649-fb00913dd9b2','2024-05-06 13:25:39','2024-05-06 13:25:39'),(36,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'525cdeb4-dd59-411f-bb52-5df63a8afb22','2024-05-06 13:25:39','2024-05-06 13:25:39'),(37,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'525cdeb4-dd59-411f-bb52-5df63a8afb22','2024-05-06 13:25:39','2024-05-06 13:25:39'),(38,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'b9066a59-6d3a-4afc-8e39-c1fb42ae4888','2024-05-06 13:25:39','2024-05-06 13:25:39'),(39,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'b9066a59-6d3a-4afc-8e39-c1fb42ae4888','2024-05-06 13:25:39','2024-05-06 13:25:39'),(40,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'47a82751-da83-42dc-8519-56365825ce03','2024-05-06 13:25:39','2024-05-06 13:25:39'),(41,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'47a82751-da83-42dc-8519-56365825ce03','2024-05-06 13:25:39','2024-05-06 13:25:39'),(42,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'04f64416-4b0d-4ff5-b7fe-44d9283e93cd','2024-05-06 13:25:39','2024-05-06 13:25:39'),(43,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'04f64416-4b0d-4ff5-b7fe-44d9283e93cd','2024-05-06 13:25:39','2024-05-06 13:25:39'),(44,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'68c05e1d-c6f5-498a-8359-d961359e5875','2024-05-06 13:25:39','2024-05-06 13:25:39'),(45,'適用部門主管','第一層級',3,1,1,1,1,1,1,1,1,1,'68c05e1d-c6f5-498a-8359-d961359e5875','2024-05-06 13:25:39','2024-05-15 11:37:32'),(46,'適用管理階層','第二層級',5,1,1,1,1,1,0,0,1,1,'68c05e1d-c6f5-498a-8359-d961359e5875','2024-05-06 13:25:39','2024-05-06 13:25:39'),(47,'適用一般醫檢師','第三層級',7,1,0,1,1,1,0,0,0,0,'68c05e1d-c6f5-498a-8359-d961359e5875','2024-05-06 13:25:39','2024-05-06 13:25:39'),(48,'適用行政人員','第四層級',9,0,0,1,1,0,0,0,0,0,'68c05e1d-c6f5-498a-8359-d961359e5875','2024-05-06 13:25:39','2024-05-06 13:25:39'),(49,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'e0abc025-f2e7-4278-8107-c415d150643d','2024-05-06 13:25:39','2024-05-06 13:25:39'),(50,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'e0abc025-f2e7-4278-8107-c415d150643d','2024-05-06 13:25:39','2024-05-06 13:25:39'),(51,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'99234bab-1674-4b55-b890-d5ad1879f8a8','2024-05-06 13:25:39','2024-05-06 13:25:39'),(52,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'99234bab-1674-4b55-b890-d5ad1879f8a8','2024-05-06 13:25:39','2024-05-06 13:25:39'),(53,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'ec31f714-25fc-4288-9b6b-ec0f9885fc99','2024-05-06 13:25:39','2024-05-06 13:25:39'),(54,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'ec31f714-25fc-4288-9b6b-ec0f9885fc99','2024-05-06 13:25:39','2024-05-06 13:25:39'),(55,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'85172964-61da-43f5-a822-898c387b100e','2024-05-06 13:25:39','2024-05-06 13:25:39'),(56,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'85172964-61da-43f5-a822-898c387b100e','2024-05-06 13:25:39','2024-05-06 13:25:39'),(57,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'ed007671-7ce8-44b1-a975-51992fa892ed','2024-05-06 13:25:39','2024-05-06 13:25:39'),(58,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'ed007671-7ce8-44b1-a975-51992fa892ed','2024-05-06 13:25:39','2024-05-06 13:25:39'),(59,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'9744b1b3-c974-4f58-9737-a683b4da92a1','2024-05-06 13:25:39','2024-05-06 13:25:39'),(60,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'9744b1b3-c974-4f58-9737-a683b4da92a1','2024-05-06 13:25:39','2024-05-06 13:25:39'),(61,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'d16f8149-4a39-4de7-9c29-869bb85b2080','2024-05-06 13:25:39','2024-05-06 13:25:39'),(62,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'d16f8149-4a39-4de7-9c29-869bb85b2080','2024-05-06 13:25:39','2024-05-06 13:25:39'),(63,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'0401357f-1efa-4cc1-8464-c99b37007e69','2024-05-06 13:25:39','2024-05-06 13:25:39'),(64,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'0401357f-1efa-4cc1-8464-c99b37007e69','2024-05-06 13:25:39','2024-05-06 13:25:39'),(65,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'848acd80-a39d-4754-803a-3f1f3ac3f8e5','2024-05-06 13:25:39','2024-05-06 13:25:39'),(66,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'848acd80-a39d-4754-803a-3f1f3ac3f8e5','2024-05-06 13:25:39','2024-05-06 13:25:39'),(67,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'4d2bcd0a-14f5-4970-ab9d-d33f3b1c4b9e','2024-05-06 13:25:40','2024-05-06 13:25:40'),(68,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'4d2bcd0a-14f5-4970-ab9d-d33f3b1c4b9e','2024-05-06 13:25:40','2024-05-06 13:25:40'),(69,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'fa17486a-e545-4327-80e7-d1d48effe82b','2024-05-06 13:25:40','2024-05-06 13:25:40'),(70,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'fa17486a-e545-4327-80e7-d1d48effe82b','2024-05-06 13:25:40','2024-05-06 13:25:40'),(71,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'fe69334c-6244-4df0-84fb-83df2dfef30a','2024-05-06 13:25:40','2024-05-06 13:25:40'),(72,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'fe69334c-6244-4df0-84fb-83df2dfef30a','2024-05-06 13:25:40','2024-05-06 13:25:40'),(73,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'40d85630-e124-4c85-bf5b-c34c8b52ee1d','2024-05-06 13:25:40','2024-05-06 13:25:40'),(74,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'40d85630-e124-4c85-bf5b-c34c8b52ee1d','2024-05-06 13:25:40','2024-05-06 13:25:40'),(75,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'382a11ad-5b14-47ae-a228-bbb9486281a4','2024-05-06 13:25:40','2024-05-06 13:25:40'),(76,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'382a11ad-5b14-47ae-a228-bbb9486281a4','2024-05-06 13:25:40','2024-05-06 13:25:40'),(77,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'a406712e-d538-4a47-ba4b-33682e4dde91','2024-05-06 13:25:40','2024-05-06 13:25:40'),(78,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'a406712e-d538-4a47-ba4b-33682e4dde91','2024-05-06 13:25:40','2024-05-06 13:25:40'),(79,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'c71faa4e-d872-4eb8-8860-9648b4bad41a','2024-05-06 13:25:40','2024-05-06 13:25:40'),(80,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'c71faa4e-d872-4eb8-8860-9648b4bad41a','2024-05-06 13:25:40','2024-05-06 13:25:40'),(81,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'ebc59f28-eae7-44ff-bba2-ee9ac993e701','2024-05-06 13:25:40','2024-05-06 13:25:40'),(82,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'ebc59f28-eae7-44ff-bba2-ee9ac993e701','2024-05-06 13:25:40','2024-05-06 13:25:40'),(83,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'125833ba-43d1-4b97-b561-78f72a8ba521','2024-05-06 13:25:40','2024-05-06 13:25:40'),(84,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'125833ba-43d1-4b97-b561-78f72a8ba521','2024-05-06 13:25:40','2024-05-06 13:25:40'),(85,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'68c3aaac-ca31-4aa4-8f2c-a38e6cd11fcb','2024-05-06 13:25:40','2024-05-06 13:25:40'),(86,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'68c3aaac-ca31-4aa4-8f2c-a38e6cd11fcb','2024-05-06 13:25:40','2024-05-06 13:25:40'),(87,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'4fd956d8-c876-4f50-8526-1e38d1c1cb92','2024-05-06 13:25:40','2024-05-06 13:25:40'),(88,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'4fd956d8-c876-4f50-8526-1e38d1c1cb92','2024-05-06 13:25:40','2024-05-06 13:25:40'),(89,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'e0d9bddf-e7c2-4716-9ad2-4661f9ddfd7f','2024-05-06 13:25:40','2024-05-06 13:25:40'),(90,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'e0d9bddf-e7c2-4716-9ad2-4661f9ddfd7f','2024-05-06 13:25:40','2024-05-06 13:25:40'),(91,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'e16fbcfa-eb87-41c4-81e2-a442c229ce9b','2024-05-06 13:25:40','2024-05-06 13:25:40'),(92,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'e16fbcfa-eb87-41c4-81e2-a442c229ce9b','2024-05-06 13:25:40','2024-05-06 13:25:40'),(93,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'d3847bc5-65c9-4c9c-9948-d7f80ee5f296','2024-05-06 13:25:40','2024-05-06 13:25:40'),(94,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'d3847bc5-65c9-4c9c-9948-d7f80ee5f296','2024-05-06 13:25:40','2024-05-06 13:25:40'),(95,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'e67b64e3-28cd-491f-91a7-cb98edafb6a1','2024-05-06 13:25:40','2024-05-06 13:25:40'),(96,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'e67b64e3-28cd-491f-91a7-cb98edafb6a1','2024-05-06 13:25:40','2024-05-06 13:25:40'),(97,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'82a9e120-2f41-4306-b59b-cfb0995e26cf','2024-05-06 13:25:40','2024-05-06 13:25:40'),(98,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'82a9e120-2f41-4306-b59b-cfb0995e26cf','2024-05-06 13:25:40','2024-05-06 13:25:40'),(99,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'177f0bbc-623e-460a-960a-0a2054c070a8','2024-05-06 13:25:40','2024-05-06 13:25:40'),(100,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'177f0bbc-623e-460a-960a-0a2054c070a8','2024-05-06 13:25:40','2024-05-06 13:25:40'),(101,'適用得標廠商','得標廠商',1,1,1,1,1,1,1,1,1,1,'431b1c87-f1b2-4ff3-98f2-0c6bb29f4d75','2024-05-06 13:25:40','2024-05-06 13:25:40'),(102,'適用部門主管','第一層級',3,1,1,1,1,1,0,1,1,1,'431b1c87-f1b2-4ff3-98f2-0c6bb29f4d75','2024-05-06 13:25:40','2024-05-06 13:25:40');
/*!40000 ALTER TABLE `warehouse_authlayer` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:04
