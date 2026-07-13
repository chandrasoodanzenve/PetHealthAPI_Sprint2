using System.Net;
using System.Text.Json;
using PetHealthAPI.Models;
using Serilog;
using System.Diagnostics;

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
            var watch = Stopwatch.StartNew();

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
            finally
            {
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                var endpoint = context.Request.Path;
                AnomalyDetection(elapsedMs, endpoint);
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
        private static DateTime _lastAlertTime = DateTime.MinValue;
        public static void AnomalyDetection(long elapsedMs, string endpoint)
        {
            const int P95_Threshold = 500; 
            
            if (elapsedMs > P95_Threshold)
            {
                if ((DateTime.UtcNow - _lastAlertTime).TotalSeconds > 60)
                {
                    Log.Error("!!! ANOMALY DETECTED !!! Endpoint: {Endpoint} | Latency: {Duration}ms | Status: P95 Violated", 
                        endpoint, elapsedMs);
                    
                    _lastAlertTime = DateTime.UtcNow;
                }
            }
        }
    }
}