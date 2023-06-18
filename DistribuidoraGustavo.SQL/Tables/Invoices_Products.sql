CREATE TABLE [dbo].[Invoices_Products]
(
    [InvoicesProductsID]    INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProductID]             INT NOT NULL,
    [InvoiceID]             INT NOT NULL,
    [UnitPrice]             DECIMAL(10,2) NOT NULL,
    [Quantity]              INT NOT NULL,
    [Amount]                DECIMAL(10,2) NOT NULL

    CONSTRAINT [FK_Invoices_Products_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Products]([ProductID]), 
    CONSTRAINT [FK_Invoices_Products_InvoiceID] FOREIGN KEY ([InvoiceID]) REFERENCES [Invoices]([InvoiceID])
)
