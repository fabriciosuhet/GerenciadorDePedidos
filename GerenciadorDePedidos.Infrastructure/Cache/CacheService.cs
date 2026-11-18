using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace GerenciadorDePedidos.Infrastructure.Cache;

public class CacheService(IMemoryCache memoryCache) : ICacheService
{
	public T? Get<T>(string key) => memoryCache.Get<T>(key);

	public void Set<T>(string key, T value, TimeSpan expiration) => memoryCache.Set(key, value, expiration);

	public bool TryGet<T>(string key, out T? value) => memoryCache.TryGetValue(key, out value);

	public void Remove(string key) => memoryCache.Remove(key);
}