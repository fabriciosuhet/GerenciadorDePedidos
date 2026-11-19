namespace GerenciadorDePedidos.Core.DTOs;

public sealed record PedidoDashboardStatsDTO(
	int TotalPedidos,
	int PedidosPendentes,
	int PedidosConcluidos,
	int PedidosCancelados,
	decimal FaturamentoTotalConcluido);
