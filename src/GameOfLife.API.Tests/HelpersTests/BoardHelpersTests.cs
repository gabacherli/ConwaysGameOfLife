using GameOfLife.API.Helpers;
using GameOfLife.API.Models;
using System.Security.Cryptography;

namespace GameOfLife.API.Tests.HelpersTests
{
    public class BoardHelpersTests
    {
        [Fact]
        public void ConvertToBinary_ShouldConvertCorrectly()
        {
            // Arrange
            var state = new List<List<bool>>
            {
                new() { true, false, true },
                new() { false, true, false }
            };
            var expectedBinary = new byte[] { 1, 0, 1, 0, 1, 0 };

            // Act
            var result = BoardHelper.ConvertToBinary(state);

            // Assert
            Assert.Equal(expectedBinary, result);
        }

        [Fact]
        public void ComputeStateHash_ShouldComputeSHA256Hash()
        {
            // Arrange
            var data = new byte[] { 1, 0, 1, 0, 1, 0 };

            // Act
            var hashResult = BoardHelper.ComputeStateHash(data);
            var expectedHash = SHA256.HashData(data);

            // Assert
            Assert.Equal(expectedHash, hashResult);
        }

        [Theory]
        [MemberData(nameof(BoardStateData))]
        public void GetNextTick_ShouldComputeCorrectNextState(List<List<bool>> initialState, int rows, int cols, List<List<bool>> expectedNextState)
        {
            // Arrange
            var board = new Board { Rows = rows, Columns = cols, State = initialState };

            var expectedNextStateHash = BoardHelper.ComputeStateHash(BoardHelper.ConvertToBinary(expectedNextState));

            // Act
            var result = BoardHelper.GetNextTick(board, board.Rows, board.Columns, out var resultStateHash);

            // Assert
            Assert.Equal(expectedNextState, result);
            Assert.Equal(expectedNextStateHash, resultStateHash);
        }

        [Theory]
        [MemberData(nameof(NeighborCountData))]
        public void CountLiveNeighbors_ShouldReturnCorrectCount(List<List<bool>> state, int row, int col, int rows, int cols, int expectedCount)
        {
            // Act
            var actualCount = BoardHelper.CountLiveNeighbors(state, row, col, rows, cols);

            // Assert
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void ConvertFromBinary_ShouldConvertCorrectly()
        {
            // Arrange
            byte[] binaryState = { 1, 0, 1, 0, 1, 0 };
            var expectedState = new List<List<bool>>
            {
                new() { true, false, true },
                new() { false, true, false }
            };

            // Act
            var result = BoardHelper.ConvertFromBinary(binaryState, 2, 3);

            // Assert
            Assert.Equal(expectedState, result);
        }

        public static IEnumerable<object[]> BoardStateData()
        {
            var gliderPattern_10x10 =
                new List<List<bool>>
                {
                    new() { false, false, false, false, false, false, false, false, false, false },
                    new() { false, false, false, false, false, false, false, false, false, false },
                    new() { false, false, true,  false, false, false, false, false, false, false },
                    new() { false, false, false, true,  false, false, false, false, false, false },
                    new() { false, true,  true,  true,  false, false, false, false, false, false },
                    new() { false, false, false, false, false, false, false, false, false, false },
                    new() { false, false, false, false, false, false, false, false, false, false },
                    new() { false, false, false, false, false, false, false, false, false, false },
                    new() { false, false, false, false, false, false, false, false, false, false },
                    new() { false, false, false, false, false, false, false, false, false, false }
                };

            var random_50x50 = GenerateRandomBoard(50, 50, 0.5);
            var checkerboard_100x100 = GenerateCheckerboardBoard(100, 100);
            var allAlive_200x200 = GenerateAllAliveBoard(200, 200);

            yield return new object[]
            {
                // 1x1 board, single dead cell should remain dead.
                new List<List<bool>> { new() { false } },
                1, 1,
                new List<List<bool>> { new() { false } }
            };

            yield return new object[]
            {
                // 10x10 Glider pattern
                gliderPattern_10x10,
                10, 10,
                BoardHelper.GetNextTick(new Board { Rows = 10, Columns = 10, State = gliderPattern_10x10 }, 10, 10, out _)
            };

            yield return new object[]
            {
                // 50x50 random board
                random_50x50,
                50, 50,
                BoardHelper.GetNextTick(new Board { Rows = 50, Columns = 50, State = random_50x50 }, 50, 50, out _)
            };

            yield return new object[]
            {
                // 100x100 Checkerboard pattern
                checkerboard_100x100,
                100, 100,
                BoardHelper.GetNextTick(new Board { Rows = 100, Columns = 100, State = checkerboard_100x100 }, 100, 100, out _)
            };

            yield return new object[]
            {
                // 200x200 All Alive Board
                allAlive_200x200,
                200, 200,
                BoardHelper.GetNextTick(new Board { Rows = 200, Columns = 200, State = allAlive_200x200 }, 200, 200, out _)
            };
        }

        public static IEnumerable<object[]> NeighborCountData()
        {
            yield return new object[]
                {
                // 3x3 board, center cell, should have 8 neighbors
                new List<List<bool>>
                {
                    new() { true, true, true },
                    new() { true, true, true },
                    new() { true, true, true }
                },
                1, 1, 3, 3, 8
            };

            yield return new object[]
            {
                // 5x5 board, corner cell (0,0), should have 3 neighbors
                new List<List<bool>>
                {
                    new() { true, true, false, false, false },
                    new() { true, true, false, false, false },
                    new() { false, false, false, false, false },
                    new() { false, false, false, false, false },
                    new() { false, false, false, false, false }
                },
                0, 0, 5, 5, 3
            };
        }

        private static List<List<bool>> GenerateRandomBoard(int rows, int cols, double fillProbability)
        {
            var random = new Random();
            var board = new List<List<bool>>();

            for (int i = 0; i < rows; i++)
            {
                var row = new List<bool>();
                for (int j = 0; j < cols; j++)
                {
                    row.Add(random.NextDouble() < fillProbability);
                }
                board.Add(row);
            }
            return board;
        }

        private static List<List<bool>> GenerateCheckerboardBoard(int rows, int cols)
        {
            var board = new List<List<bool>>();

            for (int i = 0; i < rows; i++)
            {
                var row = new List<bool>();
                for (int j = 0; j < cols; j++)
                {
                    row.Add((i + j) % 2 == 0);
                }
                board.Add(row);
            }
            return board;
        }

        private static List<List<bool>> GenerateAllAliveBoard(int rows, int cols)
        {
            return Enumerable.Range(0, rows).Select(_ => Enumerable.Repeat(true, cols).ToList()).ToList();
        }
    }
}
