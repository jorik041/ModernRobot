



-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 05/25/2016 11:49:12
-- Generated from EDMX file: D:\Dropbox\MyOwn\RoboTrader\ModernRobot\ModernServer\DBAccess\Database\Database.edmx
-- Target version: 3.0.0.0
-- --------------------------------------------------

DROP DATABASE IF EXISTS `stockinstruments`;
CREATE DATABASE `stockinstruments`;
USE `stockinstruments`;

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------

--    ALTER TABLE `ItemsSet` DROP CONSTRAINT `FK_InstrumentsItems`;
--    ALTER TABLE `StockDataSet` DROP CONSTRAINT `FK_ItemsStockData`;

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;
    DROP TABLE IF EXISTS `InstrumentsSet`;
    DROP TABLE IF EXISTS `ItemsSet`;
    DROP TABLE IF EXISTS `StockDataSet`;
SET foreign_key_checks = 1;

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

CREATE TABLE `InstrumentsSet`(
	`Id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` longtext NOT NULL);

ALTER TABLE `InstrumentsSet` ADD PRIMARY KEY (Id);




CREATE TABLE `ItemsSet`(
	`Id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Ticker` longtext NOT NULL, 
	`InstrumentCode` longtext NOT NULL, 
	`MarketCode` longtext NOT NULL, 
	`DateFrom` datetime NOT NULL, 
	`DateTo` datetime NOT NULL, 
	`InstrumentsId` int NOT NULL, 
	`Period` TINYINT UNSIGNED NOT NULL);

ALTER TABLE `ItemsSet` ADD PRIMARY KEY (Id);




CREATE TABLE `StockDataSet`(
	`DateTimeStamp` datetime NOT NULL, 
	`Open` float NOT NULL, 
	`High` float NOT NULL, 
	`Low` float NOT NULL, 
	`Close` float NOT NULL, 
	`Volume` float NOT NULL, 
	`ItemsId` int NOT NULL);

ALTER TABLE `StockDataSet` ADD PRIMARY KEY (DateTimeStamp);






-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on `InstrumentsId` in table 'ItemsSet'

ALTER TABLE `ItemsSet`
ADD CONSTRAINT `FK_InstrumentsItems`
    FOREIGN KEY (`InstrumentsId`)
    REFERENCES `InstrumentsSet`
        (`Id`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InstrumentsItems'

CREATE INDEX `IX_FK_InstrumentsItems` 
    ON `ItemsSet`
    (`InstrumentsId`);

-- Creating foreign key on `ItemsId` in table 'StockDataSet'

ALTER TABLE `StockDataSet`
ADD CONSTRAINT `FK_ItemsStockData`
    FOREIGN KEY (`ItemsId`)
    REFERENCES `ItemsSet`
        (`Id`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ItemsStockData'

CREATE INDEX `IX_FK_ItemsStockData` 
    ON `StockDataSet`
    (`ItemsId`);

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
