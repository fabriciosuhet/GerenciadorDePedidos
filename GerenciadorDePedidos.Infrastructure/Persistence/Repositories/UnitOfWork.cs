using GerenciadorDePedidos.Core.Repositories;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class UnitOfWork(GerenciadorDePedidosDbContext context) : IUnitOfWork
{
	public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		=> context.SaveChangesAsync(cancellationToken);
}
