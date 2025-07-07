using Api.Middleware;

namespace Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
    
    public static IApplicationBuilder UseAplicationInterceptor(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AplicationInterceptorMiddleware>();
    }
}