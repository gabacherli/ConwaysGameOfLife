using GameOfLife.API.Models;
using System.Data.SqlClient;
using System.Net;
using System.Text.Json;

namespace GameOfLife.API.Middleware
{
    public class HttpRequestTrackingMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ILogger<HttpRequestTrackingMiddleware> _logger;

        public HttpRequestTrackingMiddleware(RequestDelegate requestDelegate, ILogger<HttpRequestTrackingMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string traceId = Guid.NewGuid().ToString();
            context.Items["TraceId"] = traceId;

            _logger.LogInformation("[{TraceId}] Processing request {Method} {Path}", traceId, context.Request.Method, context.Request.Path);

            try
            {
                await _requestDelegate(context);
            }
            catch (Exception ex)
            {
                var customException = HandleException(context, ex, traceId);
                var json = JsonSerializer.Serialize(customException);
                await context.Response.WriteAsync(json);
            }
            finally
            {
                LogResponse(context, traceId);
            }
        }

        private void LogResponse(HttpContext context, string traceId)
        {
            var responseStatus = context.Response.StatusCode;
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            switch (responseStatus)
            {
                case >= 200 and < 300:
                    _logger.LogInformation("[{TraceId}] Request completed successfully with status {StatusCode}", traceId, responseStatus);
                    break;
                case >= 300 and < 400:
                    _logger.LogWarning("[{TraceId}] Redirection {StatusCode} on {Method} {Path}", traceId, responseStatus, requestMethod, requestPath);
                    break;
                case >= 400 and < 500:
                    _logger.LogWarning("[{TraceId}] Client error {StatusCode} on {Method} {Path}", traceId, responseStatus, requestMethod, requestPath);
                    break;
                case >= 500:
                    _logger.LogError("[{TraceId}] Server error {StatusCode} on {Method} {Path}", traceId, responseStatus, requestMethod, requestPath);
                    break;
            }
        }

        private static CustomException HandleException(HttpContext context, Exception exception, string traceId)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var customException = new CustomException
            {
                TraceId = traceId,
                Message = ErrorMessageConstants.DefaultErrorMessage,
                Error = exception.Message
            };

            switch (exception)
            {
                case ArgumentException ex:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    customException = GetCustomException(traceId, ex.GetType().Name, ErrorMessageConstants.ArgumentExceptionMessage, ex.Message);
                    break;

                case FileNotFoundException ex:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    customException = GetCustomException(traceId, ex.GetType().Name, ErrorMessageConstants.FileNotFoundExceptionMessage, ex.Message);
                    break;

                case InvalidOperationException ex:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    customException = GetCustomException(traceId, ex.GetType().Name, ErrorMessageConstants.InvalidOperationExceptionMessage, ex.Message);
                    break;

                case SqlException ex when
                    ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.Contains("error occurred while establishing a connection", StringComparison.OrdinalIgnoreCase):
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    customException = GetCustomException(traceId, ex.GetType().Name, ErrorMessageConstants.SqlExceptionUnavailableMessage, ex.Message);
                    break;

                case SqlException ex:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    customException = GetCustomException(traceId, ex.GetType().Name, ErrorMessageConstants.SqlExceptionGenericMessage, ex.Message);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            return customException;
        }

        private static CustomException GetCustomException(string traceId, string exceptionType, string customMessage, string exceptionMessage)
        {
            return new CustomException
            {
                TraceId = traceId,
                Message = $"{exceptionType}: {customMessage}",
                Error = exceptionMessage
            };
        }
    }
}
