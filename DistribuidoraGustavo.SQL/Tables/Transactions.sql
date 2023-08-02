CREATE TABLE [dbo].[Transactions]
(
    [TransactionID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Amount] DECIMAL(10,2) NOT NULL,
    [Date] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [ClientID] INT NOT NULL,
    [Description] NVARCHAR(255) NULL,
    [Type] NVARCHAR(255) NOT NULL,
    CONSTRAINT [FK_Transaction_ClientID] FOREIGN KEY ([ClientID]) REFERENCES [Clients]([ClientID])
)
