using System.Collections.Concurrent;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Caching;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromMinutes(30);
    private readonly ConcurrentBag<string> _cacheKeys = new();

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
        _cacheKeys.Add(key);
    }

    public void Set<T>(string key, T value, TimeSpan expirationTime)
    {
        _memoryCache.Set(key, value, expirationTime);
        _cacheKeys.Add(key);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        return _memoryCache.TryGetValue(key, out value);
    }

    public void RemoveByPattern(string pattern)
    {
        var keysToRemove = _cacheKeys.Where(k => k.Contains(pattern)).ToList();
        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
        }
    }

    public void RemoveMultiple(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            _memoryCache.Remove(key);
        }
    }
}