using Dapper;
using GameOfLife.API.Helpers;
using GameOfLife.API.Middleware.Providers;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Read.CustomQueries;
using GameOfLife.API.Settings;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GameOfLife.API.Repositories.Read
{
    public class BoardReadRepository : IBoardReadRepository
    {
        private readonly ILogger<BoardReadRepository> _logger;
        private readonly TraceIdProvider _traceIdProvider;
        private readonly string _connectionString;
        private const string GetBoardSql = GetBoardQuery.Sql;

        public BoardReadRepository(
            ILogger<BoardReadRepository> logger,
            TraceIdProvider traceIdProvider,
            IOptions<AppSettings> settings)
        {
            _logger = logger;
            _traceIdProvider = traceIdProvider;
            _connectionString = ConfigurationHelper.ReadDockerSecretFileAsString(settings.Value.DockerSecretPaths.BoardReadConnectionString);
        }

        public async Task<Board?> GetBoardAsync(Guid id)
        {
            string traceId = _traceIdProvider.GetTraceId();
            _logger.LogDebug("[{TraceId}] {Method}: Establishing database connection to {Repo}...", traceId, nameof(GetBoardAsync), nameof(BoardReadRepository));

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            _logger.LogDebug("[{TraceId}] {Method}: Database connection established.", traceId, nameof(GetBoardAsync));

            _logger.LogDebug("[{TraceId}] {Method}: Executing query {Query}...", traceId, nameof(GetBoardAsync), GetBoardSql);

            var stopwatch = Stopwatch.StartNew();
            // Using `dynamic` type so we can convert the binary state to a 2D array before returning the Board object.
            var record = await connection.QueryFirstOrDefaultAsync<dynamic>(GetBoardSql, new { Id = id });

            _logger.LogDebug("[{TraceId}] {Method}: Converting binary state to 2D array...", traceId, nameof(GetBoardAsync));

            var board = new Board
            {
                Id = record!.Id,
                Rows = record.Rows,
                Columns = record.Columns,
                State = BoardHelper.ConvertFromBinary(record.State, record.Rows, record.Columns)
            };

            stopwatch.Stop();
            _logger.LogInformation("[{TraceId}] {Method}: Board retrieved successfully with ID {BoardId} (Execution Time: {ElapsedMs} ms).", traceId, nameof(GetBoardAsync), board.Id, stopwatch.ElapsedMilliseconds);

            if (board is null) return null;

            return board;
        }
    }
}
