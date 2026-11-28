using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace GerenciadorDePedidos.Infrastructure.Cache;

public class CacheService(IMemoryCache memoryCache) : ICacheService
{
	private static readonly ConditionalWeakTable<IMemoryCache, ConcurrentDictionary<string, byte>> KeysByCache = new();

	private ConcurrentDictionary<string, byte> TrackedKeys => KeysByCache.GetOrCreateValue(memoryCache);

	public T? Get<T>(string key) => memoryCache.Get<T>(key);

	public void Set<T>(string key, T value, TimeSpan expiration)
	{
		var trackedKeys = TrackedKeys;
		trackedKeys[key] = 0;

		var options = new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = expiration,
			Size = 1
		};
		options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
		{
			if (evictedKey is string keyString)
				trackedKeys.TryRemove(keyString, out _);
		});

		memoryCache.Set(key, value, options);
	}

	public bool TryGet<T>(string key, out T? value) => memoryCache.TryGetValue(key, out value);

	public void Remove(string key)
	{
		memoryCache.Remove(key);
		TrackedKeys.TryRemove(key, out _);
	}

	public void RemoveMany(IEnumerable<string> keys)
	{
		foreach (var key in keys)
			Remove(key);
	}

	public void RemoveByPrefix(string prefix)
	{
		var matchingKeys = TrackedKeys.Keys
			.Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
			.ToList();

		foreach (var key in matchingKeys)
			Remove(key);
	}
}
