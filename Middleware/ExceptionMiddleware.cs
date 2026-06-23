using System.Net;
using System.Text.Json;
using PetHealthAPI.Models;

namespace PetHealthAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
                _logger.LogCritical("ALERT: API Critical Failure detected! CorrelationID: {CorrelationId}. Error: {Message}", correlationId, ex.Message);

                _logger.LogError(ex, "Error occurred ! CorrelationID: {CorrelationId}. Message: {Message}", correlationId, ex.Message);

                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = ApiResponse<object>.Failure(
                $"Internal Server Error. Your Complaint Number (Tracking ID): {correlationId}",
                new List<string> { exception.Message }
            );

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}