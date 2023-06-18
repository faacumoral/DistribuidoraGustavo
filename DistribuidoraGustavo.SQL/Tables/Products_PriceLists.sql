CREATE TABLE [dbo].[Products_PriceLists]
(
    [ProductPriceListID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ProductID] INT NOT NULL,
    [PriceListID]   INT NOT NULL,
    [Price] DECIMAL(10,2) NOT NULL, 
    CONSTRAINT [FK_Products_PriceLists_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [Products]([ProductID]), 
    CONSTRAINT [FK_Products_PriceLists_PriceListID] FOREIGN KEY ([PriceListID]) REFERENCES [PriceLists]([PriceListID])
)
