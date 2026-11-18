using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Seeders;

/// <summary>
/// Garante a criação do primeiro administrador a partir de configuração/variáveis de ambiente,
/// evitando exposição de endpoint público para criação de Admin.
/// </summary>
public static class AdminSeeder
{
	private static readonly SemaphoreSlim SeedLock = new(1, 1);

	public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
	{
		var configuration = serviceProvider.GetRequiredService<IConfiguration>();
		var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

		var email = configuration["AdminSeed:Email"];
		var password = configuration["AdminSeed:Password"];

		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
		{
			logger.LogInformation("AdminSeed não configurado; seed de administrador ignorado.");
			return;
		}

		await SeedLock.WaitAsync(cancellationToken);
		try
		{
			var dbContext = serviceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();
			var authService = serviceProvider.GetRequiredService<IAuthService>();

			var hasAdmin = await dbContext.Set<Login>()
				.AsNoTracking()
				.AnyAsync(l => l.Role == Role.Admin, cancellationToken);

			if (hasAdmin)
				return;

			var nomeCompleto = configuration["AdminSeed:NomeCompleto"] ?? "Administrador";
			var telefone = configuration["AdminSeed:Telefone"] ?? "00000000000";

			var admin = new Cliente(nomeCompleto, telefone, Role.Admin);
			var login = new Login(admin.Id, email, authService.HashPassword(password), Role.Admin);

			await dbContext.Clientes.AddAsync(admin, cancellationToken);
			await dbContext.Set<Login>().AddAsync(login, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			logger.LogInformation("Administrador inicial criado com o e-mail {Email}.", email);
		}
		finally
		{
			SeedLock.Release();
		}
	}
}