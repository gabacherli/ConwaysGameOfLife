namespace GameOfLife.API.Models
{
    public class CustomException
    {
        public string TraceId { get; internal set; } = string.Empty;
        public string Message { get; internal set; } = string.Empty;
        public string Error { get; internal set; } = string.Empty;
    }
}
