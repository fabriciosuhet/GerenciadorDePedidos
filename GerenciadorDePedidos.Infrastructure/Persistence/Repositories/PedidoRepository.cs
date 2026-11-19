using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorDePedidos.Infrastructure.Persistence.Repositories;

public class PedidoRepository(GerenciadorDePedidosDbContext context)
	: Repository<Pedido, int>(context), IPedidoRepository
{
	public Task<int> GetCountAsync(string? query, Guid? clienteId = null, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query, clienteId).CountAsync(cancellationToken);

	public Task<List<Pedido>> GetPagedAsync(string? query, int skip, int take, Guid? clienteId = null, CancellationToken cancellationToken = default)
		=> BuildFilteredQuery(query, clienteId)
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

	public async Task<PedidoDashboardStatsDTO> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
	{
		var pedidos = _dbSet.AsNoTracking();

		var total = await pedidos.CountAsync(cancellationToken);
		var pendentes = await pedidos.CountAsync(p => p.Status == Status.Pendente, cancellationToken);
		var concluidos = await pedidos.CountAsync(p => p.Status == Status.Concluido, cancellationToken);
		var cancelados = await pedidos.CountAsync(p => p.Status == Status.Cancelado, cancellationToken);
		var faturamento = await pedidos
			.Where(p => p.Status == Status.Concluido)
			.SelectMany(p => p.ItensPedidos)
			.SumAsync(i => (decimal?)i.Total, cancellationToken) ?? 0m;

		return new PedidoDashboardStatsDTO(total, pendentes, concluidos, cancelados, faturamento);
	}

	private IQueryable<Pedido> BuildFilteredQuery(string? query, Guid? clienteId)
	{
		var q = _dbSet.AsNoTracking();

		if (clienteId.HasValue)
			q = q.Where(p => p.ClienteId == clienteId.Value);

		if (!string.IsNullOrWhiteSpace(query) && Enum.TryParse<Status>(query, ignoreCase: true, out var status))
			q = q.Where(p => p.Status == status);

		return q;
	}
}