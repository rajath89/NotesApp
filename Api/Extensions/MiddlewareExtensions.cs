using Api.Middleware;

namespace Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTracing(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTracingMiddleware>();
    }

    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}