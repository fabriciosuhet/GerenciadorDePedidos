namespace GerenciadorDePedidos.Core.DTOs;

public sealed record TopProdutoDashboardDTO(
	int ProdutoId,
	string ProdutoNome,
	int QuantidadeVendida,
	decimal FaturamentoTotal);

public sealed record PedidoDashboardStatsDTO(
	int TotalPedidos,
	int PedidosPendentes,
	int PedidosConcluidos,
	int PedidosCancelados,
	decimal FaturamentoTotalConcluido,
	IReadOnlyList<TopProdutoDashboardDTO>? TopProdutos = null,
	int PedidosProcessados = 0,
	int PedidosDevolvidos = 0);
