CREATE OR ALTER PROCEDURE sp_InsertBoardState
    @Id UNIQUEIDENTIFIER OUTPUT,
    @Rows INT,
    @Columns INT,
    @State VARBINARY(MAX),
    @StateHash BINARY(32)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ExistingBoardId UNIQUEIDENTIFIER;

    -- Check if the board state already exists
    SELECT @ExistingBoardId = Id FROM Boards WHERE StateHash = @StateHash;

    IF @ExistingBoardId IS NOT NULL
    BEGIN
        -- Return existing Id
        SET @Id = @ExistingBoardId;
    END
    ELSE
    BEGIN
        BEGIN TRANSACTION;

        SET @Id = NEWID();

        -- Insert new board state
        INSERT INTO Boards (Id, Rows, Columns, State, StateHash)
        VALUES (@Id, @Rows, @Columns, @State, @StateHash);

        COMMIT TRANSACTION;
    END
END;