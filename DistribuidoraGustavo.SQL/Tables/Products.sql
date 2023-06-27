CREATE TABLE [dbo].[Products]
(
    [ProductID]     INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Code]          VARCHAR(MAX) NOT NULL,
    [Name]          VARCHAR(MAX) NOT NULL,
    [Description]   VARCHAR(MAX),
    [Active]        BIT NOT NULL DEFAULT 1,
    [BasePrice]     DECIMAL(10,2) NOT NULL DEFAULT 0
)
