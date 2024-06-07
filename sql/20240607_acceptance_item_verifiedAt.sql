ALTER TABLE `stock`.`acceptance_item` 
ADD COLUMN `VerifyAt` TIMESTAMP NULL DEFAULT NULL AFTER `LotNumberBatchSeq`;


DROP VIEW purchase_acceptance_items_view;

CREATE 
VIEW `purchase_acceptance_items_view` AS
    SELECT 
        `p`.`PurchaseMainId` AS `PurchaseMainId`,
        `p`.`ApplyDate` AS `ApplyDate`,
        `p`.`CompId` AS `CompId`,
        `p`.`CurrentStatus` AS `CurrentStatus`,
        `p`.`DemandDate` AS `DemandDate`,
        `p`.`GroupIds` AS `GroupIds`,
        `p`.`Remarks` AS `Remarks`,
        `p`.`UserId` AS `UserId`,
        `p`.`ReceiveStatus` AS `ReceiveStatus`,
        `p`.`Type` AS `Type`,
        `p`.`CreatedAt` AS `CreatedAt`,
        `p`.`UpdatedAt` AS `UpdatedAt`,
        `p`.`IsActive` AS `IsActive`,
        `p`.`SplitPrcoess` AS `SplitPrcoess`,
        `a`.`AcceptId` AS `AcceptId`,
        `a`.`AcceptQuantity` AS `AcceptQuantity`,
        `a`.`AcceptUserID` AS `AcceptUserID`,
        `a`.`LotNumberBatch` AS `LotNumberBatch`,
        `a`.`LotNumber` AS `LotNumber`,
        `a`.`ExpirationDate` AS `ExpirationDate`,
        `a`.`ItemId` AS `ItemId`,
        `a`.`OrderQuantity` AS `OrderQuantity`,
        `a`.`PackagingStatus` AS `PackagingStatus`,
        `a`.`ProductId` AS `ProductId`,
        `a`.`ProductCode` AS `ProductCode`,
        `a`.`ProductName` AS `ProductName`,
        `a`.`ProductSpec` AS `ProductSpec`,
        `a`.`UDISerialCode` AS `UDISerialCode`,
        `a`.`QcStatus` AS `QcStatus`,
        `a`.`CurrentTotalQuantity` AS `CurrentTotalQuantity`,
        `a`.`Comment` AS `Comment`,
        `a`.`QcComment` AS `QcComment`,
        `a`.`CreatedAt` AS `AcceptCreatedAt`,
        `a`.`UpdatedAt` AS `AcceptUpdatedAt`,
        `a`.`DeliverFunction` AS `DeliverFunction`,
        `a`.`DeliverTemperature` AS `DeliverTemperature`,
        `a`.`SavingFunction` AS `SavingFunction`,
        `a`.`SavingTemperature` AS `SavingTemperature`,
        `a`.`InStockStatus` AS `InStockStatus`,
        `a`.`ArrangeSupplierId` AS `ArrangeSupplierId`,
        `a`.`ArrangeSupplierName` AS `ArrangeSupplierName`,
        `a`.`VerifyAt` AS `VerifyAt`
    FROM
        (`purchase_main_sheet` `p`
        JOIN `acceptance_item` `a` ON ((`p`.`PurchaseMainId` = `a`.`PurchaseMainId`)))