CREATE TABLE [dbo].[UserTokens]
(
    [UserTokenID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [UserID] INT NOT NULL,
    [ExpireTime] DATETIME NOT NULL,
    [Token] NVARCHAR(MAX) NOT NULL,
    [Data] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_UserTokens_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID])
)
