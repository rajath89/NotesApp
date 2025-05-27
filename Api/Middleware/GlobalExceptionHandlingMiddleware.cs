using System.Net;
using System.Text.Json;

namespace Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            string traceId = context.Items.ContainsKey("TraceId")
                ? context.Items["TraceId"].ToString()
                : "untraced";

            _logger.LogError(ex, "[{TraceId}] An unhandled exception occurred", traceId);

            await HandleExceptionAsync(context, ex, traceId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An error occurred while processing your request.",
            TraceId = traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

}