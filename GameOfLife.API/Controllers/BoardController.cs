using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.API.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardController : ControllerBase
{
    private readonly ILogger<BoardController> _logger;

    public BoardController(ILogger<BoardController> logger)
    {
        _logger = logger;
    }
}
