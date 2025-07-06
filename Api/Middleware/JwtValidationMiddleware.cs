using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Api.Middleware;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _jwtSecret;

    public JwtValidationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _jwtSecret = configuration["Jwt:Secret"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
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
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid or expired JWT token.");
                return;
            }
        }
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("SSOTOKEN cookie missing.");
            return;
        }

        await _next(context);
    }
}