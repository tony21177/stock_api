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
-- Temporary view structure for view `purchase_acceptance_items_view`
--

DROP TABLE IF EXISTS `purchase_acceptance_items_view`;
/*!50001 DROP VIEW IF EXISTS `purchase_acceptance_items_view`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `purchase_acceptance_items_view` AS SELECT 
 1 AS `PurchaseMainId`,
 1 AS `ApplyDate`,
 1 AS `CompId`,
 1 AS `CurrentStatus`,
 1 AS `DemandDate`,
 1 AS `GroupIds`,
 1 AS `Remarks`,
 1 AS `UserId`,
 1 AS `ReceiveStatus`,
 1 AS `Type`,
 1 AS `CreatedAt`,
 1 AS `UpdatedAt`,
 1 AS `IsActive`,
 1 AS `AcceptId`,
 1 AS `AcceptQuantity`,
 1 AS `AcceptUserID`,
 1 AS `LotNumberBatch`,
 1 AS `LotNumber`,
 1 AS `ExpirationDate`,
 1 AS `ItemId`,
 1 AS `OrderQuantity`,
 1 AS `PackagingStatus`,
 1 AS `ProductId`,
 1 AS `ProductName`,
 1 AS `ProductSpec`,
 1 AS `UDISerialCode`,
 1 AS `QcStatus`,
 1 AS `CurrentTotalQuantity`,
 1 AS `Comment`,
 1 AS `QcComment`,
 1 AS `AcceptCreatedAt`,
 1 AS `AcceptUpdatedAt`,
 1 AS `DeliverFunction`,
 1 AS `DeliverTemperature`,
 1 AS `SavingFunction`,
 1 AS `SavingTemperature`*/;
SET character_set_client = @saved_cs_client;

--
-- Temporary view structure for view `purchase_item_list_view`
--

DROP TABLE IF EXISTS `purchase_item_list_view`;
/*!50001 DROP VIEW IF EXISTS `purchase_item_list_view`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `purchase_item_list_view` AS SELECT 
 1 AS `PurchaseMainId`,
 1 AS `ApplyDate`,
 1 AS `CompId`,
 1 AS `CurrentStatus`,
 1 AS `DemandDate`,
 1 AS `GroupIds`,
 1 AS `Remarks`,
 1 AS `UserId`,
 1 AS `ReceiveStatus`,
 1 AS `Type`,
 1 AS `CreatedAt`,
 1 AS `UpdatedAt`,
 1 AS `IsActive`,
 1 AS `MainSplitPrcoess`,
 1 AS `ItemId`,
 1 AS `Comment`,
 1 AS `ProductId`,
 1 AS `ProductName`,
 1 AS `ProductSpec`,
 1 AS `Quantity`,
 1 AS `ReceiveQuantity`,
 1 AS `ItemReceiveStatus`,
 1 AS `ItemGroupIds`,
 1 AS `ItemGroupNames`,
 1 AS `ProductCategory`,
 1 AS `ArrangeSupplierId`,
 1 AS `ArrangeSupplierName`,
 1 AS `CurrentInStockQuantity`,
 1 AS `WithCompId`,
 1 AS `withPurchaseMainId`,
 1 AS `withItemId`,
 1 AS `SubSplitProcess`*/;
SET character_set_client = @saved_cs_client;

--
-- Final view structure for view `purchase_acceptance_items_view`
--

