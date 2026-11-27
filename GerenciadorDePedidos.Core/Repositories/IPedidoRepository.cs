using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Core.Repositories;

public interface IPedidoRepository : IRepository<Pedido, int>
{
	Task<int> GetCountAsync(string? query, Guid? clienteId = null, CancellationToken cancellationToken = default);
	Task<List<Pedido>> GetPagedAsync(string? query, int skip, int take, Guid? clienteId = null, CancellationToken cancellationToken = default);
	Task<Pedido?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default);
	Task<Pedido?> GetByIdWithItensAsync(int id, CancellationToken cancellationToken = default);
	Task<PedidoDashboardStatsDTO> GetDashboardStatsAsync(
		DateTime? dataInicio = null,
		DateTime? dataFimExclusiva = null,
		int topProdutos = 5,
		CancellationToken cancellationToken = default);
	Task<List<Pedido>> GetByPeriodoAsync(
		DateTime dataInicio,
		DateTime dataFimExclusiva,
		CancellationToken cancellationToken = default);
}