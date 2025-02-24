using GameOfLife.API.Helpers;
using GameOfLife.API.Middleware.Providers;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Read;
using GameOfLife.API.Repositories.Write;

namespace GameOfLife.API.Services
{
    public class BoardService : IBoardService
    {
        private readonly ILogger<BoardService> _logger;
        private readonly TraceIdProvider _traceIdProvider;
        private readonly IBoardReadRepository _readRepository;
        private readonly IBoardWriteRepository _writeRepository;

        public BoardService(
            ILogger<BoardService> logger,
            TraceIdProvider traceIdProvider,
            IBoardReadRepository readRepository,
            IBoardWriteRepository writeRepository)
        {
            _logger = logger;
            _traceIdProvider = traceIdProvider;
            _readRepository = readRepository;
            _writeRepository = writeRepository;
        }

        /// <summary>
        /// Inserts a new board to the GameOfLife db.
        /// </summary>
        /// <param name="board">The board object containing its dimensions and a 2 dimensional array of states.</param>
        /// <returns>The freshly generated board ID.</returns>
        public async Task<Guid> InsertBoardAsync(Board board)
        {
            await _writeRepository.InsertBoardAsync(board);

            return board.Id;
        }

        /// <summary>
        /// Retrieves the next iteration of an existing board by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>The board object with its state updated to the next iteration.</returns>
        public async Task<Board?> GetNextIterationOfExistingBoardAsync(Guid id)
        {
            var board = await _readRepository.GetBoardAsync(id);

            if (board is null) return null;

            var traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Computing next iteration for Board ID {BoardId}.", traceId, nameof(GetNextIterationOfExistingBoardAsync), id);

            board.State = BoardHelper.GetNextIteration(board, out _);

            _logger.LogInformation("[{TraceId}] {Method}: Computed next iteration for Board ID {BoardId}.", traceId, nameof(GetNextIterationOfExistingBoardAsync), id);

            return board;
        }

        /// <summary>
        /// Retrieves the state of a board after a specified number of iterations.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <param name="iterations">The number of iterations to simulate.</param>
        /// <returns>The board object with its state updated after the specified number of iterations.</returns>
        public async Task<Board?> GetBoardAfterNIterationsAsync(Guid id, int iterations)
        {
            var board = await _readRepository.GetBoardAsync(id);

            if (board is null) return null;

            var traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Computing {Iterations} iterations for Board ID {BoardId}.", traceId, nameof(GetBoardAfterNIterationsAsync), iterations, id);

            board.State = BoardHelper.GetBoardAfterNIterations(board, iterations, out _);

            _logger.LogInformation("[{TraceId}] {Method}: Computed {Iterations} iterations for Board ID {BoardId}.", traceId, nameof(GetBoardAfterNIterationsAsync), iterations, id);

            return board;
        }

        /// <summary>
        /// Retrieves the stable or final iteration of a board by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <param name="maxIterations">The maximum number of iterations to simulate.</param>
        /// <returns>A tuple containing the board object, the number of iterations performed, and the reason for ending the simulation.</returns>
        public async Task<(Board?, int, EndReason)?> GetStableOrFinalIterationAsync(Guid id, int maxIterations)
        {
            var board = await _readRepository.GetBoardAsync(id);

            if (board is null) return null;

            int iterations;

            var traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Computing stable iteration for Board ID {BoardId}.", traceId, nameof(GetStableOrFinalIterationAsync), id);

            (board.State, iterations) = BoardHelper.GetStableOrFinalIteration(board, maxIterations, out _, out var endReason);

            _logger.LogInformation("[{TraceId}] {Method}: Computed stable iteration for Board ID {BoardId}. End Reason: {EndReason}", traceId, nameof(GetStableOrFinalIterationAsync), id, endReason);

            return (board, iterations, endReason);
        }
    }
}
