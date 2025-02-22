using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Read;
using GameOfLife.API.Repositories.Write;
using GameOfLife.API.Settings;
using Microsoft.Extensions.Options;

namespace GameOfLife.API.Services
{
    public class BoardService : IBoardService
    {
        private readonly ILogger<BoardService> _logger;
        private readonly IBoardReadRepository _readRepository;
        private readonly IBoardWriteRepository _writeRepository;
        private readonly int _maxAttempts;

        public BoardService(
            IBoardReadRepository readRepository,
            IBoardWriteRepository writeRepository,
            IOptions<AppSettings> settings,
            ILogger<BoardService> logger)
        {
            _readRepository = readRepository;
            _writeRepository = writeRepository;
            _maxAttempts = settings.Value.MaxAttempts;
            _logger = logger;
        }

        /// <summary>
        /// Inserts a new board to the GameOfLife db.
        /// </summary>
        /// <param name="board">The board object containing dimensions and 2 dimensional array of states.</param>
        /// <returns>The freshly generated board ID.</returns>
        public async Task<Guid> InsertBoardAsync(Board board)
        {
            await _writeRepository.InsertBoardAsync(board);

            _logger.LogInformation("Board inserted: {Id}", board.Id);

            return board.Id;
        }
    }
}
