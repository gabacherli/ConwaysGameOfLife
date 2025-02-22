using GameOfLife.API.Models;
using System.ComponentModel.DataAnnotations;

namespace GameOfLife.API.Tests.ModelsTests
{
    public class BoardTests
    {
        [Fact]
        public void Board_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var board = new Board();

            // Assert
            Assert.NotNull(board.State);
            Assert.Empty(board.State);
        }

        [Fact]
        public void Board_ShouldGenerateCorrectStateBinary()
        {
            // Arrange
            var board = new Board
            {
                Rows = 2,
                Columns = 3,
                State = new List<List<bool>>
                {
                    new() { true, false, true },
                    new() { false, true, false }
                }
            };
            var expectedBinary = new byte[] { 1, 0, 1, 0, 1, 0 };

            // Act
            var stateBinary = board.StateBinary;

            // Assert
            Assert.Equal(expectedBinary, stateBinary);
        }

        [Fact]
        public void Board_ShouldGenerateCorrectStateHash()
        {
            // Arrange
            var board = new Board
            {
                Rows = 2,
                Columns = 3,
                State = new List<List<bool>>
                {
                    new() { true, false, true },
                    new() { false, true, false }
                }
            };

            // Act
            var stateHash = board.StateHash;

            // Assert
            Assert.NotNull(stateHash);
            Assert.Equal(32, stateHash.Length); // SHA-256 produces 32-byte hashes
        }

        [Theory]
        [InlineData(3, 3, true)]
        [InlineData(2, 3, false)]
        [InlineData(3, 2, false)]
        public void ValidateState_ShouldValidateRowsAndColumns(int rows, int columns, bool isValid)
        {
            // Arrange
            var board = new Board
            {
                Rows = rows,
                Columns = columns,
                State = new List<List<bool>>
                {
                    new() { true, false, true },
                    new() { false, true, false },
                    new() { true, true, true }
                }
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(board);

            // Act
            var result = Validator.TryValidateObject(board, validationContext, validationResults, true);

            // Assert
            Assert.Equal(isValid, result);
        }

        [Fact]
        public void ValidateState_ShouldReturnError_WhenBoardStateIsNull()
        {
            // Arrange
            var board = new Board { Rows = 2, Columns = 2, State = null! };
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(board);

            // Act
            var result = Validator.TryValidateObject(board, validationContext, validationResults, true);

            // Assert
            Assert.False(result);
            Assert.Contains(validationResults, v => v.ErrorMessage == $"The {nameof(Board.State)} field is required.");
        }
    }
}
