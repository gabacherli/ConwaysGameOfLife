namespace GameOfLife.API.Middleware.Providers
{
    public class TraceIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TraceIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTraceId()
        {
            return _httpContextAccessor.HttpContext?.Items["TraceId"]?.ToString() ?? Guid.NewGuid().ToString();
        }
    }
}
