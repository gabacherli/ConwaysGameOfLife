using GameOfLife.API.Models;

namespace GameOfLife.API.Repositories.Write
{
    public interface IBoardWriteRepository
    {
        Task<Guid> InsertBoardAsync(Board board);
    }
}