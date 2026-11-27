using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GerenciadorDePedidos.API.HealthChecks;

public sealed class DatabaseHealthCheck(GerenciadorDePedidosDbContext dbContext) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
			return canConnect
				? HealthCheckResult.Healthy("Conexão com o banco de dados ativa.")
				: HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("Erro ao verificar conectividade com o banco de dados.", ex);
		}
	}
}

