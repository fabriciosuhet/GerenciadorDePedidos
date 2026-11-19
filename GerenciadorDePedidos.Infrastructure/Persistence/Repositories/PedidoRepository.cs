using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class PedidoRepository(GerenciadorDePedidosDbContext context)
	: Repository<Pedido, int>(context), IPedidoRepository
{
	public Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query).CountAsync(cancellationToken);

	public Task<List<Pedido>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query)
			.Include(p => p.ItensPedidos)
				.ThenInclude(i => i.Produto)
			.OrderByDescending(p => p.DataPedido)
			.Skip(skip)
			.Take(take)
			.ToListAsync(cancellationToken);

	public Task<Pedido?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
		=> _dbSet
			.Include(p => p.ItensPedidos)
				.ThenInclude(i => i.Produto)
			.Include(p => p.Cliente)
			.AsNoTracking()
			.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

	public Task<Pedido?> GetByIdWithItensAsync(int id, CancellationToken cancellationToken = default)
		=> _dbSet
			.Include(p => p.ItensPedidos)
			.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

	private IQueryable<Pedido> BuildFilteredQuery(string? query)
	{
		var q = _dbSet.AsNoTracking();

		if (!string.IsNullOrWhiteSpace(query) && Enum.TryParse<Status>(query, ignoreCase: true, out var status))
			q = q.Where(p => p.Status == status);

		return q;
	}
}