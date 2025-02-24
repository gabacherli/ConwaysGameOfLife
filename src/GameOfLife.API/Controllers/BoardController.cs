using GameOfLife.API.Middleware.Providers;
using GameOfLife.API.Models;
using GameOfLife.API.Services;
using GameOfLife.API.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace GameOfLife.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BoardController : ControllerBase
    {
        private readonly ILogger<BoardController> _logger;
        private readonly TraceIdProvider _traceIdProvider;
        private readonly IBoardService _boardService;
        private readonly int? _maxIterations;

        public BoardController(
            ILogger<BoardController> logger,
            TraceIdProvider traceIdProvider,
            IOptions<AppSettings> settings,
            IBoardService boardService)
        {
            _logger = logger;
            _traceIdProvider = traceIdProvider;
            _boardService = boardService;
            _maxIterations = settings?.Value?.MaxIterations;
        }

        /// <summary>
        /// Uploads a new board to the GameOfLife db.
        /// </summary>
        /// <param name="board">The board object containing its dimensions and a 2 dimensional array of states.</param>
        /// <returns>The ID of the board.</returns>
        /// <response code="200">Returns the freshly generated board ID.</response>
        /// <response code="400">ValidationResult of the payload.</response>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadBoardAsync([FromBody] Board board)
        {
            var traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Received request to insert board.", traceId, nameof(UploadBoardAsync));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _boardService.InsertBoardAsync(board);

            _logger.LogInformation("[{TraceId}] {Method}: Board inserted successfully with ID {BoardId}.", traceId, nameof(UploadBoardAsync), id);
            return Ok(id);
        }

        /// <summary>
        /// Reveals the next iteration/state of a specified board.
        /// </summary>
        /// <param name="id">The ID of an existing board.</param>
        /// <returns>The next iteration/state of a board.</returns>
        /// <response code="200">Returns the next iteration/state of an existing board.</response>
        /// <response code="404">Board not found.</response>
        [HttpGet("{id}/next")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNextIterationOfExistingBoardAsync(Guid id)
        {
            var traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Fetching board ID {BoardId}.", traceId, nameof(GetNextIterationOfExistingBoardAsync), id);

            if (id == Guid.Empty)
            {
                return BadRequest(new { Response = $"{nameof(id)} must be different than the default value." });
            }

            var result = await _boardService.GetNextIterationOfExistingBoardAsync(id);

            if (result is null)
            {
                return NotFound(new { Response = "Board not found." });
            }

            _logger.LogInformation("[{TraceId}] {Method}: Successfully computed next iteration for Board ID {BoardId}.", traceId, nameof(GetNextIterationOfExistingBoardAsync), id);
            return Ok(result);
        }

        /// <summary>
        /// Reveals the iteration/state of a specified board after N iterations.
        /// </summary>
        /// <param name="id">The ID of an existing board.</param>
        /// <param name="iterations">The number of iterations to compute.</param>
        /// <returns>The iteration/state of a board after N iterations.</returns>
        /// <response code="200">Returns the iteration/state of an existing board after N iterations.</response>
        /// <response code="400">Invalid iterations input.</response>
        /// <response code="404">Board not found.</response>
        [HttpGet("{id}/next/{iterations}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBoardAfterNIterationsAsync(Guid id, [Required, Range(1, int.MaxValue)] int iterations)
        {
            var traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Fetching board {BoardId} for {Iterations} iterations.", traceId, nameof(GetBoardAfterNIterationsAsync), id, iterations);

            // Limiting to environment config value to prevent abuse
            if (iterations <= 0 || iterations > _maxIterations || id == default)
            {
                return BadRequest(new { Response = $"{nameof(id)} must be different than the default value and {nameof(iterations)} must be between 1 and {_maxIterations}." });
            }

            var result = await _boardService.GetBoardAfterNIterationsAsync(id, iterations);

            if (result is null)
            {
                return NotFound(new { Response = "Board not found." });
            }

            _logger.LogInformation("[{TraceId}] {Method}: Successfully computed {Iterations} iterations for Board ID {BoardId}.", traceId, nameof(GetBoardAfterNIterationsAsync), iterations, id);
            return Ok(result);
        }

        /// <summary>
        /// Reveals the stable or final iteration/state of a specified board.
        /// </summary>
        /// <param name="id">The ID of an existing board.</param>
        /// <param name="maxIterations">The maximum number of iterations to compute.</param>
        /// <returns>The stable or final iteration/state of a board.</returns>
        /// <response code="200">Returns the stable or final iteration/state of an existing board.</response>
        /// <response code="400">Invalid input or reached the maximum interactions allowed.</response>
        /// <response code="404">Board not found.</response>
        [HttpGet("{id}/finalIteration/{maxIterations}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStableOrFinalIterationAsync(Guid id, [Range(1, int.MaxValue)] int maxIterations = 1500)
        {
            var traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Fetching board {BoardId} for max {MaxIterations} iterations.", traceId, nameof(GetStableOrFinalIterationAsync), id, maxIterations);

            // Limiting to environment config value to prevent abuse
            if (maxIterations <= 0 || maxIterations > _maxIterations || id == default)
            {
                return BadRequest(new { Response = $"{nameof(id)} must be different than the default value and {nameof(maxIterations)} must be between 1 and {_maxIterations}." });
            }

            var result = await _boardService.GetStableOrFinalIterationAsync(id, maxIterations);

            if (result is null)
            {
                return NotFound(new { Response = "Board not found." });
            }

            var response = new FinalIterationResponse
            {
                Board = result.Value.Item1!,
                Iterations = result.Value.Item2,
                EndReason = result.Value.Item3.ToString()
            };

            if (response.EndReason == EndReason.MaxIterationsReached.ToString())
            {
                return BadRequest(response);
            }

            _logger.LogInformation("[{TraceId}] {Method}: Computed stable iteration for Board ID {BoardId}. End Reason: {EndReason}", traceId, nameof(GetStableOrFinalIterationAsync), id, response.EndReason);
            return Ok(response);
        }
    }
}