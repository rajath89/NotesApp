using Api.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddConditionalAuthentication(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddAuthentication("FakeScheme")
                .AddScheme<AuthenticationSchemeOptions, StubJwtAuthHandler>("FakeScheme", null);
        }
        else
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["JWT:Authority"];
                    options.Audience = configuration["JWT:Audience"];
                });
        }

        return services;
    }
}