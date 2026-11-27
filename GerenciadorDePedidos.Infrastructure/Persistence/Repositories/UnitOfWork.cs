using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class UnitOfWork(GerenciadorDePedidosDbContext context) : IUnitOfWork
{
	public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		=> context.SaveChangesAsync(cancellationToken);

	public async Task ExecuteInTransactionAsync(
		Func<CancellationToken, Task> action,
		CancellationToken cancellationToken = default)
	{
		if (!context.Database.IsRelational())
		{
			await action(cancellationToken);
			return;
		}

		var strategy = context.Database.CreateExecutionStrategy();
		await strategy.ExecuteAsync(async ct =>
		{
			await using var transaction = await context.Database.BeginTransactionAsync(ct);
			try
			{
				await action(ct);
				await transaction.CommitAsync(ct);
			}
			catch
			{
				await transaction.RollbackAsync(ct);
				throw;
			}
		}, cancellationToken);
	}
}
