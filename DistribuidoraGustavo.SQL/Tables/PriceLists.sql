CREATE TABLE [dbo].[PriceLists]
(
    [PriceListId] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [Name] VARCHAR(10) NULL, 
    [Percent] DECIMAL(10, 2) NULL 
)
