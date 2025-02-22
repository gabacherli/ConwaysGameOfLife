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
        private readonly IBoardService _boardService;
        private readonly int? _maxIterations;

        public BoardController(
            ILogger<BoardController> logger,
            IOptions<AppSettings> settings,
            IBoardService boardService)
        {
            _logger = logger;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _boardService.InsertBoardAsync(board);

            return Ok(id);
        }

        /// <summary>
        /// Reveals the next tick/state of a specified board.
        /// </summary>
        /// <param name="id">The ID of an existing board.</param>
        /// <returns>The next tick/state of a board.</returns>
        /// <response code="200">Returns the next tick/state of an existing board.</response>
        /// <response code="404">Board not found.</response>
        [HttpGet("{id}/next")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNextTickOfExistingBoardAsync([Required] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Response = $"{nameof(id)} must be different than the default value." });
            }

            var result = await _boardService.GetNextTickOfExistingBoardAsync(id);

            if (result is null)
            {
                return NotFound(new { Response = "Board not found." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Reveals the tick/state of a specified board after N iterations.
        /// </summary>
        /// <param name="id">The ID of an existing board.</param>
        /// <param name="iterations">The number of iterations to compute.</param>
        /// <returns>The tick/state of a board after N iterations.</returns>
        /// <response code="200">Returns the tick/state of an existing board after N iterations.</response>
        /// <response code="400">Invalid iterations input.</response>
        /// <response code="404">Board not found.</response>
        [HttpGet("{id}/next/{iterations}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBoardAfterNIterationsAsync([Required] Guid id, [Required, Range(1, int.MaxValue)] int iterations)
        {
            if (iterations <= 0 || iterations > _maxIterations || id == default)
            {
                return BadRequest(new { Response = $"{nameof(id)} must be different than the default value and {nameof(iterations)} must be between 1 and {_maxIterations}." });
            }

            var result = await _boardService.GetBoardAfterNIterationsAsync(id, iterations);

            if (result is null)
            {
                return NotFound(new { Response = "Board not found." });
            }

            return Ok(result);
        }
    }
}