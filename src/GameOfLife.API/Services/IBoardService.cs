using GameOfLife.API.Models;

namespace GameOfLife.API.Services
{
    public interface IBoardService
    {
        Task<Board?> GetBoardAfterNIterationsAsync(Guid id, int iterations);
        Task<Board?> GetNextTickOfExistingBoardAsync(Guid id);
        Task<Guid> InsertBoardAsync(Board board);
    }
}