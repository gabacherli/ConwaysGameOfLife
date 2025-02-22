using GameOfLife.API.Models;

namespace GameOfLife.API.Services
{
    public interface IBoardService
    {
        Task<Board?> GetBoardAfterNIterationsAsync(Guid id, int iterations);
        Task<Board?> GetNextIterationOfExistingBoardAsync(Guid id);
        Task<(Board?, int, EndReason)?> GetStableOrFinalIterationAsync(Guid id, int maxIterations);
        Task<Guid> InsertBoardAsync(Board board);
    }
}