namespace GameOfLife.API.Repositories.Read.CustomQueries
{
    public static class GetBoardQuery
    {
        public const string Sql =
            """
            SELECT 
                Id, 
                Rows, 
                Columns, 
                State
            FROM 
                Boards
            WHERE 
                Id = @Id
            """;
    }
}
