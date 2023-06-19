CREATE TABLE [dbo].[Invoices]
(
    [InvoiceID]         INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [InvoiceNumber]     VARCHAR(255) NOT NULL UNIQUE,
    [ClientID]          INT NOT NULL,
    [CreatedDate]       DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [Active]            BIT NOT NULL DEFAULT 0,
    [PriceListID]       INT NULL, 
    [Description]       NVARCHAR(MAX) NULL, 
    CONSTRAINT [FK_Invoices_ClientID] FOREIGN KEY ([ClientID]) REFERENCES [Clients]([ClientID]), 
    CONSTRAINT [FK_Invoices_PriceListID] FOREIGN KEY ([PriceListID]) REFERENCES [PriceLists]([PriceListID]) 
)
