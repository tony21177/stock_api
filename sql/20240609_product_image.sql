CREATE TABLE `product_image` (
  `id` int NOT NULL AUTO_INCREMENT,
  `productId` varchar(100) DEFAULT NULL,
  `CompId` varchar(45) DEFAULT NULL,
  `Image` text,
  `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
