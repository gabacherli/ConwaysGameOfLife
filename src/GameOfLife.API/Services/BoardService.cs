using GameOfLife.API.Helpers;
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
        /// <param name="board">The board object containing its dimensions and a 2 dimensional array of states.</param>
        /// <returns>The freshly generated board ID.</returns>
        public async Task<Guid> InsertBoardAsync(Board board)
        {
            await _writeRepository.InsertBoardAsync(board);

            _logger.LogInformation("Board inserted: {Id}", board.Id);

            return board.Id;
        }

        /// <summary>
        /// Retrieves the next tick of an existing board by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>The board object with its state updated to the next tick.</returns>
        public async Task<Board?> GetNextTickOfExistingBoardAsync(Guid id)
        {
            var board = await _readRepository.GetBoardAsync(id);

            if (board is null) return null;
            board.State = BoardHelper.GetNextTick(board, board.Rows, board.Columns, out _);

            return board;
        }
    }
}
