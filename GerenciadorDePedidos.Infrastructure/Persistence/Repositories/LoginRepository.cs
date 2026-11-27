using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class LoginRepository(GerenciadorDePedidosDbContext context)
	: Repository<Login, int>(context), ILoginRepository
{
	public Task<Login?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Include(l => l.Cliente)
			.SingleOrDefaultAsync(l => l.Email == email, cancellationToken);

	public Task<Login?> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
		=> _dbSet.SingleOrDefaultAsync(l => l.ClienteId == clienteId, cancellationToken);
}
