CREATE OR ALTER PROCEDURE sp_InsertBoardState
    @Width INT,
    @Height INT,
    @State VARBINARY(MAX),
    @StateHash BINARY(32),
    @BoardId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ExistingBoardId UNIQUEIDENTIFIER;

    -- Check if the board state already exists
    SELECT @ExistingBoardId = BoardId FROM Boards WHERE StateHash = @StateHash;

    IF @ExistingBoardId IS NOT NULL
    BEGIN
        -- Return existing BoardId
        SET @BoardId = @ExistingBoardId;
    END
    ELSE
    BEGIN
        -- Insert new board state
        INSERT INTO Boards (Width, Height, State, StateHash)
        VALUES (@Width, @Height, @State, @StateHash);
        
        SET @BoardId = NEWID();
    END
END;