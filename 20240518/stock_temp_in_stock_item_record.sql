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
-- Table structure for table `temp_in_stock_item_record`
--

DROP TABLE IF EXISTS `temp_in_stock_item_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `temp_in_stock_item_record` (
  `InStockId` varchar(100) NOT NULL,
  `LotNumberBatch` varchar(100) NOT NULL COMMENT '批次',
  `LotNumber` varchar(100) NOT NULL COMMENT '批號',
  `CompId` varchar(100) NOT NULL COMMENT '所屬公司ID',
  `OriginalQuantity` int NOT NULL COMMENT '現有庫存量',
  `ExpirationDate` date DEFAULT NULL COMMENT '保存期限',
  `InStockQuantity` int NOT NULL COMMENT '此次入庫數量',
  `ProductId` varchar(100) NOT NULL COMMENT '品項PK',
  `ProductCode` varchar(200) NOT NULL,
  `ProductName` varchar(200) NOT NULL COMMENT '品項名稱',
  `ProductSpec` varchar(300) NOT NULL COMMENT '品項規格',
  `Type` varchar(45) NOT NULL COMMENT '類型\nPURCHASE : 來源是採購\nSHIFT : 調撥\nADJUST : 調整（盤盈）\nRETURN : 退庫',
  `UserId` varchar(100) NOT NULL COMMENT '執行入庫人員的UserID',
  `UserName` varchar(100) NOT NULL COMMENT '執行入庫人員的UserName',
  `IsTransfer` tinyint(1) NOT NULL COMMENT '用來判斷暫存項目是不是已經有複製過去 InStockItemRecord,(0)false: 未複製,(1)true: 已複製',
  `InventoryID` varchar(100) NOT NULL COMMENT '用來判斷屬於哪一張主單',
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `DeliverFunction` varchar(45) DEFAULT NULL,
  `DeliverTemperature` double DEFAULT NULL,
  `SavingFunction` varchar(45) DEFAULT NULL,
  `SavingTemperature` double DEFAULT NULL,
  PRIMARY KEY (`InStockId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='盤點和調撥單據，如果是要入庫的話，會先寫入一筆資料到這個暫時的表，等到審核通過再複製到 InStockItemRecord.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `temp_in_stock_item_record`
--

