CREATE TABLE [dbo].[Invoices]
(
    [InvoiceId]         INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [InvoiceNumber]     VARCHAR(255) NOT NULL UNIQUE,
    [ClientID]          INT NOT NULL,
    [CreatedDate]       DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [Active]            BIT NOT NULL DEFAULT 0,

    CONSTRAINT [FK_Invoices_ClientID] FOREIGN KEY ([ClientID]) REFERENCES [Clients]([ClientID]) 
)
