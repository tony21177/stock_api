-- MySQL dump 10.13  Distrib 8.0.33, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: handover
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
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `account_password`
--

DROP TABLE IF EXISTS `account_password`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `account_password` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `username` varchar(255) NOT NULL,
  `password` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `identity` varchar(255) DEFAULT NULL,
  `medical_institution` varchar(10) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `backup`
--

DROP TABLE IF EXISTS `backup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `backup` (
  `id` int NOT NULL AUTO_INCREMENT,
  `check_date` date DEFAULT NULL,
  `passport` varchar(12) NOT NULL,
  `medical_record_number` varchar(20) NOT NULL,
  `english_name` varchar(50) NOT NULL,
  `item` varchar(10) NOT NULL,
  `result` varchar(50) NOT NULL,
  `personnel` varchar(5) NOT NULL,
  `reviewer` varchar(5) NOT NULL,
  `upload_date` datetime DEFAULT NULL,
  `review_date` date DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `病歷號碼` (`medical_record_number`),
  KEY `護照` (`passport`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `calendar`
--

DROP TABLE IF EXISTS `calendar`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `calendar` (
  `id` int NOT NULL AUTO_INCREMENT,
  `subject` varchar(50) NOT NULL,
  `title` varchar(20) NOT NULL,
  `starttime` varchar(11) NOT NULL,
  `endtime` varchar(11) NOT NULL,
  `color` varchar(50) NOT NULL,
  `allday` varchar(1) NOT NULL DEFAULT '0',
  `body` varchar(255) NOT NULL,
  `tag` varchar(50) NOT NULL,
  `isserie` varchar(1) NOT NULL DEFAULT '0',
  `notify_icon` varchar(1) NOT NULL DEFAULT '0',
  `notify_minutes` varchar(20) NOT NULL DEFAULT '-1',
  `is_private` varchar(1) NOT NULL DEFAULT '0',
  `event_show_as` varchar(1) NOT NULL DEFAULT '0',
  `location` varchar(10) NOT NULL,
  `serie_key` varchar(1) NOT NULL DEFAULT '1',
  `sign` varchar(255) NOT NULL,
  `link` varchar(200) NOT NULL,
  `sign_for_receipt` varchar(5) NOT NULL,
  `issued` tinyint(1) NOT NULL,
  `initiator` varchar(5) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `calendar_execution`
--

DROP TABLE IF EXISTS `calendar_execution`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `calendar_execution` (
  `execution_date` datetime DEFAULT NULL,
  `count_value` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `calendar_index`
--

DROP TABLE IF EXISTS `calendar_index`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `calendar_index` (
  `id` int NOT NULL AUTO_INCREMENT,
  `category` varchar(20) NOT NULL,
  `index_value` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `chat`
--

DROP TABLE IF EXISTS `chat`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chat` (
  `id` int NOT NULL AUTO_INCREMENT,
  `time` datetime DEFAULT NULL,
  `person` varchar(255) DEFAULT NULL,
  `content` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `countries`
--

DROP TABLE IF EXISTS `countries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `countries` (
  `id` int NOT NULL AUTO_INCREMENT,
  `area_code` varchar(2) NOT NULL,
  `region` varchar(20) NOT NULL,
  `country` varchar(20) NOT NULL,
  `identity_type` varchar(10) NOT NULL,
  `item` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `國家` (`country`),
  KEY `項目` (`item`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `equipment_allocation`
--

DROP TABLE IF EXISTS `equipment_allocation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `equipment_allocation` (
  `usage_date` date NOT NULL,
  `usage_session` varchar(11) NOT NULL,
  `team_assignment` varchar(3) NOT NULL,
  `equipment_number` varchar(10) NOT NULL,
  `equipment_name` varchar(50) NOT NULL,
  `allocator` varchar(5) NOT NULL,
  `remark` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `file_list`
--

DROP TABLE IF EXISTS `file_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `file_list` (
  `id` int NOT NULL AUTO_INCREMENT,
  `id_index` varchar(11) NOT NULL,
  `title` varchar(20) NOT NULL,
  `event_date` datetime NOT NULL,
  `category` varchar(20) NOT NULL,
  `update_date` datetime NOT NULL,
  `file` varchar(100) NOT NULL,
  `personnel` varchar(5) NOT NULL,
  `file_status` int NOT NULL DEFAULT '1',
  `file_level` int NOT NULL DEFAULT '3',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `file_output`
--

DROP TABLE IF EXISTS `file_output`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `file_output` (
  `check_date` date DEFAULT NULL,
  `medical_record_number` varchar(11) NOT NULL,
  `number` varchar(5) NOT NULL,
  `name` varchar(8) NOT NULL,
  `item` varchar(10) NOT NULL,
  `result` varchar(50) NOT NULL,
  `personnel` varchar(5) NOT NULL,
  `reviewer` varchar(5) NOT NULL,
  `upload_date` date DEFAULT NULL,
  `review_date` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `foreign_languages`
--

DROP TABLE IF EXISTS `foreign_languages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `foreign_languages` (
  `nationality` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `gender` varchar(255) DEFAULT NULL,
  `country` varchar(255) DEFAULT NULL,
  `birthdate` varchar(255) DEFAULT NULL,
  `passport_number` varchar(255) DEFAULT NULL,
  `residence_permit_number` varchar(255) DEFAULT NULL,
  `home_phone` varchar(255) DEFAULT NULL,
  `mobile_phone` varchar(255) DEFAULT NULL,
  `work_city` varchar(255) DEFAULT NULL,
  `health_check_type` varchar(255) DEFAULT NULL,
  `medical_inquiry` varchar(255) DEFAULT NULL,
  `height` varchar(255) DEFAULT NULL,
  `weight` varchar(255) DEFAULT NULL,
  `body_temperature` varchar(255) DEFAULT NULL,
  `vision` varchar(255) DEFAULT NULL,
  `left_right` varchar(255) DEFAULT NULL,
  `blood_pressure` varchar(255) DEFAULT NULL,
  `second_blood_pressure` varchar(255) DEFAULT NULL,
  `confirmed_pregnancy` varchar(255) DEFAULT NULL,
  `unconfirmed_pregnancy` varchar(255) DEFAULT NULL,
  `not_pregnant` varchar(255) DEFAULT NULL,
  `signature_field` varchar(255) DEFAULT NULL,
  `height_field` varchar(255) DEFAULT NULL,
  `weight_field` varchar(255) DEFAULT NULL,
  `body_temperature_field` varchar(255) DEFAULT NULL,
  `vision_field` varchar(255) DEFAULT NULL,
  `blood_pressure_field` varchar(255) DEFAULT NULL,
  `xray_field` varchar(255) DEFAULT NULL,
  `blood_test_field` varchar(255) DEFAULT NULL,
  `stool_field` varchar(255) DEFAULT NULL,
  `enema` varchar(255) DEFAULT NULL,
  `warning_one` varchar(255) NOT NULL,
  `warning_two` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `foreignt`
--

DROP TABLE IF EXISTS `foreignt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `foreignt` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reservation_date` date DEFAULT NULL,
  `inspection_date` date DEFAULT NULL,
  `dispatch` varchar(10) NOT NULL,
  `employer_phone` varchar(25) NOT NULL,
  `inspection_period` varchar(2) NOT NULL,
  `passport` varchar(12) NOT NULL,
  `medical_record_number` varchar(20) NOT NULL,
  `english_name` varchar(50) NOT NULL,
  `item` varchar(10) NOT NULL,
  `item_code` int NOT NULL,
  `result` varchar(255) NOT NULL,
  `person` varchar(5) NOT NULL,
  `reviewer` varchar(5) NOT NULL,
  `upload_date` datetime DEFAULT NULL,
  `complete_date` datetime DEFAULT NULL,
  `report_review_date` date DEFAULT NULL,
  `attachment` varchar(5) NOT NULL DEFAULT 'false',
  `inspection_category` varchar(10) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `病歷號碼` (`medical_record_number`),
  KEY `護照` (`passport`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `foreigntback`
--

DROP TABLE IF EXISTS `foreigntback`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `foreigntback` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reservation_date` date DEFAULT NULL,
  `inspection_date` date DEFAULT NULL,
  `dispatch` varchar(10) NOT NULL,
  `employer_phone` varchar(25) NOT NULL,
  `inspection_period` varchar(2) NOT NULL,
  `passport` varchar(12) NOT NULL,
  `medical_record_number` varchar(20) NOT NULL,
  `english_name` varchar(50) NOT NULL,
  `item` varchar(10) NOT NULL,
  `item_code` int NOT NULL,
  `result` varchar(50) NOT NULL,
  `person` varchar(5) NOT NULL,
  `reviewer` varchar(5) NOT NULL,
  `upload_date` datetime DEFAULT NULL,
  `report_review_date` date DEFAULT NULL,
  `attachment` varchar(5) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `病歷號碼` (`medical_record_number`),
  KEY `護照` (`passport`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `generallaborpackage`
--

DROP TABLE IF EXISTS `generallaborpackage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `generallaborpackage` (
  `id` int NOT NULL AUTO_INCREMENT,
  `project_number` varchar(11) NOT NULL,
  `inspected_unit` varchar(10) NOT NULL,
  `package_category` varchar(4) NOT NULL,
  `package_name` varchar(20) NOT NULL,
  `package_price` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `healthcare_facilities`
--

DROP TABLE IF EXISTS `healthcare_facilities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `healthcare_facilities` (
  `facility_address` varchar(255) DEFAULT NULL,
  `facility_code` varchar(255) DEFAULT NULL,
  `column3` varchar(255) DEFAULT NULL,
  `column4` varchar(255) DEFAULT NULL,
  `phone_number` varchar(255) DEFAULT NULL,
  `facility_name` varchar(255) DEFAULT NULL,
  `phone_area_code` varchar(255) DEFAULT NULL,
  `identifier` int DEFAULT NULL,
  `termination_date` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `identity_type`
--

DROP TABLE IF EXISTS `identity_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `identity_type` (
  `id` int NOT NULL AUTO_INCREMENT,
  `identifier` int DEFAULT NULL,
  `identity` varchar(255) DEFAULT NULL,
  `check_type` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `identity_type_marking`
--

DROP TABLE IF EXISTS `identity_type_marking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `identity_type_marking` (
  `identity_type` varchar(255) NOT NULL,
  `marking` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inspection_unit_management`
--

DROP TABLE IF EXISTS `inspection_unit_management`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inspection_unit_management` (
  `id` int NOT NULL AUTO_INCREMENT,
  `project_number` varchar(12) NOT NULL,
  `name` varchar(20) NOT NULL,
  `tax_id` int NOT NULL,
  `contact_number` varchar(20) NOT NULL,
  `contact_person` varchar(5) NOT NULL,
  `address` varchar(50) NOT NULL,
  `note` varchar(20) NOT NULL,
  `color` varchar(20) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `專案號碼` (`project_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `instrument_equipment_list`
--

DROP TABLE IF EXISTS `instrument_equipment_list`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `instrument_equipment_list` (
  `id` varchar(255) DEFAULT NULL,
  `equipment_name` varchar(255) DEFAULT NULL,
  `brand_model` varchar(255) DEFAULT NULL,
  `measurement_range` varchar(255) DEFAULT NULL,
  `sop_number` varchar(255) DEFAULT NULL,
  `location` varchar(255) DEFAULT NULL,
  `custodian` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inventory_parameters`
--

DROP TABLE IF EXISTS `inventory_parameters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory_parameters` (
  `id` int NOT NULL AUTO_INCREMENT,
  `object` varchar(20) NOT NULL,
  `group` varchar(10) NOT NULL,
  `product_name` varchar(20) NOT NULL,
  `packaging` varchar(10) NOT NULL,
  `vendor` varchar(20) NOT NULL,
  `unit` varchar(10) NOT NULL,
  `storage_location` varchar(20) NOT NULL,
  `storage_temperature` varchar(10) NOT NULL,
  `usage_per_unit` varchar(10) NOT NULL,
  `instrument_used` varchar(20) NOT NULL,
  `item_category` varchar(10) NOT NULL,
  `safety_stock` int NOT NULL,
  `stock_upper_limit` int NOT NULL,
  `acceptance_period` int NOT NULL,
  `order_hotline` varchar(20) NOT NULL,
  `shelf_life_after_opening` int NOT NULL,
  `advanced_inspection` varchar(4) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `lab_test_management`
--

DROP TABLE IF EXISTS `lab_test_management`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lab_test_management` (
  `item_code` int NOT NULL AUTO_INCREMENT,
  `item` varchar(50) NOT NULL,
  `category` varchar(10) NOT NULL,
  `foreign_worker_exclusive` varchar(4) NOT NULL,
  `chinese_name` varchar(30) NOT NULL,
  `chinese_abbreviation` varchar(30) NOT NULL,
  `gender_exclusive` varchar(4) NOT NULL,
  `examination_category` varchar(10) NOT NULL,
  `item_category` varchar(10) NOT NULL,
  `unit` varchar(10) NOT NULL,
  `format` varchar(5) NOT NULL,
  `clinical_decision_value` varchar(50) NOT NULL,
  `single_comparison_value` varchar(500) DEFAULT NULL,
  `male_warning_low` double NOT NULL,
  `male_warning_high` double NOT NULL,
  `female_warning_low` double NOT NULL,
  `female_warning_high` double NOT NULL,
  `reference_range` varchar(100) NOT NULL,
  `specimen_type` varchar(10) NOT NULL,
  `specimen_volume` varchar(255) DEFAULT NULL,
  `collection_container` varchar(255) DEFAULT NULL,
  `item_description` varchar(255) DEFAULT NULL,
  `antibody_judgment` varchar(5) NOT NULL,
  `common_phrases` varchar(255) DEFAULT NULL,
  `below_warning_low_string` varchar(255) DEFAULT NULL,
  `below_low_string` varchar(255) DEFAULT NULL,
  `above_high_string` varchar(255) DEFAULT NULL,
  `above_warning_high_string` varchar(255) DEFAULT NULL,
  `keying_order` int NOT NULL,
  `tracking_treatment_location` varchar(255) DEFAULT NULL,
  `report_sorting` varchar(255) DEFAULT NULL,
  `occupational_hazard_item_code` varchar(255) DEFAULT NULL,
  `report_printing_name` varchar(255) DEFAULT NULL,
  `english_report_printing_name` varchar(255) DEFAULT NULL,
  `english_examination_category` varchar(255) DEFAULT NULL,
  `english_item_category` varchar(255) DEFAULT NULL,
  `process_order` varchar(255) DEFAULT NULL,
  `container_code` varchar(255) DEFAULT NULL,
  `tube_order` varchar(255) DEFAULT NULL,
  `split_order_group` varchar(255) DEFAULT NULL,
  `selling_price` varchar(255) DEFAULT NULL,
  `cost_price` varchar(255) DEFAULT NULL,
  `occupational_hazard_price` varchar(255) DEFAULT NULL,
  `overall_review_sorting` varchar(255) DEFAULT NULL,
  `entry_sorting` varchar(255) DEFAULT NULL,
  `input_type` varchar(255) DEFAULT NULL,
  `examination_usage_time` varchar(255) DEFAULT NULL,
  `overall_review_category` varchar(255) DEFAULT NULL,
  `display_overall_review` varchar(255) DEFAULT NULL,
  `health_insurance_code` varchar(255) DEFAULT NULL,
  `NPO` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`item_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `legal_infectious_disease_positive_specimen_control_record`
--

DROP TABLE IF EXISTS `legal_infectious_disease_positive_specimen_control_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `legal_infectious_disease_positive_specimen_control_record` (
  `id` int NOT NULL AUTO_INCREMENT,
  `login_date` date DEFAULT NULL,
  `unit` varchar(50) NOT NULL,
  `number` varchar(25) NOT NULL,
  `name` varchar(50) NOT NULL,
  `item` varchar(50) NOT NULL,
  `result_report` varchar(255) NOT NULL,
  `save_date` varchar(50) NOT NULL,
  `disposal_date` varchar(50) NOT NULL,
  `personnel` varchar(5) NOT NULL,
  `audit` varchar(50) NOT NULL,
  `remark` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `license_data_confirmation`
--

DROP TABLE IF EXISTS `license_data_confirmation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `license_data_confirmation` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `medical_record_number` varchar(11) NOT NULL,
  `date` datetime DEFAULT NULL,
  `examination_date` date DEFAULT NULL,
  `english_name` varchar(5) NOT NULL,
  `birthday` varchar(5) NOT NULL,
  `passport` varchar(5) NOT NULL,
  `nationality` varchar(5) NOT NULL,
  `gender` varchar(5) NOT NULL,
  `ID_number` varchar(5) NOT NULL,
  `chinese_name` varchar(5) NOT NULL,
  `m_english_name` varchar(5) NOT NULL,
  `m_birthday` varchar(5) NOT NULL,
  `m_passport` varchar(5) NOT NULL,
  `m_nationality` varchar(5) NOT NULL,
  `m_gender` varchar(5) NOT NULL,
  `m_ID_number` varchar(5) NOT NULL,
  `m_chinese_name` varchar(5) NOT NULL,
  `status` int NOT NULL,
  `verification` int NOT NULL,
  `verifier` varchar(5) NOT NULL,
  `verification_date` datetime NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `linemessnger`
--

DROP TABLE IF EXISTS `linemessnger`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `linemessnger` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` varchar(255) NOT NULL,
  `user` varchar(10) NOT NULL,
  `category` varchar(10) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mifqualitycontrol`
--

DROP TABLE IF EXISTS `mifqualitycontrol`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mifqualitycontrol` (
  `id` int NOT NULL AUTO_INCREMENT,
  `configuration_date` datetime DEFAULT NULL,
  `production_volume_ml` varchar(255) NOT NULL,
  `preparer` varchar(255) NOT NULL,
  `expiration_date` datetime DEFAULT NULL,
  `usage_date` date DEFAULT NULL,
  `quality_control_status` varchar(255) NOT NULL,
  `quality_control_performer` varchar(5) NOT NULL,
  `remark` varchar(255) NOT NULL,
  `approver` varchar(10) NOT NULL,
  `approval_date` date DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `nationality_codes`
--

DROP TABLE IF EXISTS `nationality_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `nationality_codes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `order_management`
--

DROP TABLE IF EXISTS `order_management`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_management` (
  `id` int NOT NULL AUTO_INCREMENT,
  `order_date` date NOT NULL,
  `item_name` varchar(20) NOT NULL,
  `item_category` varchar(20) NOT NULL,
  `supplier` varchar(20) NOT NULL,
  `usage_group` varchar(20) NOT NULL,
  `order_quantity` varchar(10) NOT NULL,
  `total_quantity_after_storage` varchar(20) NOT NULL,
  `applicant` varchar(5) NOT NULL,
  `storage_date` date NOT NULL,
  `storage_lot_number` varchar(255) DEFAULT NULL,
  `storage_quantity` varchar(255) DEFAULT NULL,
  `preliminary_acceptance_status` varchar(255) DEFAULT NULL,
  `noncompliance_action` varchar(255) DEFAULT NULL,
  `acceptor` varchar(255) DEFAULT NULL,
  `note` varchar(20) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `medical_record_number` varchar(11) NOT NULL,
  `status` varchar(8) NOT NULL DEFAULT 'Edit',
  `project_number` varchar(11) NOT NULL,
  `worker_category` varchar(10) NOT NULL,
  `examination_date` date DEFAULT NULL,
  `number` varchar(5) NOT NULL,
  `passport` varchar(12) NOT NULL,
  `receive_date` date DEFAULT NULL,
  `actual_date` datetime DEFAULT NULL,
  `refund_time` datetime DEFAULT NULL,
  `examination_period` varchar(2) NOT NULL,
  `english_name` varchar(50) NOT NULL,
  `chinese_name` varchar(10) NOT NULL,
  `id_number` varchar(12) NOT NULL,
  `source_name` varchar(2) NOT NULL DEFAULT '門診',
  `gender` varchar(2) NOT NULL,
  `birthdate` date NOT NULL,
  `nationality` varchar(5) NOT NULL,
  `workplace` varchar(5) NOT NULL,
  `examination_category` varchar(4) NOT NULL DEFAULT '第二類',
  `examination_type` varchar(50) NOT NULL,
  `additional_item` varchar(50) NOT NULL,
  `identity_type` varchar(10) NOT NULL,
  `dispatching_party` varchar(10) NOT NULL,
  `agent_phone` varchar(20) NOT NULL,
  `employer_phone` varchar(20) NOT NULL,
  `worker_mobile` varchar(20) NOT NULL,
  `residence` varchar(50) NOT NULL,
  `past_diseases` varchar(20) NOT NULL,
  `execution_item` varchar(20) NOT NULL,
  `receivable_amount` decimal(10,0) NOT NULL,
  `return_date` datetime DEFAULT NULL,
  `reexamination_date` datetime DEFAULT NULL,
  `export_excel_date` datetime DEFAULT NULL,
  `photo_submission` varchar(6) NOT NULL,
  `photo_date` date DEFAULT NULL,
  `physical_report_date` date DEFAULT NULL,
  `lab_report_date` date DEFAULT NULL,
  `xray_result` datetime DEFAULT NULL,
  `reviewer` varchar(5) NOT NULL,
  `review_time` datetime DEFAULT NULL,
  `report_print_date` date DEFAULT NULL,
  `report_date` date DEFAULT NULL,
  `edit_date` date DEFAULT NULL,
  `report_archived` date DEFAULT NULL,
  `passport_matching_date` datetime DEFAULT NULL,
  `remark` varchar(100) NOT NULL,
  `CDC_confirmation_result` varchar(10) NOT NULL,
  `medical_status` bit(1) NOT NULL,
  `signing_medical_examiner` varchar(5) NOT NULL,
  `signing_date` datetime DEFAULT NULL,
  PRIMARY KEY (`medical_record_number`),
  KEY `病歷號碼` (`medical_record_number`),
  KEY `檢查日期` (`examination_date`),
  KEY `護照` (`passport`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `package`
--

DROP TABLE IF EXISTS `package`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `package` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(255) DEFAULT NULL,
  `package_name` varchar(255) DEFAULT NULL,
  `item_code` varchar(255) DEFAULT NULL,
  `item` varchar(255) DEFAULT NULL,
  `chinese_name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `personnel_reading_record`
--

DROP TABLE IF EXISTS `personnel_reading_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personnel_reading_record` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_index` int NOT NULL,
  `Title` varchar(20) NOT NULL,
  `type` varchar(10) NOT NULL,
  `datetime` datetime DEFAULT NULL,
  `person` varchar(10) NOT NULL,
  `document` varchar(255) NOT NULL,
  `comment` varchar(255) NOT NULL,
  `sign_status` int NOT NULL,
  `process` int NOT NULL,
  `sign_datetime` datetime DEFAULT NULL,
  `applicant` varchar(5) NOT NULL,
  `read_datetime` datetime DEFAULT NULL,
  `sign` varchar(10) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `date` (`datetime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `photo_history`
--

DROP TABLE IF EXISTS `photo_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `photo_history` (
  `medical_record_number` varchar(255) NOT NULL,
  `photo_date` datetime DEFAULT NULL,
  `photographer` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `physician_interpretation`
--

DROP TABLE IF EXISTS `physician_interpretation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `physician_interpretation` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `location` varchar(255) DEFAULT NULL,
  `interpretation` varchar(255) DEFAULT NULL,
  `result_judgment` varchar(255) DEFAULT NULL,
  `data_display` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `判讀` (`interpretation`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `project_management`
--

DROP TABLE IF EXISTS `project_management`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_management` (
  `project_number` varchar(11) NOT NULL,
  `inspected_unit` varchar(10) NOT NULL,
  `set_name` varchar(10) NOT NULL,
  `item_code` varchar(4) NOT NULL,
  `item` varchar(50) NOT NULL,
  `remarks` varchar(200) NOT NULL,
  KEY `專案號碼` (`project_number`),
  KEY `項目代碼` (`item_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `project_numbers`
--

DROP TABLE IF EXISTS `project_numbers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `project_numbers` (
  `number` varchar(12) NOT NULL,
  `time` datetime NOT NULL,
  PRIMARY KEY (`number`),
  KEY `日期編號` (`number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `public_health_confirmation_record`
--

DROP TABLE IF EXISTS `public_health_confirmation_record`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `public_health_confirmation_record` (
  `medical_record_number` varchar(20) NOT NULL,
  `english_name` varchar(30) NOT NULL,
  `chinese_name` varchar(10) NOT NULL,
  `test_result` varchar(255) NOT NULL,
  `notification_date` date DEFAULT NULL,
  `original_specimen_collection_date` date DEFAULT NULL,
  `retest_first_date` date DEFAULT NULL,
  `retest_first_result` varchar(10) NOT NULL,
  `retest_second_date` date DEFAULT NULL,
  `retest_second_result` varchar(10) NOT NULL,
  `retest_third_date` date DEFAULT NULL,
  `retest_third_result` varchar(10) NOT NULL,
  `result_summary` varchar(10) NOT NULL,
  `remark` varchar(50) NOT NULL,
  `is_received` varchar(2) NOT NULL DEFAULT '否',
  `is_concluded` varchar(2) NOT NULL DEFAULT '否',
  `reviewer` varchar(5) NOT NULL,
  `review_time` date DEFAULT NULL,
  PRIMARY KEY (`medical_record_number`),
  KEY `病歷號碼` (`medical_record_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `report_correction`
--

DROP TABLE IF EXISTS `report_correction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `report_correction` (
  `id` int NOT NULL AUTO_INCREMENT,
  `medical_record_number` varchar(11) NOT NULL,
  `english_name` varchar(20) NOT NULL,
  `chinese_name` varchar(10) NOT NULL,
  `nationality` varchar(1) NOT NULL,
  `gender` varchar(1) NOT NULL,
  `category` varchar(10) NOT NULL,
  `original_report_result` varchar(20) NOT NULL,
  `corrected_report_result` varchar(20) NOT NULL,
  `modification_reason` varchar(20) NOT NULL,
  `original_reporter` varchar(5) NOT NULL,
  `corrected_reporter` varchar(5) NOT NULL,
  `original_report_date` datetime NOT NULL,
  `corrected_report_date` datetime NOT NULL,
  `reviewer` varchar(5) NOT NULL,
  `review_date` date DEFAULT NULL,
  `remark` varchar(20) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `search`
--

DROP TABLE IF EXISTS `search`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `search` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `item` varchar(50) NOT NULL,
  `option1` varchar(255) NOT NULL,
  `option2` varchar(255) NOT NULL,
  `option3` varchar(255) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shortcut_keys`
--

DROP TABLE IF EXISTS `shortcut_keys`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `shortcut_keys` (
  `item` varchar(255) DEFAULT NULL,
  `option` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `special_operations`
--

DROP TABLE IF EXISTS `special_operations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `special_operations` (
  `project_number` varchar(11) NOT NULL,
  `inspected_unit` varchar(10) NOT NULL,
  `operation_type` varchar(10) NOT NULL,
  `expense_type` varchar(10) NOT NULL,
  `package_price` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `special_operations_management`
--

DROP TABLE IF EXISTS `special_operations_management`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `special_operations_management` (
  `special_operation_code` varchar(255) DEFAULT NULL,
  `special_operation_name` varchar(255) DEFAULT NULL,
  `item_code` varchar(255) DEFAULT NULL,
  `item` varchar(255) DEFAULT NULL,
  `chinese_name` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `special_operations_package`
--

DROP TABLE IF EXISTS `special_operations_package`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `special_operations_package` (
  `id` int NOT NULL AUTO_INCREMENT,
  `project_number` varchar(11) NOT NULL,
  `inspected_unit` varchar(10) NOT NULL,
  `package_name` varchar(20) NOT NULL,
  `expense_type` varchar(10) NOT NULL,
  `package_price` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `stock_in_details`
--

DROP TABLE IF EXISTS `stock_in_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stock_in_details` (
  `batch_number` varchar(255) NOT NULL,
  `reagent_name` varchar(20) NOT NULL,
  `reagent_number` varchar(10) NOT NULL,
  `quantity` varchar(2) NOT NULL,
  `expiration_date` date DEFAULT NULL,
  `executor` varchar(10) NOT NULL,
  `summary` varchar(5) NOT NULL,
  `execution_date` datetime DEFAULT NULL,
  `acceptance_status` varchar(4) NOT NULL DEFAULT '尚未驗收',
  `supervisor_review` varchar(10) NOT NULL,
  `review_date` date DEFAULT NULL,
  PRIMARY KEY (`batch_number`),
  KEY `批次號碼` (`batch_number`),
  KEY `試劑品名` (`reagent_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `stock_out_details`
--

DROP TABLE IF EXISTS `stock_out_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stock_out_details` (
  `id` int NOT NULL AUTO_INCREMENT,
  `batch_number` varchar(20) NOT NULL,
  `reagent_name` varchar(20) NOT NULL,
  `reagent_lot_number` varchar(20) NOT NULL,
  `quantity` varchar(2) NOT NULL,
  `reagent_expiry` date DEFAULT NULL,
  `executor` varchar(10) NOT NULL,
  `summary` varchar(10) NOT NULL,
  `execution_date` datetime DEFAULT NULL,
  `supervisor_review` varchar(10) NOT NULL,
  `review_date` date DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_batch_number` (`batch_number`),
  KEY `idx_reagent_name` (`reagent_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `t`
--

DROP TABLE IF EXISTS `t`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t` (
  `userName` varchar(100) DEFAULT NULL,
  `Price` int DEFAULT NULL,
  `Dt` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `寄生蟲管理`
--

DROP TABLE IF EXISTS `寄生蟲管理`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `寄生蟲管理` (
  `編號` int NOT NULL AUTO_INCREMENT,
  `類別` varchar(5) NOT NULL,
  `學名` varchar(50) DEFAULT NULL,
  `判讀` varchar(10) DEFAULT NULL,
  `備註` varchar(10) DEFAULT NULL,
  `不合格項目代碼` varchar(3) NOT NULL,
  PRIMARY KEY (`編號`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `寄生蟲陽性`
--

DROP TABLE IF EXISTS `寄生蟲陽性`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `寄生蟲陽性` (
  `編號` int NOT NULL AUTO_INCREMENT,
  `學名` varchar(50) DEFAULT NULL,
  `判讀` varchar(10) DEFAULT NULL,
  `備註` varchar(10) DEFAULT NULL,
  `不合格項目代碼` varchar(3) NOT NULL,
  PRIMARY KEY (`編號`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `梅毒血清品管`
--

DROP TABLE IF EXISTS `梅毒血清品管`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `梅毒血清品管` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `日期` date DEFAULT NULL,
  `STS陰性` varchar(255) DEFAULT NULL,
  `STS陽性` varchar(255) DEFAULT NULL,
  `STS新開封陰性` varchar(255) DEFAULT NULL,
  `STS新開封陽性` varchar(255) DEFAULT NULL,
  `TPPA陰性` varchar(255) DEFAULT NULL,
  `TPPA陽性` varchar(255) DEFAULT NULL,
  `TPPA新開封陰性` varchar(255) DEFAULT NULL,
  `TPPA新開封陽性` varchar(255) DEFAULT NULL,
  `執行醫檢師` varchar(255) DEFAULT NULL,
  `審核者` varchar(255) DEFAULT NULL,
  `審核日期` date DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `測試`
--

DROP TABLE IF EXISTS `測試`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `測試` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `單位` varchar(10) NOT NULL,
  `人數` int NOT NULL,
  `開始時間` datetime DEFAULT NULL,
  `結束時間` datetime DEFAULT NULL,
  `顏色` varchar(20) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `送檢清單`
--

DROP TABLE IF EXISTS `送檢清單`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `送檢清單` (
  `收件日期` date NOT NULL,
  `編號` varchar(10) NOT NULL,
  `診所病歷` varchar(20) NOT NULL,
  `身分證號` varchar(12) NOT NULL,
  `姓名` varchar(20) NOT NULL,
  `來源名稱` varchar(5) NOT NULL,
  `性別` varchar(2) NOT NULL,
  `年齡` varchar(3) NOT NULL,
  `AC-Sug` varchar(10) NOT NULL,
  `Cre` varchar(10) NOT NULL,
  `BUN` varchar(10) NOT NULL,
  `UA` varchar(10) NOT NULL,
  `GOT` varchar(10) NOT NULL,
  `GPT` varchar(10) NOT NULL,
  `GGT` varchar(10) NOT NULL,
  `ALP` varchar(10) NOT NULL,
  `TP` varchar(10) NOT NULL,
  `ALB` varchar(10) NOT NULL,
  `GLO` varchar(10) NOT NULL,
  `A/G` varchar(10) NOT NULL,
  `T-B` varchar(10) NOT NULL,
  `D-B` varchar(10) NOT NULL,
  `TG` varchar(10) NOT NULL,
  `TC` varchar(10) NOT NULL,
  `HDL -C` varchar(10) NOT NULL,
  `LDL -C` varchar(10) NOT NULL,
  `CPK` varchar(10) NOT NULL,
  `LDH` varchar(10) NOT NULL,
  `AMY` varchar(10) NOT NULL,
  `C/HDL` varchar(10) NOT NULL,
  `BG` varchar(10) NOT NULL,
  `Rh` varchar(10) NOT NULL,
  `W.B.C` varchar(10) NOT NULL,
  `RBC/M` varchar(10) NOT NULL,
  `HgB/M` varchar(10) NOT NULL,
  `Hct/M` varchar(10) NOT NULL,
  `MCV` varchar(10) NOT NULL,
  `MCH` varchar(10) NOT NULL,
  `MCHC` varchar(10) NOT NULL,
  `Plt[K]` varchar(10) NOT NULL,
  `MPV` varchar(10) NOT NULL,
  `RDW` varchar(10) NOT NULL,
  `P-LCR` varchar(10) NOT NULL,
  `PDW` varchar(10) NOT NULL,
  `Net-s%` varchar(10) NOT NULL,
  `Lym-L%` varchar(10) NOT NULL,
  `Mono%` varchar(10) NOT NULL,
  `Eosin%` varchar(10) NOT NULL,
  `Baso%` varchar(10) NOT NULL,
  `Net -C` varchar(10) NOT NULL,
  `Lym -C` varchar(10) NOT NULL,
  `Mono-C` varchar(10) NOT NULL,
  `Eosi-C` varchar(10) NOT NULL,
  `Baso-C` varchar(10) NOT NULL,
  `eGFR` varchar(10) NOT NULL,
  `AFP` varchar(10) NOT NULL,
  `CEA` varchar(10) NOT NULL,
  `T-3` varchar(10) NOT NULL,
  `T-4` varchar(10) NOT NULL,
  `TSH` varchar(10) NOT NULL,
  `HBA1C` varchar(10) NOT NULL,
  `HBsAg` varchar(10) NOT NULL,
  `HBsAb` varchar(10) NOT NULL,
  `HCV-Ab` varchar(10) NOT NULL,
  `HAV-M` varchar(10) NOT NULL,
  `HAV-G` varchar(10) NOT NULL,
  `SALA` varchar(10) NOT NULL,
  `SALB` varchar(10) NOT NULL,
  `SALO` varchar(10) NOT NULL,
  `SALOH` varchar(10) NOT NULL,
  `CKMB` varchar(10) NOT NULL,
  `RA` varchar(10) NOT NULL,
  `MeaslG` varchar(10) NOT NULL,
  `Rb-Ab` varchar(10) NOT NULL,
  `梅毒檢查RPR` varchar(10) NOT NULL,
  `TPPA` varchar(10) NOT NULL,
  `Par(MIF)` varchar(10) NOT NULL,
  `C-RP` varchar(10) NOT NULL,
  `Heli-P` varchar(10) NOT NULL,
  `Homocy` varchar(10) NOT NULL,
  `Na` varchar(10) NOT NULL,
  `K` varchar(10) NOT NULL,
  `Cl` varchar(10) NOT NULL,
  `CA` varchar(10) NOT NULL,
  `P` varchar(10) NOT NULL,
  `Mg` varchar(10) NOT NULL,
  `HBeAg` varchar(10) NOT NULL,
  `HBeAb` varchar(10) NOT NULL,
  `HBcAb` varchar(10) NOT NULL,
  `PSA` int NOT NULL,
  `FPSA` int NOT NULL,
  `NSE` int NOT NULL,
  `PAP` int NOT NULL,
  `CA-125` int NOT NULL,
  `CA-199` int NOT NULL,
  `CA-724` int NOT NULL,
  `C21-1` int NOT NULL,
  `B-HCG` int NOT NULL,
  `CA-153` int NOT NULL,
  `EB IgA` int NOT NULL,
  `SCC` int NOT NULL,
  `LH` int NOT NULL,
  `FSH` int NOT NULL,
  `Thyroglobulin` int NOT NULL,
  `Thyroglobulin Ab` int NOT NULL,
  `E2` int NOT NULL,
  `AMH` int NOT NULL,
  `PGTR` int NOT NULL,
  `Testo` int NOT NULL,
  `Free Testo` int NOT NULL,
  `維生素D3` int NOT NULL,
  `Insulin` int NOT NULL,
  `EBIgG` int NOT NULL,
  `HTLV` int NOT NULL,
  `HIV` int NOT NULL,
  `ANA` int NOT NULL,
  `U-RBC` int NOT NULL,
  `U-WBC` int NOT NULL,
  `U-EP` int NOT NULL,
  `U-CAS` int NOT NULL,
  `U-CRY` int NOT NULL,
  `U-PUS` int NOT NULL,
  `Bacteria` int NOT NULL,
  `AMEOBA` int NOT NULL,
  `傷寒糞便檢查` varchar(10) NOT NULL,
  `副傷寒糞便檢查` varchar(10) NOT NULL,
  `STOOL CUL` varchar(10) NOT NULL,
  `Shigella` int NOT NULL,
  `FOBT` int NOT NULL,
  `Alcohol` int NOT NULL,
  `VZVIgG` int NOT NULL,
  `FreeT4` int NOT NULL,
  `Pb` int NOT NULL,
  `AMPHET` varchar(10) NOT NULL,
  `Morphi` varchar(10) NOT NULL,
  `大麻` int NOT NULL,
  `FM2` int NOT NULL,
  `K他命` int NOT NULL,
  `海洛因HEROIN` int NOT NULL,
  `COCAIN` int NOT NULL,
  `IgE免疫球蛋白E` int NOT NULL,
  `β2微球蛋白` int NOT NULL,
  `鐵蛋白` int NOT NULL,
  `膀胱癌` int NOT NULL,
  `MumpslG` int NOT NULL,
  `Troponin-I` int NOT NULL,
  `Troponin-T` int NOT NULL,
  `ChyIgM` int NOT NULL,
  `CMV-m` int NOT NULL,
  `TPHA` int NOT NULL,
  `Ni` int NOT NULL,
  `iAS` int NOT NULL,
  `AS3+` int NOT NULL,
  `AS5+` int NOT NULL,
  `DMA` int NOT NULL,
  `MMA` int NOT NULL,
  `iAS/CRE` int NOT NULL,
  `As3+/CRE` int NOT NULL,
  `UTP` int NOT NULL,
  `UPCR` int NOT NULL,
  `UCRE` int NOT NULL,
  `Cr` int NOT NULL,
  `U-Cr/CRE` int NOT NULL,
  `尿中鎘` int NOT NULL,
  `尿中鎘/CRE` int NOT NULL,
  `德國痲疹IgM抗體Rb-IgM` int NOT NULL,
  `高敏感度蛋白` int NOT NULL,
  `CRP(定量)` int NOT NULL,
  `VLDL` int NOT NULL,
  `LDL/HDL比值` int NOT NULL,
  `過敏原55項` int NOT NULL,
  `血色素-H` int NOT NULL,
  `血色素-BART` int NOT NULL,
  `血色素-A1` int NOT NULL,
  `血色素-F` int NOT NULL,
  `血色素-S` int NOT NULL,
  `血色素-A2` int NOT NULL,
  `血色素-碳化物` int NOT NULL,
  `披衣菌抗體IgA` int NOT NULL,
  `披衣菌抗體IgG` int NOT NULL,
  `尿中鎳` int NOT NULL,
  `尿中鎳/CRE` int NOT NULL,
  `HSV II IgG` int NOT NULL,
  `HSV II IgM` int NOT NULL,
  `血中汞` int NOT NULL,
  `尿中汞` int NOT NULL,
  `血清銦` int NOT NULL,
  `血清膽鹼脂脢CHE` int NOT NULL,
  `懷孕試驗` int NOT NULL,
  `ACLM` int NOT NULL,
  `ACLG` int NOT NULL,
  `B2GPIG` int NOT NULL,
  `FE` int NOT NULL,
  `UIBC` int NOT NULL,
  `TIBC` int NOT NULL,
  `碳13` int NOT NULL,
  `HBV PCR` int NOT NULL,
  `HLA-B27` int NOT NULL,
  `骨鈣素` int NOT NULL,
  `備註` int NOT NULL,
  `登革熱抗原快篩` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2023-06-29  0:18:32
