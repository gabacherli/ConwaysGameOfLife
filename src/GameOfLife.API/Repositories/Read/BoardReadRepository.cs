using Dapper;
using GameOfLife.API.Helpers;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Read.CustomQueries;
using GameOfLife.API.Settings;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace GameOfLife.API.Repositories.Read
{
    public class BoardReadRepository : IBoardReadRepository
    {
        private readonly string _connectionString;
        private const string GetBoardSql = GetBoardQuery.Sql;

        public BoardReadRepository(IOptions<AppSettings> settings)
        {
            _connectionString = ConfigurationHelper.ReadDockerSecretFileAsString(settings.Value.DockerSecretPaths.BoardReadConnectionString);
        }

        public async Task<Board?> GetBoardAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Using `dynamic` type so we can convert the binary state to a 2D array before returning the Board object.
            var board = await connection.QueryFirstOrDefaultAsync<dynamic>(GetBoardSql, new { Id = id });

            if (board is null) return null;

            return new Board
            {
                Id = board!.Id,
                Rows = board.Rows,
                Columns = board.Columns,
                State = BoardHelper.ConvertFromBinary(board.State, board.Rows, board.Columns)
            };
        }
    }
}
