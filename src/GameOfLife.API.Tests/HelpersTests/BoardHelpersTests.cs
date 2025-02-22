using GameOfLife.API.Helpers;
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
            var result = BoardHelpers.ConvertToBinary(state);

            // Assert
            Assert.Equal(expectedBinary, result);
        }

        [Fact]
        public void ComputeStateHash_ShouldComputeSHA256Hash()
        {
            // Arrange
            var data = new byte[] { 1, 0, 1, 0, 1, 0 };

            // Act
            var hashResult = BoardHelpers.ComputeStateHash(data);
            var expectedHash = SHA256.HashData(data);

            // Assert
            Assert.Equal(expectedHash, hashResult);
        }
    }
}