/*!50001 DROP VIEW IF EXISTS `purchase_acceptance_items_view`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`Gary`@`%` SQL SECURITY DEFINER */
/*!50001 VIEW `purchase_acceptance_items_view` AS select `p`.`PurchaseMainId` AS `PurchaseMainId`,`p`.`ApplyDate` AS `ApplyDate`,`p`.`CompId` AS `CompId`,`p`.`CurrentStatus` AS `CurrentStatus`,`p`.`DemandDate` AS `DemandDate`,`p`.`GroupIds` AS `GroupIds`,`p`.`Remarks` AS `Remarks`,`p`.`UserId` AS `UserId`,`p`.`ReceiveStatus` AS `ReceiveStatus`,`p`.`Type` AS `Type`,`p`.`CreatedAt` AS `CreatedAt`,`p`.`UpdatedAt` AS `UpdatedAt`,`p`.`IsActive` AS `IsActive`,`a`.`AcceptId` AS `AcceptId`,`a`.`AcceptQuantity` AS `AcceptQuantity`,`a`.`AcceptUserID` AS `AcceptUserID`,`a`.`LotNumberBatch` AS `LotNumberBatch`,`a`.`LotNumber` AS `LotNumber`,`a`.`ExpirationDate` AS `ExpirationDate`,`a`.`ItemId` AS `ItemId`,`a`.`OrderQuantity` AS `OrderQuantity`,`a`.`PackagingStatus` AS `PackagingStatus`,`a`.`ProductId` AS `ProductId`,`a`.`ProductName` AS `ProductName`,`a`.`ProductSpec` AS `ProductSpec`,`a`.`UDISerialCode` AS `UDISerialCode`,`a`.`QcStatus` AS `QcStatus`,`a`.`CurrentTotalQuantity` AS `CurrentTotalQuantity`,`a`.`Comment` AS `Comment`,`a`.`QcComment` AS `QcComment`,`a`.`CreatedAt` AS `AcceptCreatedAt`,`a`.`UpdatedAt` AS `AcceptUpdatedAt`,`a`.`DeliverFunction` AS `DeliverFunction`,`a`.`DeliverTemperature` AS `DeliverTemperature`,`a`.`SavingFunction` AS `SavingFunction`,`a`.`SavingTemperature` AS `SavingTemperature` from (`purchase_main_sheet` `p` join `acceptance_item` `a` on((`p`.`PurchaseMainId` = `a`.`PurchaseMainId`))) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `purchase_item_list_view`
--

/*!50001 DROP VIEW IF EXISTS `purchase_item_list_view`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`Gary`@`%` SQL SECURITY DEFINER */
/*!50001 VIEW `purchase_item_list_view` AS select `m`.`PurchaseMainId` AS `PurchaseMainId`,`m`.`ApplyDate` AS `ApplyDate`,`m`.`CompId` AS `CompId`,`m`.`CurrentStatus` AS `CurrentStatus`,`m`.`DemandDate` AS `DemandDate`,`m`.`GroupIds` AS `GroupIds`,`m`.`Remarks` AS `Remarks`,`m`.`UserId` AS `UserId`,`m`.`ReceiveStatus` AS `ReceiveStatus`,`m`.`Type` AS `Type`,`m`.`CreatedAt` AS `CreatedAt`,`m`.`UpdatedAt` AS `UpdatedAt`,`m`.`IsActive` AS `IsActive`,`m`.`SplitPrcoess` AS `MainSplitPrcoess`,`s`.`ItemId` AS `ItemId`,`s`.`Comment` AS `Comment`,`s`.`ProductId` AS `ProductId`,`s`.`ProductName` AS `ProductName`,`s`.`ProductSpec` AS `ProductSpec`,`s`.`Quantity` AS `Quantity`,`s`.`ReceiveQuantity` AS `ReceiveQuantity`,`s`.`ReceiveStatus` AS `ItemReceiveStatus`,`s`.`GroupIds` AS `ItemGroupIds`,`s`.`GroupNames` AS `ItemGroupNames`,`s`.`ProductCategory` AS `ProductCategory`,`s`.`ArrangeSupplierId` AS `ArrangeSupplierId`,`s`.`ArrangeSupplierName` AS `ArrangeSupplierName`,`s`.`CurrentInStockQuantity` AS `CurrentInStockQuantity`,`s`.`WithCompId` AS `WithCompId`,`s`.`withPurchaseMainId` AS `withPurchaseMainId`,`s`.`withItemId` AS `withItemId`,`s`.`SplitProcess` AS `SubSplitProcess` from (`purchase_main_sheet` `m` join `purchase_sub_item` `s` on((`m`.`PurchaseMainId` = `s`.`PurchaseMainId`))) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-05-18 14:55:21
