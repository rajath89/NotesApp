using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Caching;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ICacheService, InMemoryCacheService>();
        services.AddScoped<IGlobalWorkspaceCache, GlobalWorkspaceCache>();
        
        return services;
    }
}