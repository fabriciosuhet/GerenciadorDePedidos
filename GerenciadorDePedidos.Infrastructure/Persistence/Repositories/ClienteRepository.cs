using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class ClienteRepository(GerenciadorDePedidosDbContext context)
	: Repository<Cliente, Guid>(context), IClienteRepository
{
	public Task<int> GetCountAsync(string? query, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query).CountAsync(cancellationToken);

	public Task<List<Cliente>> GetPagedAsync(string? query, int skip, int take, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query)
			.OrderBy(c => c.NomeCompleto)
			.Skip(skip)
			.Take(take)
			.ToListAsync(cancellationToken);

	public Task<Cliente?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.Include(c => c.Pedidos)
				.ThenInclude(p => p.ItensPedidos)
					.ThenInclude(ip => ip.Produto)
			.AsNoTracking()
			.SingleOrDefaultAsync(c => c.Ativo && c.Id == id, cancellationToken);

	public async Task<bool> HasVinculosAsync(Guid clienteId, CancellationToken cancellationToken = default)
		=> await _context.Set<Pedido>().AsNoTracking().AnyAsync(p => p.ClienteId == clienteId, cancellationToken)
			|| await _context.Set<MovimentacaoEstoque>().AsNoTracking().AnyAsync(m => m.ClienteId == clienteId, cancellationToken);

	private IQueryable<Cliente> BuildFilteredQuery(string? query)
	{
		var q = _dbSet.AsNoTracking();
		if (!string.IsNullOrWhiteSpace(query))
			q = q.Where(c => c.NomeCompleto.Contains(query) || c.Telefone.Contains(query));

		return q;
	}
}