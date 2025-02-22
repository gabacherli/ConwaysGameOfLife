using Dapper;
using GameOfLife.API.Helpers;
using GameOfLife.API.Models;
using GameOfLife.API.Settings;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;

namespace GameOfLife.API.Repositories.Write
{
    public class BoardWriteRepository : IBoardWriteRepository
    {
        private readonly string _connectionString;
        private const string InsertBoardSql = "sp_insertBoardState";

        public BoardWriteRepository(IOptions<AppSettings> settings)
        {
            _connectionString = ConfigurationHelper.ReadDockerSecretFileAsString(settings.Value.DockerSecretPaths.BoardWriteConnectionString);
        }

        public async Task<Guid> InsertBoardAsync(Board board)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("@Rows", board.Rows, DbType.Int32);
            parameters.Add("@Columns", board.Columns, DbType.Int32);
            parameters.Add("@State", board.StateBinary, DbType.Binary);
            parameters.Add("@StateHash", board.StateHash, DbType.Binary);
            parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(InsertBoardSql, parameters, commandType: CommandType.StoredProcedure);

            board.Id = parameters.Get<Guid>("@Id");

            return board.Id;
        }
    }
}
