CREATE TABLE [dbo].[ImportedFiles]
(
    [ImportedFileID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [FileName] NVARCHAR(MAX) NOT NULL,
    [DateTime] DATETIME NOT NULL,
    [FileContent] NVARCHAR(MAX) NOT NULL
)
