using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class ProdutoRepository(GerenciadorDePedidosDbContext context)
	: Repository<Produto, int>(context), IProdutoRepository
{
	public Task<List<Produto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
	{
		var distinctIds = ids.Distinct().ToList();
		return _dbSet
			.AsNoTracking()
			.Where(p => distinctIds.Contains(p.Id))
			.ToListAsync(cancellationToken);
	}

	public Task<List<Produto>> GetByIdsForUpdateAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
	{
		var distinctIds = ids.Distinct().ToList();
		return _dbSet
			.Where(p => distinctIds.Contains(p.Id))
			.ToListAsync(cancellationToken);
	}

	public Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query).CountAsync(cancellationToken);

	public Task<List<Produto>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query)
			.OrderBy(p => p.Id)
			.Skip(skip)
			.Take(take)
			.ToListAsync(cancellationToken);

	public Task<int> GetLowStockCountAsync(int threshold = 5, CancellationToken cancellationToken = default)
		=> _dbSet.AsNoTracking().CountAsync(p => p.Estoque <= threshold, cancellationToken);

	public Task<bool> HasPedidoVinculadoAsync(int produtoId, CancellationToken cancellationToken = default)
		=> _context.Set<ItemPedido>()
			.AsNoTracking()
			.AnyAsync(ip => ip.ProdutoId == produtoId, cancellationToken);

	private IQueryable<Produto> BuildFilteredQuery(string? query)
	{
		var q = _dbSet.AsNoTracking();
		if (!string.IsNullOrWhiteSpace(query))
			q = q.Where(p => p.Nome.Contains(query));

		return q;
	}
}