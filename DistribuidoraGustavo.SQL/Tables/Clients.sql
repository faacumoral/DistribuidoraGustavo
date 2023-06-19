CREATE TABLE [dbo].[Clients]
(
    [ClientID]      INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Name]          VARCHAR(MAX) NOT NULL,
    [InvoicePrefix] VARCHAR(255) NOT NULL, 
    [DefaultPriceListID] INT NULL, 
    CONSTRAINT [FK_Clients_PriceListID] FOREIGN KEY ([DefaultPriceListID]) REFERENCES [PriceLists]([PriceListID])
)
