using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Api.Middleware;

public class AplicationInterceptorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AplicationInterceptorMiddleware> _logger;
    private readonly string _jwtSecret;

    public AplicationInterceptorMiddleware(RequestDelegate next, ILogger<AplicationInterceptorMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _jwtSecret = configuration["JwtSettings:SecretKey"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.Value?.ToLower() ?? string.Empty;
        
        // Check if the request is for a public endpoint
        if (requestPath.Contains("public"))
        {
            // For public endpoints, generate a new trace ID and proceed without JWT validation
            string traceId = Guid.NewGuid().ToString();
            context.Items["TraceId"] = traceId;
            
            _logger.LogInformation("Public request {Method} {Path} started with TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);
            
            await _next(context);
            return;
        }

        // For non-public endpoints, validate JWT token
        if (context.Request.Cookies.TryGetValue("SSOTOKEN", out string token))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSecret);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                context.User = principal;

                // Extract trace ID from JWT token if present, otherwise generate new one
                var jwtToken = validatedToken as JwtSecurityToken;
                string traceId = jwtToken?.Claims?.FirstOrDefault(x => x.Type == "traceId")?.Value;
                
                if (string.IsNullOrEmpty(traceId))
                {
                    traceId = Guid.NewGuid().ToString();
                }

                context.Items["TraceId"] = traceId;

                _logger.LogInformation("Authenticated request {Method} {Path} started with TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, traceId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("JWT validation failed for request {Method} {Path}: {Error}",
                    context.Request.Method, context.Request.Path, ex.Message);
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid or expired JWT token.");
                return;
            }
        }
        else
        {
            _logger.LogWarning("SSOTOKEN cookie missing for request {Method} {Path}",
                context.Request.Method, context.Request.Path);
            
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("SSOTOKEN cookie missing.");
            return;
        }

        await _next(context);
    }
}