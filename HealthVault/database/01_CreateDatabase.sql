/*
    HealthVault — SQL Server
    Script 01: create the database
    Run in SSMS, Azure Data Studio, or any SQL Server workbench
    against a server you can administer (local, Express, or Azure SQL).
*/
IF DB_ID(N'HealthVaultDb') IS NULL
BEGIN
    CREATE DATABASE HealthVaultDb;
END
GO

ALTER DATABASE HealthVaultDb SET RECOVERY SIMPLE;
GO

USE HealthVaultDb;
GO