LOCK TABLES `temp_in_stock_item_record` WRITE;
/*!40000 ALTER TABLE `temp_in_stock_item_record` DISABLE KEYS */;
INSERT INTO `temp_in_stock_item_record` VALUES ('0032ced5-9581-4e21-9751-a40e19410fc9','12345_2024515181938','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',6,'2025-12-31',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-091','091','1000ul 藍色吸管尖','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'9dc98742-4b0d-45d8-a39f-aa2eaf83f2d0','2024-05-15 10:19:38','2024-05-15 10:19:38',NULL,NULL,NULL,NULL),('04b1e550-42e8-4ab9-a14b-18f65c215fb0','123_2024510133618','123','63c0dbc4-fdca-47e8-b084-5880fb596de2',239,'2025-01-01',4,'63c0dbc4-fdca-47e8-b084-5880fb596de2-239','239','PBS Buffer pH 6.8','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'136c6596-2b96-492f-b130-adbaad6abc57','2024-05-10 05:36:18','2024-05-10 05:36:18',NULL,NULL,NULL,NULL),('04b1fb05-d6c6-4542-9116-a0fdae40976f','BBBBB_2024515105424','BBBBB','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-05-15',35,'0401357f-1efa-4cc1-8464-c99b37007e69-050','050','螢光含樹脂需氧塑膠培養瓶(灰)-442023','瓶','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'b85783ec-cc0f-4251-81fc-0ea30929b2b0','2024-05-15 02:54:22','2024-05-15 02:54:22','OTHER',15,'OTHER',15),('0576894f-c0c2-4c7e-a2df-d7b7ef1c065a','AAAAA_202454','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',200,'2024-12-31',100,'0401357f-1efa-4cc1-8464-c99b37007e69-046','','Amies w/Charcoal Swabs厭氧細菌拭子','支','PURCHASE','778909c2-734c-4d10-a54a-9d0c2257c8ac','garyTest1',1,'535e6aad-bd2f-4c18-ba3d-f70d0ba2bc89','2024-05-04 10:33:34','2024-05-04 10:33:34',NULL,NULL,NULL,NULL),('07bfc363-4802-4481-b8b4-049329910b07','12345_2024514175329','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2024-05-30',20,'63c0dbc4-fdca-47e8-b084-5880fb596de2-243','243','7H11 Argar Plate 結核桿菌培養基','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'917aaffd-4333-4c7c-8344-cd62505dd465','2024-05-14 09:53:29','2024-05-14 09:53:29',NULL,NULL,NULL,NULL),('0adab747-66f3-4518-9309-0193dce70c6d','12345_2024515174418','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',20,'63c0dbc4-fdca-47e8-b084-5880fb596de2-106','106','葡萄糖粉-75g','袋','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3db51da5-c9ff-474b-b592-6828b3250ae8','2024-05-15 09:44:18','2024-05-15 09:44:18',NULL,NULL,NULL,NULL),('0d9da008-3195-48b7-98a2-a0bb41aef182','AAAAA_2024511142914','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-05-14',20,'0401357f-1efa-4cc1-8464-c99b37007e69-050','050','螢光含樹脂需氧塑膠培養瓶(灰)-442023','瓶','PURCHASE','778909c2-734c-4d10-a54a-9d0c2257c8ac','garyTest1',1,'a7002eb2-c2ae-4e3e-83a8-1b7f80ba0deb','2024-05-11 06:29:15','2024-05-11 06:29:15',NULL,NULL,NULL,NULL),('10a506ca-a2f9-47f7-88df-4ed53410331c','123456_2024510154311','123456','68c05e1d-c6f5-498a-8359-d961359e5875',0,'2025-05-31',2,'68c05e1d-c6f5-498a-8359-d961359e5875-299','299','酸性酒精溶液Acid Alcohol Solution','組','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'e1e77de1-1174-4ee6-a80d-f1c9e0ccc299','2024-05-10 07:43:11','2024-05-10 07:43:11',NULL,NULL,NULL,NULL),('11595c13-79ff-45d7-b3c0-9a6c44fb7857','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',7,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'3afa5ea9-2a20-4bfc-a402-5918f54f7dab','2024-05-07 16:43:57','2024-05-07 16:43:57',NULL,NULL,NULL,NULL),('1516134d-d3ec-4564-9b09-e6dc800f027b','12345_2024515174325','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',10,'2025-12-31',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-105','105','葡萄糖粉-50g','袋','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'c4e1147c-4eca-4470-9679-30f3237f602c','2024-05-15 09:43:26','2024-05-15 09:43:26',NULL,NULL,NULL,NULL),('15da5a02-612d-4838-946b-797e0718f32b','3214403_202457','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',252,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'e251091a-cb10-4ed9-a531-a0f0e1361cca','2024-05-07 03:44:04','2024-05-07 03:44:04',NULL,NULL,NULL,NULL),('165fbb8d-52bb-4045-812f-ee966faca117','_2024515183557','','63c0dbc4-fdca-47e8-b084-5880fb596de2',3,'2025-12-31',3,'63c0dbc4-fdca-47e8-b084-5880fb596de2-198','198','1.7ml微量離心管','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'e3375dcb-154f-43da-91ec-c44d9bbd6aef','2024-05-15 10:35:57','2024-05-15 10:35:57',NULL,NULL,NULL,NULL),('16fb485a-117a-45cc-b007-a9c6062e361d','123456_2024510133821','123456','63c0dbc4-fdca-47e8-b084-5880fb596de2',242,'2025-10-10',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-242','242','蒸氣滅菌指示劑含培養基(孢子) 10^6 ATCC 7953','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'136c6596-2b96-492f-b130-adbaad6abc57','2024-05-10 05:38:21','2024-05-10 05:38:21',NULL,NULL,NULL,NULL),('1e3c7d4b-82c6-4e78-bf0b-ac5fdbfd33bd','443321_202451104034','443321','0401357f-1efa-4cc1-8464-c99b37007e69',15,'2024-05-11',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'6b482797-436a-46ec-92bb-471b569459a5','2024-05-10 16:40:34','2024-05-10 16:40:34',NULL,NULL,NULL,NULL),('2319198d-a10e-430c-bcd4-0e940e3ad7e1','3214403_202451416178','3214403','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-01-31',25,'63c0dbc4-fdca-47e8-b084-5880fb596de2-146','146','ANTI-B  Gamma-clone','瓶','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'80adb359-a9fc-4fe5-a9bc-f791c874042a','2024-05-14 08:17:08','2024-05-14 08:17:08',NULL,NULL,NULL,NULL),('23be23d3-4f36-4157-896a-6c5bffa2e8d8','12345_2024515154358','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',4,'63c0dbc4-fdca-47e8-b084-5880fb596de2-040','040','張氏糞便濃縮集卵瓶 (無配件)( 支)','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3be0128f-4d3e-4f26-ab3f-4e5b5dd7c9c2','2024-05-15 07:43:58','2024-05-15 07:43:58',NULL,NULL,NULL,NULL),('2557c812-1c5d-4d57-9386-94b89e865ecb','54321_202451103019','54321','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-05-11',12,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'ed910e26-2b88-42fa-832b-64bb19795b58','2024-05-10 16:30:19','2024-05-10 16:30:19',NULL,NULL,NULL,NULL),('2a00e5c1-9d61-40eb-b6c6-79417f3fe6c0','12345_2024515181057','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',3,'63c0dbc4-fdca-47e8-b084-5880fb596de2-198','198','1.7ml微量離心管','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'231b4064-7895-4461-995d-910348127c93','2024-05-15 10:10:57','2024-05-15 10:10:57',NULL,NULL,NULL,NULL),('2e013b9d-a531-4d7b-8f60-269bed731abc','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',6,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'2d62ecb4-69ca-4043-9504-43d14e95a32f','2024-05-07 16:41:20','2024-05-07 16:41:20',NULL,NULL,NULL,NULL),('2e835b20-a08b-4667-ba37-2ea21d8690fd','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',5,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'109a1030-dd96-4d05-bd9f-b8e47f21e054','2024-05-07 16:38:16','2024-05-07 16:38:16',NULL,NULL,NULL,NULL),('2ec41058-7d4c-4ed9-9c28-484ab7bbca14','AAAAA_202454','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',100,'2024-12-31',100,'0401357f-1efa-4cc1-8464-c99b37007e69-046','','Amies w/Charcoal Swabs厭氧細菌拭子','支','PURCHASE','778909c2-734c-4d10-a54a-9d0c2257c8ac','garyTest1',1,'535e6aad-bd2f-4c18-ba3d-f70d0ba2bc89','2024-05-04 09:54:42','2024-05-04 09:54:42',NULL,NULL,NULL,NULL),('2f064375-1988-490d-a27b-3ecf7488d352','12345_2024515174128','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',10,'63c0dbc4-fdca-47e8-b084-5880fb596de2-105','105','葡萄糖粉-50g','袋','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'5548b93d-e8d4-4fc3-a687-b9c678e8815c','2024-05-15 09:41:28','2024-05-15 09:41:28',NULL,NULL,NULL,NULL),('348bbcdf-c85f-4e59-97de-af5722c00c44','54321_2024515163254','54321','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',8,'63c0dbc4-fdca-47e8-b084-5880fb596de2-133','133','MIF染色固定液','組','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3be0128f-4d3e-4f26-ab3f-4e5b5dd7c9c2','2024-05-15 08:32:54','2024-05-15 08:32:54',NULL,NULL,NULL,NULL),('34c729e9-bf72-4a49-93f6-93d12e6e25be','12345_202451114337','12345','0401357f-1efa-4cc1-8464-c99b37007e69',117,'2024-05-11',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'4fc39826-f723-47bf-8933-1d2fa9b40eeb','2024-05-11 06:03:37','2024-05-11 06:03:37',NULL,NULL,NULL,NULL),('38153d7b-4b08-4626-835e-8f6bca403d3b','BBBBB_2024511143156','BBBBB','0401357f-1efa-4cc1-8464-c99b37007e69',20,'2024-05-14',25,'0401357f-1efa-4cc1-8464-c99b37007e69-050','050','螢光含樹脂需氧塑膠培養瓶(灰)-442023','瓶','PURCHASE','778909c2-734c-4d10-a54a-9d0c2257c8ac','garyTest1',1,'b4ace5d2-bd60-4162-b875-edcae59b3811','2024-05-11 06:31:57','2024-05-11 06:31:57',NULL,NULL,NULL,NULL),('394dec9a-98ce-468c-887f-1ee2396c3c73','987554_202451015223','987554','63c0dbc4-fdca-47e8-b084-5880fb596de2',237,'2025-10-10',9,'63c0dbc4-fdca-47e8-b084-5880fb596de2-237','237','NALC 1.25(N-Acetyl-L-Cysteine) 乙醯基半胱胺酸','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'136c6596-2b96-492f-b130-adbaad6abc57','2024-05-10 07:02:23','2024-05-10 07:02:23',NULL,NULL,NULL,NULL),('3aa573db-59a1-477f-871b-aed0a1c87761','AAAAA_202454','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',400,'2024-12-31',100,'0401357f-1efa-4cc1-8464-c99b37007e69-046','','Amies w/Charcoal Swabs厭氧細菌拭子','支','PURCHASE','778909c2-734c-4d10-a54a-9d0c2257c8ac','garyTest1',1,'338b8d48-501b-46c2-9e85-a6a8af1c1dec','2024-05-04 14:53:34','2024-05-04 14:53:34','OTHER',15,'OTHER',15),('3adb04e9-dbb5-4041-a02e-e65ea0e57eae','4321_202451183435','4321','0401357f-1efa-4cc1-8464-c99b37007e69',16,'2024-05-11',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'febc8331-3a69-4ebe-9b2f-fdd0425476a4','2024-05-11 00:34:35','2024-05-11 00:34:35',NULL,NULL,NULL,NULL),('3d6e27ba-1fa5-41e3-a7a3-b45c7a63385d','X001_202456','X001','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-05-08',49,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7276','使用者A',1,'7930c339-26ff-424a-95de-3b2dd2457a24','2024-05-06 04:32:26','2024-05-06 04:32:26',NULL,NULL,NULL,NULL),('427dd296-f8b5-4fb7-b984-9d8ccce05b2d','AAAAA_202454','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-12-31',100,'0401357f-1efa-4cc1-8464-c99b37007e69-046','','Amies w/Charcoal Swabs厭氧細菌拭子','支','PURCHASE','778909c2-734c-4d10-a54a-9d0c2257c8ac','garyTest1',1,'535e6aad-bd2f-4c18-ba3d-f70d0ba2bc89','2024-05-04 09:52:05','2024-05-04 09:52:05',NULL,NULL,NULL,NULL),('44f14fcd-ff9a-4c3b-a515-5ddf71560b35','AAAABBB_202453','AAAABBB','0401357f-1efa-4cc1-8464-c99b37007e69',9,'2024-12-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-041','','白色膠杯170CC','條','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'b4a0e4da-dbac-4427-8277-78a51939cd07','2024-05-02 16:04:14','2024-05-02 16:04:14',NULL,NULL,NULL,NULL),('486ae416-8e15-4c72-87c5-80504fe236be','3214403_202457','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',49,'2025-01-31',200,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'4a608d71-0158-4ae3-ac07-1be878b5b788','2024-05-07 03:15:10','2024-05-07 03:15:10',NULL,NULL,NULL,NULL),('4a2df881-1c25-4322-bc61-0ceb24368c08','3321_202451103927','3321','0401357f-1efa-4cc1-8464-c99b37007e69',14,'2024-05-11',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'0576e297-d649-4aca-978d-15992082af01','2024-05-10 16:39:27','2024-05-10 16:39:27',NULL,NULL,NULL,NULL),('4d8964fe-d5fc-4c7e-91f5-fddf8b62106e','8873_2024516165847','8873','68c05e1d-c6f5-498a-8359-d961359e5875',0,'2025-05-16',10,'68c05e1d-c6f5-498a-8359-d961359e5875-081','081','r滅菌96孔U型盤附蓋 _MJ96','個','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'bc45bd6a-d2d6-4441-9107-cd6eb14ba4a3','2024-05-16 08:58:47','2024-05-16 08:58:47',NULL,NULL,NULL,NULL),('50926e8f-14f6-46a9-8e8c-9887f621714b','54321_2024514175824','54321','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2024-05-14',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-298','298','石碳酸復紅溶液Carbolfuchsin Solution- ZN','組','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'8e429b11-e6bc-425b-86d8-e68c5108abaf','2024-05-14 09:58:24','2024-05-14 09:58:24',NULL,NULL,NULL,NULL),('53db2a1e-119e-40e2-b3eb-8a905a0c24e3','_202451515255','','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-221','221','分注瓶(小) ','瓶','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3be0128f-4d3e-4f26-ab3f-4e5b5dd7c9c2','2024-05-15 07:25:06','2024-05-15 07:25:06',NULL,NULL,NULL,NULL),('57e5833f-5fcd-4cd8-9b7e-70fb83e7437e','123456_2024510133525','123456','63c0dbc4-fdca-47e8-b084-5880fb596de2',236,'2025-10-10',12,'63c0dbc4-fdca-47e8-b084-5880fb596de2-236','236','Middlebrook 7H9 with 30%glycerol分枝桿菌7H9培養液(含甘油)1ML','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'136c6596-2b96-492f-b130-adbaad6abc57','2024-05-10 05:35:25','2024-05-10 05:35:25',NULL,NULL,NULL,NULL),('599d3a32-6309-4b5e-b308-4d2f0f7709e4','12345_202451518192','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',2,'2025-12-31',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-091','091','1000ul 藍色吸管尖','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'231b4064-7895-4461-995d-910348127c93','2024-05-15 10:19:02','2024-05-15 10:19:02',NULL,NULL,NULL,NULL),('5befcc99-e829-48c1-8a27-636133fdce5e','12345_202451421635','12345','68c05e1d-c6f5-498a-8359-d961359e5875',0,'2024-05-14',200,'68c05e1d-c6f5-498a-8359-d961359e5875-243','243','7H11 Argar Plate 結核桿菌培養基','包','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'ecbbca0f-4c65-4cf1-aeb9-adaa273e9c4d','2024-05-14 13:06:35','2024-05-14 13:06:35',NULL,NULL,NULL,NULL),('5d1fdf56-8738-431b-9adf-cbf35e92360e','12345_2024515194353','12345','68c05e1d-c6f5-498a-8359-d961359e5875',30,'2025-12-31',30,'68c05e1d-c6f5-498a-8359-d961359e5875-279','279','乳膠檢驗手套(無粉) 12\" GSO-SL-132 S','包','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'26479531-6430-4567-aa13-a8078648ddb1','2024-05-15 11:43:54','2024-05-15 11:43:54',NULL,NULL,NULL,NULL),('5f7fb77e-68d6-4ac3-ad78-9f4729e45591','3214403_2024510172723','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-05-07',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'3b20b559-c553-47c5-8176-6117a34a74fc','2024-05-10 09:27:23','2024-05-10 09:27:23',NULL,NULL,NULL,NULL),('6280bff5-e1b7-4220-9d31-0734c45a38e2','3214403_202457','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',253,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'ebeef775-11c5-45b0-a8ab-f0fd9b72d155','2024-05-07 03:45:34','2024-05-07 03:45:34',NULL,NULL,NULL,NULL),('63c92a70-bf1c-45cc-8dfc-4dd7c9d1e18f','12345_2024515194330','12345','68c05e1d-c6f5-498a-8359-d961359e5875',0,'2025-12-31',30,'68c05e1d-c6f5-498a-8359-d961359e5875-279','279','乳膠檢驗手套(無粉) 12\" GSO-SL-132 S','包','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'5ac7d507-8bc8-440c-8455-3a5b66bce8f2','2024-05-15 11:43:30','2024-05-15 11:43:30',NULL,NULL,NULL,NULL),('64887983-c167-4ce1-9833-2fe24b5addc7','12345890_2024512214954','12345890','63c0dbc4-fdca-47e8-b084-5880fb596de2',269,'2024-05-12',100,'63c0dbc4-fdca-47e8-b084-5880fb596de2-171','171','拜奈克思肺炎鏈球菌抗原檢驗卡 未滅菌','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'bb2c609e-6f93-40d6-8324-46bef873983e','2024-05-12 13:49:54','2024-05-12 13:49:54',NULL,NULL,NULL,NULL),('6831cf79-131e-4f75-bfc6-c9b27f033eec','4321_20245110341','4321','0401357f-1efa-4cc1-8464-c99b37007e69',12,'2024-05-11',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'27803f14-edc0-41ec-8a57-cc07878e0b97','2024-05-10 16:34:01','2024-05-10 16:34:01',NULL,NULL,NULL,NULL),('68ca23e2-f7c2-4e7f-8022-007837be80b5','5544321_2024515181953','5544321','63c0dbc4-fdca-47e8-b084-5880fb596de2',8,'2025-12-30',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-091','091','1000ul 藍色吸管尖','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'e3375dcb-154f-43da-91ec-c44d9bbd6aef','2024-05-15 10:19:53','2024-05-15 10:19:53',NULL,NULL,NULL,NULL),('696d66e6-fa7a-4267-b9f0-e2e70a98e97c','12345_2024515181442','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-261','261','1000ul filter tip濾膜吸管尖','箱','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'e3375dcb-154f-43da-91ec-c44d9bbd6aef','2024-05-15 10:14:42','2024-05-15 10:14:42',NULL,NULL,NULL,NULL),('69c83b1a-fba6-4451-925d-8b3c0bae7123','123_2024510133451','123','63c0dbc4-fdca-47e8-b084-5880fb596de2',234,'2025-05-10',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-234','234','McFarland標準液 0.5','支','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'136c6596-2b96-492f-b130-adbaad6abc57','2024-05-10 05:34:51','2024-05-10 05:34:51',NULL,NULL,NULL,NULL),('721e5487-157c-4922-975f-9f36a092ec91','12345_202451518817','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2024-12-31',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-260','260','2ml螺蓋圓底可站離心管 (滅菌裝 )','箱','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'e3375dcb-154f-43da-91ec-c44d9bbd6aef','2024-05-15 10:08:17','2024-05-15 10:08:17',NULL,NULL,NULL,NULL),('743fd0c7-5e75-4a72-8a76-226a9408445e','9998_2024510152642','9998','63c0dbc4-fdca-47e8-b084-5880fb596de2',212,'2025-10-10',4,'63c0dbc4-fdca-47e8-b084-5880fb596de2-212','212','Panbio新冠病毒抗原快速檢測裝置(NP)','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'66abe15c-f4f7-47dd-961d-de21a2162bed','2024-05-10 07:26:42','2024-05-10 07:26:42',NULL,NULL,NULL,NULL),('74458495-e5cf-4053-9288-a84fcf5fa983','Y001_202456','Y001','0401357f-1efa-4cc1-8464-c99b37007e69',100,'2024-05-07',10,'0401357f-1efa-4cc1-8464-c99b37007e69-013','','快速血清分離真空採血管5mL-367986(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7276','使用者A',1,'4a608d71-0158-4ae3-ac07-1be878b5b788','2024-05-06 04:40:54','2024-05-06 04:40:54',NULL,NULL,NULL,NULL),('771fe2eb-bcee-43ef-aaaa-b95e32308231','12345_2024515153953','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',10,'63c0dbc4-fdca-47e8-b084-5880fb596de2-063','063','張氏糞便濃縮集卵瓶 (含吸管,試管)','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3be0128f-4d3e-4f26-ab3f-4e5b5dd7c9c2','2024-05-15 07:39:53','2024-05-15 07:39:53',NULL,NULL,NULL,NULL),('792648ea-602a-4a1e-9126-7894368321f1','XXX01_202455','XXX01','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-05-05',100,'0401357f-1efa-4cc1-8464-c99b37007e69-013','','快速血清分離真空採血管5mL-367986(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7276','使用者A',1,'7930c339-26ff-424a-95de-3b2dd2457a24','2024-05-05 07:43:26','2024-05-05 07:43:26',NULL,NULL,NULL,NULL),('829ddacd-f606-4f43-8dae-cda4825f9eb8','4321_202451183511','4321','0401357f-1efa-4cc1-8464-c99b37007e69',17,'2024-05-11',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'848af024-4145-42cd-9784-919e2d0727e1','2024-05-11 00:35:11','2024-05-11 00:35:11',NULL,NULL,NULL,NULL),('83cd418d-ac01-46a0-b807-d17f58c4a190','12345_2024515181916','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',4,'2025-12-31',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-091','091','1000ul 藍色吸管尖','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3db51da5-c9ff-474b-b592-6828b3250ae8','2024-05-15 10:19:16','2024-05-15 10:19:16',NULL,NULL,NULL,NULL),('84c99eea-72ea-4044-ab55-450741f671ce','123456_2024510203558','123456','63c0dbc4-fdca-47e8-b084-5880fb596de2',206,'2024-05-31',3,'63c0dbc4-fdca-47e8-b084-5880fb596de2-206','206','亞培百而靈諾羅病毒快速檢驗套組','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'66abe15c-f4f7-47dd-961d-de21a2162bed','2024-05-10 12:35:58','2024-05-10 12:35:58',NULL,NULL,NULL,NULL),('8a42118e-d4af-4224-b9c2-51b4caeb0c9a','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',1,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'e297b6ff-601b-4841-bd81-1261db9bc2c5','2024-05-07 16:11:54','2024-05-07 16:11:54',NULL,NULL,NULL,NULL),('8f0ab511-8f53-41ff-9cb4-64e7f49c5416','12345_2024515193916','12345','68c05e1d-c6f5-498a-8359-d961359e5875',0,'2024-12-31',28,'68c05e1d-c6f5-498a-8359-d961359e5875-070','070','塑膠吸管3mL _0311','包','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'26479531-6430-4567-aa13-a8078648ddb1','2024-05-15 11:39:16','2024-05-15 11:39:16',NULL,NULL,NULL,NULL),('96c04b04-9fc5-4b3e-8072-94c9b19cdb0c','AAAAA_202454','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',300,'2024-12-31',100,'0401357f-1efa-4cc1-8464-c99b37007e69-046','','Amies w/Charcoal Swabs厭氧細菌拭子','支','PURCHASE','778909c2-734c-4d10-a54a-9d0c2257c8ac','garyTest1',1,'535e6aad-bd2f-4c18-ba3d-f70d0ba2bc89','2024-05-04 10:33:42','2024-05-04 10:33:42',NULL,NULL,NULL,NULL),('977cf027-d199-4721-8579-a6fc96144fe5','AAAAA_2024515105423','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2025-12-31',35,'0401357f-1efa-4cc1-8464-c99b37007e69-049','049','含溶血素螢光厭氧塑膠血液培養瓶(紫色)-442021','瓶','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'b85783ec-cc0f-4251-81fc-0ea30929b2b0','2024-05-15 02:54:22','2024-05-15 02:54:22','OTHER',15,'OTHER',15),('a48bab27-3f3b-457a-bb9c-0a7f68bcb0e3','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',3,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'2ea3c4cd-ee8f-43af-a3a4-144c0d1962c4','2024-05-07 16:25:37','2024-05-07 16:25:37',NULL,NULL,NULL,NULL),('a57f21b4-d658-4f04-9e58-296d332fd5c9','kjlj_2024510152712','kjlj','63c0dbc4-fdca-47e8-b084-5880fb596de2',201,'2025-10-10',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-201','201','拜奈克思呼吸道融合流病毒抗原快速診斷檢驗卡(未滅菌)','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'66abe15c-f4f7-47dd-961d-de21a2162bed','2024-05-10 07:27:12','2024-05-10 07:27:12',NULL,NULL,NULL,NULL),('a9c367ed-9026-4072-83cf-8f1df1cb5f6c','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'d07a1dc4-0660-4557-8657-b39df8e43360','2024-05-07 16:09:59','2024-05-07 16:09:59',NULL,NULL,NULL,NULL),('ad71372e-8afc-46e9-85e7-e9d8f609cbb8','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2025-01-31',30,'0401357f-1efa-4cc1-8464-c99b37007e69-011','','尿液檢體收集器 6mL-366408(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'8b249f7b-5a53-47e5-aa98-be20df067a1a','2024-05-07 16:07:44','2024-05-07 16:07:44',NULL,NULL,NULL,NULL),('b70ae9b1-2bbe-457a-b8d8-0606533ba10f','123JS99_2024515224658','123JS99','68c05e1d-c6f5-498a-8359-d961359e5875',0,'2025-05-13',2,'68c05e1d-c6f5-498a-8359-d961359e5875-099','099','STW醫用口罩(成人平面)','盒','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'d51244ab-d235-4cc2-b40f-c114d97a03a1','2024-05-15 14:46:59','2024-05-15 14:46:59',NULL,NULL,NULL,NULL),('c1da6273-fdcf-45e4-a6d3-ddc0bf4bdff2','123_2024510133436','123','63c0dbc4-fdca-47e8-b084-5880fb596de2',241,'2025-05-10',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-241','241','TB Drug Test / 結核菌藥物試驗培養基','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'136c6596-2b96-492f-b130-adbaad6abc57','2024-05-10 05:34:36','2024-05-10 05:34:36',NULL,NULL,NULL,NULL),('c22f8179-f675-4540-8cef-e9c4fdec4880','321_202451103613','321','0401357f-1efa-4cc1-8464-c99b37007e69',13,'2024-05-11',1,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'e731b4b5-d7a1-4b44-a1ce-a1126f0d2478','2024-05-10 16:36:13','2024-05-10 16:36:13',NULL,NULL,NULL,NULL),('c3be08a9-585e-4dd6-8ad2-9d36a27b8a75','54321_2024515154845','54321','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',4,'63c0dbc4-fdca-47e8-b084-5880fb596de2-039','039','張氏糞便濃縮集卵瓶 (無配件)','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3be0128f-4d3e-4f26-ab3f-4e5b5dd7c9c2','2024-05-15 07:48:45','2024-05-15 07:48:45',NULL,NULL,NULL,NULL),('c793a854-c7d4-45d8-9758-c105d23f408b','3214403_202451114652','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',117,'2024-05-11',10,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'1e93acb5-b0cc-453d-a819-8f3ce8f7cd15','2024-05-11 06:06:52','2024-05-11 06:06:52',NULL,NULL,NULL,NULL),('e00b554d-c038-4de8-84f6-afc3cdcb71dc','3214403_202457','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',251,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'e29123da-bcbf-4e51-bf11-13ffd2c3013a','2024-05-07 03:38:43','2024-05-07 03:38:43',NULL,NULL,NULL,NULL),('e03e05f1-94be-4662-b7b1-75afe7e1d4dd','3214403_202457','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',249,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'b94a4291-cfaa-46ec-966d-779bc49820f5','2024-05-07 03:30:41','2024-05-07 03:30:41',NULL,NULL,NULL,NULL),('e48cf216-f0bd-42a5-a0fd-449013907934','12345_2024515181621','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-091','091','1000ul 藍色吸管尖','包','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'5548b93d-e8d4-4fc3-a687-b9c678e8815c','2024-05-15 10:16:22','2024-05-15 10:16:22',NULL,NULL,NULL,NULL),('e6e39f5a-43fa-4617-85b0-9c95501ae475','12345_202451415300','12345','68c05e1d-c6f5-498a-8359-d961359e5875',0,'2024-05-14',24,'68c05e1d-c6f5-498a-8359-d961359e5875-110','110','酒精75% 4L/桶','桶','PURCHASE','b34f1c15-f459-4c7c-b460-8198c7aeac06','檢驗科主任',1,'e53c596f-65d3-4d23-a922-a8060a5e9266','2024-05-14 07:30:00','2024-05-14 07:30:00',NULL,NULL,NULL,NULL),('e6ea8e72-e43d-4563-afa9-3bb5c7b0f8b3','12345888_2024510204058','12345888','63c0dbc4-fdca-47e8-b084-5880fb596de2',171,'2024-05-29',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-171','171','拜奈克思肺炎鏈球菌抗原檢驗卡 未滅菌','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'66abe15c-f4f7-47dd-961d-de21a2162bed','2024-05-10 12:40:58','2024-05-10 12:40:58',NULL,NULL,NULL,NULL),('ebdafc08-4670-4773-9063-470f2e98e51b','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',4,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'3fab6258-4d61-4730-94a5-96ae05b050e1','2024-05-07 16:27:58','2024-05-07 16:27:58',NULL,NULL,NULL,NULL),('ed6731b9-2119-498c-910f-ae3b535a015c','3214403_2024511135943','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',17,'2024-05-11',100,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'00288d9c-8e9c-4df6-8191-af2d12c2d445','2024-05-11 05:59:43','2024-05-11 05:59:43',NULL,NULL,NULL,NULL),('ef8de641-3a96-448f-814f-fe211e6e16ce','99898564_2024510152819','99898564','63c0dbc4-fdca-47e8-b084-5880fb596de2',200,'2026-12-21',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-200','200','第二代拜奈克思流感二合一型病毒抗原快速檢驗試劑','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'66abe15c-f4f7-47dd-961d-de21a2162bed','2024-05-10 07:28:19','2024-05-10 07:28:19',NULL,NULL,NULL,NULL),('efd53d93-bd97-45b4-8a0d-427b9b20a526','_2024515183329','','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',1,'63c0dbc4-fdca-47e8-b084-5880fb596de2-262','262','200 filter tip濾膜吸管尖','箱','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'e3375dcb-154f-43da-91ec-c44d9bbd6aef','2024-05-15 10:33:29','2024-05-15 10:33:29',NULL,NULL,NULL,NULL),('f4513523-669e-4a92-b015-c4886b2e2235','12345_2024515153131','12345','63c0dbc4-fdca-47e8-b084-5880fb596de2',0,'2025-12-31',2,'63c0dbc4-fdca-47e8-b084-5880fb596de2-185','185','Ehrlichs Reagent','瓶','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'3be0128f-4d3e-4f26-ab3f-4e5b5dd7c9c2','2024-05-15 07:31:31','2024-05-15 07:31:31',NULL,NULL,NULL,NULL),('f675c504-3984-4d7d-ae43-b8fa9fd9c7b1','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',8,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'f086d8fd-e8de-4b4e-99b0-1802fccef196','2024-05-07 16:45:20','2024-05-07 16:45:20',NULL,NULL,NULL,NULL),('f9650a80-d3dc-4e0a-b0da-fba6b6e48686','AAAAA_20245823235','AAAAA','0401357f-1efa-4cc1-8464-c99b37007e69',0,'2024-09-28',10,'0401357f-1efa-4cc1-8464-c99b37007e69-023','023','嬰兒專用微量血清採血管(黃)-SST-365967(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'29a1fce3-e767-4d82-bd13-2a27243b3115','2024-05-08 15:23:05','2024-05-08 15:23:05',NULL,NULL,NULL,NULL),('fe5a3e6e-f82c-465a-8759-6ed2ce87ed7d','12345889_2024512211858','12345889','63c0dbc4-fdca-47e8-b084-5880fb596de2',172,'2024-05-12',100,'63c0dbc4-fdca-47e8-b084-5880fb596de2-171','171','拜奈克思肺炎鏈球菌抗原檢驗卡 未滅菌','盒','PURCHASE','c3f28016-9e85-441c-826a-743ff9314433','金萬林管理員',1,'9c9a79ad-3d96-42bd-99e8-1153d3b64b64','2024-05-12 13:18:58','2024-05-12 13:18:58',NULL,NULL,NULL,NULL),('ffd28138-c4be-43fd-a119-558531cb8e2f','3214403_202458','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',2,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'67f5da15-8503-43ac-b07a-56fe381c00a7','2024-05-07 16:21:06','2024-05-07 16:21:06',NULL,NULL,NULL,NULL),('ffe67961-50df-481d-8986-ac4b73775c3c','3214403_202457','3214403','0401357f-1efa-4cc1-8464-c99b37007e69',250,'2025-01-31',1,'0401357f-1efa-4cc1-8464-c99b37007e69-017','','檸檬鈉真空採血管1.8mL-363080(支)','支','PURCHASE','9ddc8a2d-6386-402d-b658-6b37decd7299','Tony',1,'5aaa5839-1b74-47e3-a7cd-16b34e56eabb','2024-05-07 03:36:39','2024-05-07 03:36:39',NULL,NULL,NULL,NULL);
/*!40000 ALTER TABLE `temp_in_stock_item_record` ENABLE KEYS */;
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
