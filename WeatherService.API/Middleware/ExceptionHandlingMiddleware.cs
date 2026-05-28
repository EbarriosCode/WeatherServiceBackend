using System.Net;
using System.Text.Json;

namespace WeatherService.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this._next(context);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                ArgumentException or ArgumentOutOfRangeException => (HttpStatusCode.BadRequest, "Invalid request parameters."),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found."),
                HttpRequestException => (HttpStatusCode.BadGateway, "External weather service unavailable."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                this._logger.LogError(exception, "Unhandled exception at {Path}", context.Request.Path);
            else
                this._logger.LogWarning("Handled exception at {Path}: {Message}", context.Request.Path, exception.Message);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problem = new
            {
                type = $"https://httpstatuses.com/{(int)statusCode}",
                title,
                status = (int)statusCode,
                detail = exception.Message,
                instance = context.Request.Path.Value
            };

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }
    }
}
