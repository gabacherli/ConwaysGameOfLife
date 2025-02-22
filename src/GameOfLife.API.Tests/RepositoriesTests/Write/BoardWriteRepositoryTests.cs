using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Write;
using NSubstitute;

namespace GameOfLife.API.Tests.RepositoriesTests.Write
{
    public class BoardWriteRepositoryTests
    {
        private readonly IBoardWriteRepository _repo = Substitute.For<IBoardWriteRepository>();

        [Fact]
        public async Task InsertBoardAsync_ShouldReturnGeneratedBoardId()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            var board = new Board
            {
                Rows = 5,
                Columns = 5,
                State = new List<List<bool>> { new() { false, true, false, true, false }, new() { true, false, true, false, true } }
            };

            _repo.InsertBoardAsync(board).Returns(boardId);

            // Act
            var resultId = await _repo.InsertBoardAsync(board);

            // Assert
            Assert.NotEqual(Guid.Empty, boardId);
            Assert.Equal(boardId, resultId);
        }
    }
}
