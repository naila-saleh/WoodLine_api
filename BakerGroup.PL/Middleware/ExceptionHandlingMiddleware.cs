using System.Net;
using System.Text.Json;

namespace BakerGroup.PL.Middleware;

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
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";

        // You can add custom exceptions here
        // if (exception is UnauthorizedAccessException) ...

        response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(new { 
            StatusCode = statusCode, 
            Message = message,
            Detail = exception.Message // In production, don't show full details
        });

        return context.Response.WriteAsync(result);
    }
}
