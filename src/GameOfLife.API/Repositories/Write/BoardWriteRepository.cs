using Dapper;
using GameOfLife.API.Helpers;
using GameOfLife.API.Middleware.Providers;
using GameOfLife.API.Models;
using GameOfLife.API.Settings;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GameOfLife.API.Repositories.Write
{
    public class BoardWriteRepository : IBoardWriteRepository
    {
        private readonly ILogger<BoardWriteRepository> _logger;
        private readonly TraceIdProvider _traceIdProvider;
        private readonly string _connectionString;
        private const string InsertBoardSql = "sp_insertBoardState";

        public BoardWriteRepository(
            ILogger<BoardWriteRepository> logger,
            TraceIdProvider traceIdProvider,
            IOptions<AppSettings> settings)
        {
            _logger = logger;
            _traceIdProvider = traceIdProvider;
            _connectionString = ConfigurationHelper.ReadDockerSecretFileAsString(settings.Value.DockerSecretPaths.BoardWriteConnectionString);
        }

        public async Task<Guid> InsertBoardAsync(Board board)
        {
            string traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Establishing database connection to {Repo}...", traceId, nameof(InsertBoardAsync), nameof(BoardWriteRepository));

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            _logger.LogDebug("[{TraceId}] {Method}: Database connection established.", traceId, nameof(InsertBoardAsync));

            var parameters = new DynamicParameters();
            parameters.Add("@Rows", board.Rows, DbType.Int32);
            parameters.Add("@Columns", board.Columns, DbType.Int32);
            parameters.Add("@State", board.StateBinary, DbType.Binary);
            parameters.Add("@StateHash", board.StateHash, DbType.Binary);
            parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            _logger.LogDebug("[{TraceId}] {Method}: Executing stored procedure {SP}...", traceId, nameof(InsertBoardAsync), InsertBoardSql);

            var stopwatch = Stopwatch.StartNew();
            await connection.ExecuteAsync(InsertBoardSql, parameters, commandType: CommandType.StoredProcedure);
            stopwatch.Stop();

            board.Id = parameters.Get<Guid>("@Id");

            _logger.LogInformation("[{TraceId}] {Method}: Board inserted successfully with ID {BoardId} (Execution Time: {ElapsedMs} ms).",
                traceId, nameof(InsertBoardAsync), board.Id, stopwatch.ElapsedMilliseconds);

            return board.Id;
        }
    }
}
