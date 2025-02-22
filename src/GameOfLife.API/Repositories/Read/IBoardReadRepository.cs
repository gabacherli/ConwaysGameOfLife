using GameOfLife.API.Models;

namespace GameOfLife.API.Repositories.Read
{
    public interface IBoardReadRepository
    {
        Task<Board?> GetBoardAsync(Guid id);
    }
}