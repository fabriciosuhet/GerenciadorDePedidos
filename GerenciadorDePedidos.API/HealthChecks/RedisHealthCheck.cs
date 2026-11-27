using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace GerenciadorDePedidos.API.HealthChecks;

public sealed class RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var latency = await connectionMultiplexer.GetDatabase().PingAsync();
			return HealthCheckResult.Healthy(
				"Conexão com o Redis ativa.",
				new Dictionary<string, object> { ["latencyMs"] = Math.Round(latency.TotalMilliseconds, 2) });
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("Erro ao conectar no Redis.", ex);
		}
	}
}

