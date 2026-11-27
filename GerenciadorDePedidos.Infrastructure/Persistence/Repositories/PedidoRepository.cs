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
			.IgnoreQueryFilters()
			.Include(p => p.ItensPedidos)
				.ThenInclude(i => i.Produto)
			.OrderByDescending(p => p.DataPedido)
			.Skip(skip)
			.Take(take)
			.ToListAsync(cancellationToken);

	public Task<Pedido?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.Include(p => p.ItensPedidos)
				.ThenInclude(i => i.Produto)
			.Include(p => p.Cliente)
			.AsNoTracking()
			.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

	public Task<Pedido?> GetByIdWithItensAsync(int id, CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.Include(p => p.ItensPedidos)
			.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

	public Task<List<Pedido>> GetByPeriodoAsync(
		DateTime dataInicio,
		DateTime dataFimExclusiva,
		CancellationToken cancellationToken = default)
		=> _dbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Include(p => p.Cliente)
			.Include(p => p.ItensPedidos)
				.ThenInclude(i => i.Produto)
			.Where(p => p.DataPedido >= dataInicio && p.DataPedido < dataFimExclusiva)
			.OrderByDescending(p => p.DataPedido)
			.ToListAsync(cancellationToken);

	public async Task<PedidoDashboardStatsDTO> GetDashboardStatsAsync(
		DateTime? dataInicio = null,
		DateTime? dataFimExclusiva = null,
		int topProdutos = 5,
		CancellationToken cancellationToken = default)
	{
		var pedidos = _dbSet.IgnoreQueryFilters().AsNoTracking();

		if (dataInicio.HasValue && dataFimExclusiva.HasValue)
		{
			var inicio = dataInicio.Value;
			var fim = dataFimExclusiva.Value;
			pedidos = pedidos.Where(p => p.DataPedido >= inicio && p.DataPedido < fim);
		}

		var total = await pedidos.CountAsync(cancellationToken);
		var pendentes = await pedidos.CountAsync(p => p.Status == Status.Pendente, cancellationToken);
		var concluidos = await pedidos.CountAsync(p => p.Status == Status.Concluido, cancellationToken);
		var cancelados = await pedidos.CountAsync(p => p.Status == Status.Cancelado, cancellationToken);
		var faturamento = await pedidos
			.Where(p => p.Status == Status.Concluido)
			.SelectMany(p => p.ItensPedidos)
			.SumAsync(i => (decimal?)i.Total, cancellationToken) ?? 0m;

		var itensConcluidos = await pedidos
			.Where(p => p.Status == Status.Concluido)
			.SelectMany(p => p.ItensPedidos)
			.Select(i => new
			{
				i.ProdutoId,
				ProdutoNome = i.Produto != null ? i.Produto.Nome : "Produto Desconhecido",
				i.Quantidade,
				i.Total
			})
			.ToListAsync(cancellationToken);

		var ranking = itensConcluidos
			.GroupBy(i => new { i.ProdutoId, i.ProdutoNome })
			.Select(g => new TopProdutoDashboardDTO(
				g.Key.ProdutoId,
				g.Key.ProdutoNome,
				g.Sum(x => x.Quantidade),
				g.Sum(x => x.Total)))
			.OrderByDescending(p => p.FaturamentoTotal)
			.ThenByDescending(p => p.QuantidadeVendida)
			.ThenBy(p => p.ProdutoNome)
			.Take(topProdutos)
			.ToList();

		return new PedidoDashboardStatsDTO(total, pendentes, concluidos, cancelados, faturamento, ranking);
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