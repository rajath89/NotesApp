using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Caching;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromMinutes(10);

    public InMemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T Get<T>(string key)
    {
        return _memoryCache.Get<T>(key);
    }

    public void Set<T>(string key, T value)
    {
        _memoryCache.Set(key, value, _defaultCacheTime);
    }

    public void Set<T>(string key, T value, TimeSpan expirationTime)
    {
        _memoryCache.Set(key, value, expirationTime);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        return _memoryCache.TryGetValue(key, out value);
    }
}