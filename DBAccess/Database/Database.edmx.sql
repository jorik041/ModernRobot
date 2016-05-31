
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/31/2016 14:09:16
-- Generated from EDMX file: D:\Dropbox\MyOwn\RoboTrader\ModernRobot\ModernServer\DBAccess\Database\Database.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [stockinstruments];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'InstrumentsSet'
CREATE TABLE [dbo].[InstrumentsSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ItemsSet'
CREATE TABLE [dbo].[ItemsSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Ticker] nvarchar(max)  NOT NULL,
    [InstrumentCode] nvarchar(max)  NOT NULL,
    [MarketCode] nvarchar(max)  NOT NULL,
    [DateFrom] datetime  NOT NULL,
    [DateTo] datetime  NOT NULL,
    [InstrumentId] int  NOT NULL,
    [Period] tinyint  NOT NULL
);
GO

-- Creating table 'StockDataSet'
CREATE TABLE [dbo].[StockDataSet] (
    [Id] int  NOT NULL,
    [Open] real  NOT NULL,
    [High] real  NOT NULL,
    [Low] real  NOT NULL,
    [Close] real  NOT NULL,
    [Volume] real  NOT NULL,
    [ItemId] int  NOT NULL,
    [DateTimeStamp] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'InstrumentsSet'
ALTER TABLE [dbo].[InstrumentsSet]
ADD CONSTRAINT [PK_InstrumentsSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ItemsSet'
ALTER TABLE [dbo].[ItemsSet]
ADD CONSTRAINT [PK_ItemsSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StockDataSet'
ALTER TABLE [dbo].[StockDataSet]
ADD CONSTRAINT [PK_StockDataSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [InstrumentId] in table 'ItemsSet'
ALTER TABLE [dbo].[ItemsSet]
ADD CONSTRAINT [FK_InstrumentsItems]
    FOREIGN KEY ([InstrumentId])
    REFERENCES [dbo].[InstrumentsSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_InstrumentsItems'
CREATE INDEX [IX_FK_InstrumentsItems]
ON [dbo].[ItemsSet]
    ([InstrumentId]);
GO

-- Creating foreign key on [ItemId] in table 'StockDataSet'
ALTER TABLE [dbo].[StockDataSet]
ADD CONSTRAINT [FK_ItemsStockData]
    FOREIGN KEY ([ItemId])
    REFERENCES [dbo].[ItemsSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ItemsStockData'
CREATE INDEX [IX_FK_ItemsStockData]
ON [dbo].[StockDataSet]
    ([ItemId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------