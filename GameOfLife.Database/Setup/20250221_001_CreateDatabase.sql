IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'GameOfLife')
BEGIN
    CREATE DATABASE GameOfLife;
END;
GO