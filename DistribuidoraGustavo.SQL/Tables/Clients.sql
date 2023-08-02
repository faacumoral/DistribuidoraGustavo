CREATE TABLE [dbo].[Clients]
(
    [ClientID]      INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Name]          VARCHAR(MAX) NOT NULL,
    [InvoicePrefix] VARCHAR(255) NOT NULL, 
    [DefaultPriceListID] INT NULL, 
    [ActualBalance] DECIMAL(10,2) NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Clients_PriceListID] FOREIGN KEY ([DefaultPriceListID]) REFERENCES [PriceLists]([PriceListID])
)
