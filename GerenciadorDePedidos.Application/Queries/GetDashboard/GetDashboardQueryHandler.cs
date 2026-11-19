using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler(
	IClienteRepository clienteRepository,
	IProdutoRepository produtoRepository,
	IPedidoRepository pedidoRepository) : IRequestHandler<GetDashboardQuery, DashboardViewModel>
{
	public async Task<DashboardViewModel> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
	{
		var totalClientes = await clienteRepository.GetCountAsync(cancellationToken);
		var totalProdutos = await produtoRepository.GetCountAsync(cancellationToken);
		var estoqueBaixo = await produtoRepository.GetLowStockCountAsync(cancellationToken: cancellationToken);
		var pedidoStats = await pedidoRepository.GetDashboardStatsAsync(cancellationToken);

		return new DashboardViewModel(
			totalClientes,
			totalProdutos,
			estoqueBaixo,
			pedidoStats.TotalPedidos,
			pedidoStats.PedidosPendentes,
			pedidoStats.PedidosConcluidos,
			pedidoStats.PedidosCancelados,
			pedidoStats.FaturamentoTotalConcluido);
	}
}
