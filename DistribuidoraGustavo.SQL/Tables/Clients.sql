CREATE TABLE [dbo].[Clients]
(
    [ClientId]      INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Name]          VARCHAR(MAX) NOT NULL,
    [InvoicePrefix] VARCHAR(255) NOT NULL
)
