using GameOfLife.API.Helpers;
using GameOfLife.API.Models;
using System.Security.Cryptography;

namespace GameOfLife.API.Tests.HelpersTests
{
    public class BoardHelperTests
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
        public void GetNextIteration_ShouldComputeCorrectNextState(List<List<bool>> initialState, int rows, int columns, List<List<bool>> expectedNextState)
        {
            // Arrange
            var board = new Board { Rows = rows, Columns = columns, State = initialState };

            var expectedNextStateHash = BoardHelper.ComputeStateHash(BoardHelper.ConvertToBinary(expectedNextState));

            // Act
            var result = BoardHelper.GetNextIteration(board, out var resultStateHash);

            // Assert
            Assert.Equal(expectedNextState, result);
            Assert.Equal(expectedNextStateHash, resultStateHash);
        }

        [Theory]
        [MemberData(nameof(NeighborCountData))]
        public void CountLiveNeighbors_ShouldReturnCorrectCount(List<List<bool>> state, int row, int col, int rows, int columns, int expectedCount)
        {
            // Act
            var actualCount = BoardHelper.CountLiveNeighbors(state, row, col, rows, columns);

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

        [Theory]
        [MemberData(nameof(BoardAfterIterationsData))]
        public void GetBoardAfterNIterations_ShouldComputeCorrectFinalState(List<List<bool>> initialState, int rows, int columns, int iterations, List<List<bool>> expectedFinalState)
        {
            // Arrange
            var board = new Board { Rows = rows, Columns = columns, State = initialState };

            var expectedFinalStateHash = BoardHelper.ComputeStateHash(BoardHelper.ConvertToBinary(expectedFinalState));

            // Act
            var finalState = BoardHelper.GetBoardAfterNIterations(board, iterations, out var resultStateHash);

            // Assert
            Assert.Equal(expectedFinalState, finalState);
            Assert.Equal(expectedFinalStateHash, resultStateHash);
        }

        [Theory]
        [MemberData(nameof(StableOrFinalIterationData))]
        public void GetStableOrFinalIteration_ShouldDetectEndCondition(List<List<bool>> initialState, int rows, int columns, int maxIterations, EndReason expectedEndReason, int expectedIterations)
        {
            // Arrange
            var board = new Board { Rows = rows, Columns = columns, State = initialState };

            // Act
            var (finalState, iterationCount) = BoardHelper.GetStableOrFinalIteration(board, maxIterations, out _, out var endReason);

            // Assert
            Assert.NotNull(finalState);
            Assert.Equal(expectedEndReason, endReason);
            Assert.Equal(expectedIterations, iterationCount);
        }

        public static IEnumerable<object[]> BoardStateData()
        {
            var gliderPattern_10x10 = Generate10x10GliderPattern();
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
                BoardHelper.GetNextIteration(new Board { Rows = 10, Columns = 10, State = gliderPattern_10x10 }, out _)
            };

            yield return new object[]
            {
                // 50x50 random board
                random_50x50,
                50, 50,
                BoardHelper.GetNextIteration(new Board { Rows = 50, Columns = 50, State = random_50x50 }, out _)
            };

            yield return new object[]
            {
                // 100x100 Checkerboard pattern
                checkerboard_100x100,
                100, 100,
                BoardHelper.GetNextIteration(new Board { Rows = 100, Columns = 100, State = checkerboard_100x100 }, out _)
            };

            yield return new object[]
            {
                // 200x200 All Alive Board
                allAlive_200x200,
                200, 200,
                BoardHelper.GetNextIteration(new Board { Rows = 200, Columns = 200, State = allAlive_200x200 }, out _)
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

        public static IEnumerable<object[]> BoardAfterIterationsData()
        {
            List<List<bool>> gliderPattern_10x10 = Generate10x10GliderPattern();

            var random_50x50 = GenerateRandomBoard(50, 50, 0.5);
            var checkerboard_100x100 = GenerateCheckerboardBoard(100, 100);
            var allAlive_200x200 = GenerateAllAliveBoard(200, 200);

            yield return new object[]
            {
                // 1x1 board, single dead cell should remain dead after any iterations
                new List<List<bool>> { new() { false } },
                1, 1, 5, // 5 iterations
                new List<List<bool>> { new() { false } }
            };

            yield return new object[]
            {
                // 10x10 Glider pattern after 5 iterations
                gliderPattern_10x10,
                10, 10, 5,
                BoardHelper.GetBoardAfterNIterations(new Board { Rows = 10, Columns = 10, State = gliderPattern_10x10 }, 5, out _)
            };

            yield return new object[]
            {
                // 50x50 random board after 10 iterations
                random_50x50,
                50, 50, 10,
                BoardHelper.GetBoardAfterNIterations(new Board { Rows = 50, Columns = 50, State = random_50x50 }, 10, out _)
            };

            yield return new object[]
            {
                // 100x100 Checkerboard pattern after 2 iterations
                checkerboard_100x100,
                100, 100, 2,
                BoardHelper.GetBoardAfterNIterations(new Board { Rows = 100, Columns = 100, State = checkerboard_100x100 }, 2, out _)
            };

            yield return new object[]
            {
                // 200x200 All Alive Board after 3 iterations
                allAlive_200x200,
                200, 200, 3,
                BoardHelper.GetBoardAfterNIterations(new Board { Rows = 200, Columns = 200, State = allAlive_200x200 }, 3, out _)
            };
        }

        public static IEnumerable<object[]> StableOrFinalIterationData()
        {
            var gliderPattern_10x10 = Generate10x10GliderPattern();
            var loop_oscillator_4x4 = GenerateOscillatorPattern(4, 4);
            var loop_pattern_6x6 = GenerateLoopPattern(6, 6);
            var random_10000x10000 = GenerateRandomBoard(10000, 10000, 0.5);

            yield return new object[]
            {
                // Looping board: Detects a loop and stops early
                loop_oscillator_4x4, 4, 4, 10, EndReason.Loop, 2
            };

            yield return new object[]
            {
                // Looping board: Detects a loop and stops early
                loop_pattern_6x6, 6, 6, 20, EndReason.Loop, 2
            };

            yield return new object[]
            {
                // 10x10 Glider pattern: Stops at max iterations if no stability or loops
                gliderPattern_10x10, 10, 10, 50, EndReason.Stable, 23
            };

            yield return new object[]
            {
                // 10000x10000 random board: Should reach max iterations since it's unpredictable
                random_10000x10000, 10000, 10000, 1000, EndReason.MaxIterationsReached, 1000
            };
        }

        private static List<List<bool>> Generate10x10GliderPattern()
        {
            return new List<List<bool>>
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
        }

        private static List<List<bool>> GenerateRandomBoard(int rows, int columns, double fillProbability)
        {
            var random = new Random();
            var board = new List<List<bool>>();

            for (int i = 0; i < rows; i++)
            {
                var row = new List<bool>();
                for (int j = 0; j < columns; j++)
                {
                    row.Add(random.NextDouble() < fillProbability);
                }
                board.Add(row);
            }
            return board;
        }

        private static List<List<bool>> GenerateOscillatorPattern(int rows, int columns)
        {
            var board = new List<List<bool>>();

            for (int i = 0; i < rows; i++)
            {
                var row = new List<bool>();
                for (int j = 0; j < columns; j++)
                {
                    // Simple "Blinker" oscillator: vertical → horizontal flip
                    row.Add((i == rows / 2) && (j >= columns / 2 - 1 && j <= columns / 2 + 1));
                }
                board.Add(row);
            }
            return board;
        }

        private static List<List<bool>> GenerateCheckerboardBoard(int rows, int columns)
        {
            var board = new List<List<bool>>();

            for (int i = 0; i < rows; i++)
            {
                var row = new List<bool>();
                for (int j = 0; j < columns; j++)
                {
                    row.Add((i + j) % 2 is 0);
                }
                board.Add(row);
            }
            return board;
        }

        private static List<List<bool>> GenerateLoopPattern(int rows, int columns)
        {
            var board = new List<List<bool>>();

            for (int i = 0; i < rows; i++)
            {
                var row = new List<bool>();
                for (int j = 0; j < columns; j++)
                {
                    // Small "Pulsar" or Traffic Light Oscillator (changes between 3 states)
                    row.Add((i == 2 && j == 1) || (i == 2 && j == 2) || (i == 2 && j == 3));
                }
                board.Add(row);
            }
            return board;
        }

        private static List<List<bool>> GenerateAllAliveBoard(int rows, int columns)
        {
            return Enumerable.Range(0, rows).Select(_ => Enumerable.Repeat(true, columns).ToList()).ToList();
        }
    }
}
