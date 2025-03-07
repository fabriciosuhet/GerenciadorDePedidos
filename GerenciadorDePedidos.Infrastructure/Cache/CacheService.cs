using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace GerenciadorDePedidos.Infrastructure.Cache;

public class CacheService : ICacheService
{
	private readonly IMemoryCache _memoryCache;

	public CacheService(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}

	public T Get<T>(string key)
	{
		return _memoryCache.Get<T>(key);
	}

	public void Set<T>(string key, T value, TimeSpan expiration)
	{
		_memoryCache.Set(key, value, expiration);
	}

	public bool TryGet<T>(string key, out T value)
	{
		return _memoryCache.TryGetValue(key, out value);
	}
}