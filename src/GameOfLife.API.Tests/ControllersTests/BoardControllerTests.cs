using GameOfLife.API.Controllers;
using GameOfLife.API.Models;
using GameOfLife.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GameOfLife.API.Tests.ControllersTests
{
    public class BoardControllerTests
    {
        private readonly IBoardService _boardService = Substitute.For<IBoardService>();
        private readonly ILogger<BoardController> _logger = Substitute.For<ILogger<BoardController>>();
        private readonly BoardController _controller;

        public BoardControllerTests()
        {
            _controller = new BoardController(_logger, _boardService);
        }

        public static IEnumerable<object[]> GetInvalidBoardTestCases()
        {
            yield return new object[]
            {
                new Board
                {
                    Rows = 5,
                    Columns = 5,
                    State = new List<List<bool>>() // Empty state
                },
                nameof(Board.State),
                "Board state cannot be null or empty."
            };

            yield return new object[]
            {
                new Board
                {
                    Rows = 5,
                    Columns = 5,
                    State = new List<List<bool>>
                    {
                        new() { true, false, true, false },
                        new() { false, true, false, true, false },
                        new() { true, false, true, false, true },
                        new() { false, true, false, true, false },
                        new() { true, false, true, false, true }
                    }
                },
                nameof(Board.State),
                "Board state column count mismatch in 1 row(s): Row 0 (Expected 5, Found 4)."
            };

            yield return new object[]
            {
                new Board
                {
                    Rows = 4, // Should be 5
                    Columns = 5,
                    State = new List<List<bool>>
                    {
                        new() { true, false, true, false, true },
                        new() { false, true, false, true, false },
                        new() { true, false, true, false, true },
                        new() { false, true, false, true, false },
                        new() { true, false, true, false, true }
                    }
                },
                nameof(Board.State),
                "Board state row count mismatch: Expected 4, but got 5."
            };

            yield return new object[]
            {
                new Board
                {
                    Rows = 5,
                    Columns = 5,
                    State = new List<List<bool>>
                    {
                        new() { true, false, true, false, true },
                        new() { false, true, false, true, false },
                        new() { true, false, true, false, true },
                        new() { false, true, false, true, false },
                        new() { } // Empty row
                    }
                },
                nameof(Board.State),
                "Board state column count mismatch in 1 row(s): Row 4 (Expected 5, Found 0)."
            };
        }

        [Fact]
        public async Task UploadBoard_ShouldReturnOk_WhenBoardIsInserted()
        {
            // Arrange
            var board = new Board
            {
                Rows = 5,
                Columns = 5,
                State = new List<List<bool>>
                {
                    new() { true, false, true, false, true },
                    new() { false, true, false, true, false },
                    new() { true, false, true, false, true },
                    new() { false, true, false, true, false },
                    new() { true, false, true, false, true }
                }
            };
            var expectedId = Guid.NewGuid();
            _boardService.InsertBoardAsync(board).Returns(expectedId);

            // Act
            var result = await _controller.UploadBoardAsync(board) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(expectedId, (Guid)result.Value!);
        }

        [Theory]
        [MemberData(nameof(GetInvalidBoardTestCases))]
        public async Task UploadBoard_ShouldReturnBadRequest_WhenModelStateIsInvalid(Board board, string expectedErrorKey, string expectedErrorMessage)
        {
            // Arrange
            _controller.ModelState.AddModelError(expectedErrorKey, expectedErrorMessage);

            // Act
            var result = await _controller.UploadBoardAsync(board) as BadRequestObjectResult;

            var errorResponse = result!.Value as SerializableError;

            // Assert
            Assert.NotNull(result);
            Assert.True(errorResponse!.ContainsKey(expectedErrorKey));

            var errorMessages = errorResponse[expectedErrorKey] as string[];
            Assert.NotNull(errorMessages);
            Assert.Single(errorMessages);
            Assert.Contains(expectedErrorMessage, errorMessages.First());
        }
    }
}