using System.Text.Json;
using GerenciadorDePedidos.Core.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GerenciadorDePedidos.Infrastructure.Cache;

public sealed class RedisCacheService(
	IConnectionMultiplexer connectionMultiplexer,
	ILogger<RedisCacheService> logger,
	string instancePrefix = "GerenciadorDePedidos:") : ICacheService
{
	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private IDatabase Database => connectionMultiplexer.GetDatabase();

	public T? Get<T>(string key)
		=> TryGet<T>(key, out var value) ? value : default;

	public void Set<T>(string key, T value, TimeSpan expiration)
	{
		try
		{
			var json = JsonSerializer.Serialize(value, SerializerOptions);
			Database.StringSet(BuildKey(key), json, expiration);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Falha ao gravar chave {CacheKey} no Redis.", key);
		}
	}

	public bool TryGet<T>(string key, out T? value)
	{
		try
		{
			var redisValue = Database.StringGet(BuildKey(key));
			if (!redisValue.HasValue)
			{
				value = default;
				return false;
			}

			value = JsonSerializer.Deserialize<T>(redisValue.ToString(), SerializerOptions);
			return value is not null;
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Falha ao ler chave {CacheKey} no Redis; seguindo sem cache.", key);
			value = default;
			return false;
		}
	}

	public void Remove(string key)
	{
		try
		{
			Database.KeyDelete(BuildKey(key));
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Falha ao remover chave {CacheKey} no Redis.", key);
		}
	}

	public void RemoveMany(IEnumerable<string> keys)
	{
		try
		{
			var redisKeys = keys
				.Where(k => !string.IsNullOrWhiteSpace(k))
				.Distinct(StringComparer.Ordinal)
				.Select(k => (RedisKey)BuildKey(k))
				.ToArray();

			if (redisKeys.Length > 0)
				Database.KeyDelete(redisKeys);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Falha ao remover lote de chaves no Redis.");
		}
	}

	public void RemoveByPrefix(string prefix)
	{
		try
		{
			var db = Database;
			var pattern = $"{BuildKey(prefix)}*";

			foreach (var endpoint in connectionMultiplexer.GetEndPoints())
			{
				var server = connectionMultiplexer.GetServer(endpoint);
				if (!server.IsConnected || server.IsReplica)
					continue;

				var keys = server.Keys(database: db.Database, pattern: pattern).ToArray();
				if (keys.Length > 0)
					db.KeyDelete(keys);
			}
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Falha ao remover chaves com prefixo {CachePrefix} no Redis.", prefix);
		}
	}

	private string BuildKey(string key) => $"{instancePrefix}{key}";
}

