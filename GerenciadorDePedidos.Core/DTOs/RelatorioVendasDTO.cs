using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.DTOs;

public sealed record RelatorioVendaPedidoItemDTO(
	int PedidoId,
	DateTime DataPedido,
	Guid ClienteId,
	string ClienteNome,
	Status Status,
	int QuantidadeItens,
	int TotalUnidades,
	decimal ValorTotal);

public sealed record RelatorioVendaProdutoRankingDTO(
	int ProdutoId,
	string ProdutoNome,
	int QuantidadeVendida,
	decimal FaturamentoTotal,
	decimal ParticipacaoPercentual);

public sealed record RelatorioVendaClienteRankingDTO(
	Guid ClienteId,
	string ClienteNome,
	int PedidosConcluidos,
	int PedidosCancelados,
	decimal ValorTotalComprado,
	decimal TicketMedio);

public sealed record RelatorioVendasDTO(
	PeriodoRelatorio Periodo,
	DateTime DataInicio,
	DateTime DataFim,
	DateTime GeradoEmUtc,
	int TotalPedidos,
	int PedidosPendentes,
	int PedidosProcessados,
	int PedidosConcluidos,
	int PedidosCancelados,
	int TotalUnidadesVendidas,
	decimal FaturamentoTotalConcluido,
	decimal ValorTotalCancelado,
	decimal TicketMedio,
	decimal TaxaCancelamentoPercentual,
	IReadOnlyList<RelatorioVendaProdutoRankingDTO> TopProdutos,
	IReadOnlyList<RelatorioVendaClienteRankingDTO> ClientesMaisAtivos,
	IReadOnlyList<RelatorioVendaPedidoItemDTO> Pedidos,
	int PedidosDevolvidos = 0,
	decimal ValorTotalDevolvido = 0m);

