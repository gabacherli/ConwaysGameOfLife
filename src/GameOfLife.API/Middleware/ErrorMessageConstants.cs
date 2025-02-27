namespace GameOfLife.API.Middleware
{
    public static class ErrorMessageConstants
    {
        public const string DefaultErrorMessage = "An unexpected error occurred.";
        public const string ArgumentExceptionMessage = "Invalid input provided.";
        public const string FileNotFoundExceptionMessage = "File not found.";
        public const string InvalidOperationExceptionMessage = "Operation could not be completed.";
        public const string SqlExceptionUnavailableMessage = "Database is unavailable.";
        public const string SqlExceptionGenericMessage = "An error occurred while processing the request.";
    }
}
