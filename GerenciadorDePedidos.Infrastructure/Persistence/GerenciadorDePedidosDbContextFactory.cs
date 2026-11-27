using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GerenciadorDePedidos.Infrastructure.Persistence;

public class GerenciadorDePedidosDbContextFactory : IDesignTimeDbContextFactory<GerenciadorDePedidosDbContext>
{
	public GerenciadorDePedidosDbContextFactory() { }

	public GerenciadorDePedidosDbContext CreateDbContext(string[] args)
	{
		var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException(
				"Defina a variável de ambiente 'ConnectionStrings__DefaultConnection' antes de executar operações de design-time do EF Core.");
		}

		var optionsBuilder = new DbContextOptionsBuilder<GerenciadorDePedidosDbContext>();
		optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));

		return new GerenciadorDePedidosDbContext(optionsBuilder.Options);
	}
}