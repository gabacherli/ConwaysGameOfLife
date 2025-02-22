using GameOfLife.API.Models;
using GameOfLife.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.API.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardController : ControllerBase
{
    private readonly ILogger<BoardController> _logger;
    private readonly IBoardService _boardService;

    public BoardController(ILogger<BoardController> logger, IBoardService boardService)
    {
        _logger = logger;
        _boardService = boardService;
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextTickOfExistingBoardAsync(Guid id)
    {
        var result = await _boardService.GetNextTickOfExistingBoardAsync(id);

        if (result is null)
            return NotFound(new { _ = "Board not found." });

        return Ok(result);
    }
}
