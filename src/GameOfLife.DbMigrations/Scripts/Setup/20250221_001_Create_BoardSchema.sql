SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.Boards', 'U') IS NOT NULL
    DROP TABLE dbo.Boards;

CREATE TABLE Boards (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Rows INT NOT NULL,
    Columns INT NOT NULL,
    State VARBINARY(MAX) NOT NULL,
    StateHash BINARY(32) NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE UNIQUE INDEX IX_Boards_StateHash ON Boards(StateHash);

GO
