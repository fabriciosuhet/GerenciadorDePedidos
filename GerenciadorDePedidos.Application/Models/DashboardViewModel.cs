namespace GerenciadorDePedidos.Application.Models;

public sealed record DashboardViewModel(
	int TotalClientes,
	int TotalProdutos,
	int ProdutosComEstoqueBaixo,
	int TotalPedidos,
	int PedidosPendentes,
	int PedidosConcluidos,
	int PedidosCancelados,
	decimal FaturamentoTotalConcluido);
