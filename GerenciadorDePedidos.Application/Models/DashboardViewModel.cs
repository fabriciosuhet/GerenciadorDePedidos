using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Application.Models;

public sealed record DashboardViewModel(
	int TotalClientes,
	int TotalProdutos,
	int ProdutosComEstoqueBaixo,
	int TotalPedidos,
	int PedidosPendentes,
	int PedidosConcluidos,
	int PedidosCancelados,
	decimal FaturamentoTotalConcluido,
	decimal TicketMedio = 0m,
	decimal TaxaCancelamentoPercentual = 0m,
	PeriodoRelatorio? Periodo = null,
	DateTime? DataInicio = null,
	DateTime? DataFim = null,
	IReadOnlyList<TopProdutoDashboardDTO>? TopProdutos = null,
	int PedidosProcessados = 0,
	int PedidosDevolvidos = 0);
