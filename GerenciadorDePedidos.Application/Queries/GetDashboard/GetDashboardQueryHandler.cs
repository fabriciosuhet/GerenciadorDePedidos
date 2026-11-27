using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Helpers;
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
		DateTime? dataInicio = null;
		DateTime? dataFimExclusiva = null;
		DateTime? dataFimInclusiva = null;

		if (request.Periodo.HasValue)
		{
			var referencia = (request.DataReferencia ?? DateTime.UtcNow).Date;
			var (inicio, fimExclusiva) = PeriodoRelatorioHelper.CalcularIntervalo(request.Periodo.Value, referencia);
			dataInicio = inicio;
			dataFimExclusiva = fimExclusiva;
			dataFimInclusiva = fimExclusiva.AddDays(-1);
		}

		var topLimit = Math.Clamp(request.TopProdutos, 1, 20);

		var totalClientes = await clienteRepository.GetCountAsync(cancellationToken);
		var totalProdutos = await produtoRepository.GetCountAsync(cancellationToken);
		var estoqueBaixo = await produtoRepository.GetLowStockCountAsync(cancellationToken: cancellationToken);
		var pedidoStats = await pedidoRepository.GetDashboardStatsAsync(
			dataInicio,
			dataFimExclusiva,
			topLimit,
			cancellationToken);

		var ticketMedio = pedidoStats.PedidosConcluidos > 0
			? Math.Round(pedidoStats.FaturamentoTotalConcluido / pedidoStats.PedidosConcluidos, 2)
			: 0m;

		var taxaCancelamento = pedidoStats.TotalPedidos > 0
			? Math.Round((decimal)pedidoStats.PedidosCancelados / pedidoStats.TotalPedidos * 100m, 2)
			: 0m;

		return new DashboardViewModel(
			totalClientes,
			totalProdutos,
			estoqueBaixo,
			pedidoStats.TotalPedidos,
			pedidoStats.PedidosPendentes,
			pedidoStats.PedidosConcluidos,
			pedidoStats.PedidosCancelados,
			pedidoStats.FaturamentoTotalConcluido,
			ticketMedio,
			taxaCancelamento,
			request.Periodo,
			dataInicio,
			dataFimInclusiva,
			pedidoStats.TopProdutos ?? []);
	}
}
