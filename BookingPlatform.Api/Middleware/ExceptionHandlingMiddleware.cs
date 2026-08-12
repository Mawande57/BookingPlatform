using BookingPlatform.Application.Exceptions;

using System.Net;
using System.Text.Json;


namespace BookingPlatform.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception occurred");

                var (statusCode, message) = ex switch
                {
                    ValidationException => (HttpStatusCode.BadRequest, ex.Message),
                    UnauthorizedException => (HttpStatusCode.Unauthorized, ex.Message),
                    NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                    ConflictException => (HttpStatusCode.Conflict, ex.Message),
                    _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                var response = JsonSerializer.Serialize(new { error = message });
                await context.Response.WriteAsync(response);
            }
        }
    }
}
