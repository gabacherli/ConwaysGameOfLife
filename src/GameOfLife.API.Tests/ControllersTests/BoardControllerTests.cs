﻿using GameOfLife.API.Controllers;
using GameOfLife.API.Helpers;
using GameOfLife.API.Models;
using GameOfLife.API.Services;
using GameOfLife.API.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NSubstitute;

namespace GameOfLife.API.Tests.ControllersTests
{
    public class BoardControllerTests
    {
        private readonly ILogger<BoardController> _logger = Substitute.For<ILogger<BoardController>>();
        private readonly IBoardService _boardService = Substitute.For<IBoardService>();
        private readonly IOptions<AppSettings> _appSettings = Substitute.For<IOptions<AppSettings>>();
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private BoardController _controller;

        public BoardControllerTests()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = false }
            };
            _controller = new BoardController(_logger, _appSettings, _boardService);
        }

        [Fact]
        public async Task UploadBoardAsync_ShouldReturnOk_WhenBoardIsInserted()
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
        public async Task UploadBoardAsync_ShouldReturnBadRequest_WhenModelStateIsInvalid(Board board, string expectedErrorKey, string expectedErrorMessage)
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

        [Fact]
        public async Task GetNextTickOfExistingBoardAsync_ShouldReturnOk_WhenBoardExists()
        {
            // Arrange
            var firstIterationJson = File.ReadAllText("./ControllersTests/Payloads/20x20board_1stIteration.json");

            var secondIterationJson = File.ReadAllText("./ControllersTests/Payloads/20x20board_2ndIteration.json");

            var firstIteration = JsonConvert.DeserializeObject<Board>(firstIterationJson, _jsonSerializerSettings);

            var expectedSecondIteration = JsonConvert.DeserializeObject<Board>(secondIterationJson, _jsonSerializerSettings);

            _boardService.GetNextTickOfExistingBoardAsync(firstIteration!.Id)!.Returns(Task.FromResult(expectedSecondIteration));

            var secondIteration = BoardHelper.GetNextTick(firstIteration, firstIteration.Rows, firstIteration.Columns, out var secondIterationStateHash);

            // Act
            var result = await _controller.GetNextTickOfExistingBoardAsync(firstIteration.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedSecondIteration, okResult.Value);
            Assert.Equal(expectedSecondIteration!.State, secondIteration);
            Assert.Equal(expectedSecondIteration!.StateHash, secondIterationStateHash);
        }

        [Fact]
        public async Task GetNextTickOfExistingBoardAsync_ShouldReturnNotFound_WhenBoardDoesNotExist()
        {
            // Arrange
            var boardId = Guid.NewGuid();
            _boardService.GetNextTickOfExistingBoardAsync(boardId).Returns(Task.FromResult<Board?>(null));

            // Act
            var result = await _controller.GetNextTickOfExistingBoardAsync(boardId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Theory]
        [InlineData(1501, 1500, "350ac640-c1f8-49b0-a43a-2ca3360f4413")]
        [InlineData(0, 1000, "1720f071-ad23-4531-ac42-b7e6582ca078")]
        [InlineData(-3, 100, "b3c6369c-2416-41be-9849-1a5e914af7d3")]
        [InlineData(500, 1000, "00000000-0000-0000-0000-000000000000")]

        public async Task GetNextNIterationsAsync_ShouldReturnBadRequest_WhenInputIsInvalid(int iterations, int maxIterations, string id)
        {
            // Arrange
            _appSettings.Value.Returns(new AppSettings { MaxIterations = maxIterations });

            var boardId = Guid.Parse(id);

            _controller = new BoardController(_logger, _appSettings, _boardService);

            // Act
            var result = await _controller.GetBoardAfterNIterationsAsync(boardId, iterations);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Contains($"{nameof(id)} must be different than the default value and {nameof(iterations)} must be between 1 and {maxIterations}.", badRequestResult.Value!.ToString());
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
    }
}