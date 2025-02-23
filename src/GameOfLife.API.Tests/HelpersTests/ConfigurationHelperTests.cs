using GameOfLife.API.Helpers;

namespace GameOfLife.API.Tests.HelpersTests
{
    public class ConfigurationHelperTests
    {
        [Fact]
        public void ReadDockerSecretFileAsString_ShouldReturnTrimmedContent()
        {
            // Arrange
            var testFilePath = Path.GetTempFileName();
            var expectedContent = "secret-value";
            File.WriteAllText(testFilePath, " secret-value \n");

            try
            {
                // Act
                var result = ConfigurationHelper.ReadDockerSecretFileAsString(testFilePath);

                // Assert
                Assert.Equal(expectedContent, result);
            }
            finally
            {
                File.Delete(testFilePath);
            }
        }

        [Fact]
        public void ReadDockerSecretFileAsString_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentPath = "non_existent_file.txt";
            var expectedErrorMessage = $"Docker Secret File missing: {nonExistentPath}";

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() =>
                ConfigurationHelper.ReadDockerSecretFileAsString(nonExistentPath));
            Assert.Equal(expectedErrorMessage, exception.Message);
        }
    }

}
