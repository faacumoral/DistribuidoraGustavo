-- Marca la migración inicial como aplicada en una base que ya tiene el esquema
-- (creada con el proyecto SSDT anterior), para que EF Core no intente crear las tablas.
-- Ejecutar UNA sola vez por base, antes del primer "dotnet ef database update".

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261008025713_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261008025713_InitialCreate', N'10.0.12');
END;

-- Índices sobre claves foráneas: la migración inicial los crea y el esquema anterior no los tenía.
-- Crearlos deja la base igual a lo que EF Core asume.

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Clients_DefaultPriceListID')
    CREATE INDEX [IX_Clients_DefaultPriceListID] ON [Clients] ([DefaultPriceListID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Invoices_ClientID')
    CREATE INDEX [IX_Invoices_ClientID] ON [Invoices] ([ClientID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Invoices_PriceListID')
    CREATE INDEX [IX_Invoices_PriceListID] ON [Invoices] ([PriceListID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Invoices_Products_InvoiceID')
    CREATE INDEX [IX_Invoices_Products_InvoiceID] ON [Invoices_Products] ([InvoiceID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Invoices_Products_ProductID')
    CREATE INDEX [IX_Invoices_Products_ProductID] ON [Invoices_Products] ([ProductID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_PriceLists_PriceListID')
    CREATE INDEX [IX_Products_PriceLists_PriceListID] ON [Products_PriceLists] ([PriceListID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_PriceLists_ProductID')
    CREATE INDEX [IX_Products_PriceLists_ProductID] ON [Products_PriceLists] ([ProductID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Transactions_ClientID')
    CREATE INDEX [IX_Transactions_ClientID] ON [Transactions] ([ClientID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserTokens_UserID')
    CREATE INDEX [IX_UserTokens_UserID] ON [UserTokens] ([UserID]);
