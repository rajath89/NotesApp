namespace Api.Middleware;

public class RequestTracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTracingMiddleware> _logger;

    public RequestTracingMiddleware(RequestDelegate next, ILogger<RequestTracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only add tracing for authenticated requests
        if (context.User.Identity.IsAuthenticated)
        {
            // Generate trace ID
            string traceId = Guid.NewGuid().ToString();

            // Add to HttpContext items so it can be accessed by controllers
            context.Items["TraceId"] = traceId;

            _logger.LogInformation("Request {Method} {Path} started with TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);
        }

        await _next(context);
    }

}