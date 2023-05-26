CREATE TABLE [dbo].[Users]
(
    [UserId]    INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Username]  VARCHAR(MAX) NOT NULL,
    [Name]      VARCHAR(MAX) NULL,
    [Password]  VARCHAR(MAX) NULL,
    [Active]    BIT NOT NULL DEFAULT 1
)
