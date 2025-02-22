using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Read;
using NSubstitute;

namespace GameOfLife.API.Tests.RepositoriesTests.Read
{
    public class BoardReadRepositoryTests
    {
        private readonly IBoardReadRepository _repo = Substitute.For<IBoardReadRepository>();

        [Fact]
        public async Task GetBoardAsync_ShouldReturnSpecifiedBoard()
        {
            // Arrange
            var boardId = Guid.NewGuid();

            _repo.GetBoardAsync(boardId).Returns(new Board() { Id = boardId });

            // Act
            var board = await _repo.GetBoardAsync(boardId);

            // Assert
            Assert.NotNull(board);
            Assert.Equal(boardId, board.Id);
        }

        [Fact]
        public async Task GetBoardAsync_ShouldReturnNull_WhenBoardDoesNotExist()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            _repo.GetBoardAsync(boardId).Returns((Board)null!);

            // Act
            var board = await _repo.GetBoardAsync(boardId);

            // Assert
            Assert.Null(board);
        }
    }
}
