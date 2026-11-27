using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.DTOs;

public sealed record MovimentacaoEstoqueRelatorioItemDTO(
	int Id,
	DateTime DataMovimentacao,
	Tipo TipoMovimentacao,
	int ProdutoId,
	string ProdutoNome,
	int Quantidade,
	decimal PrecoUnitario,
	decimal ValorTotal,
	int EstoqueAtualProduto,
	Guid ClienteId,
	string ClienteNome);

public sealed record ResumoProdutoMovimentacaoDTO(
	int ProdutoId,
	string ProdutoNome,
	decimal PrecoUnitario,
	int EstoqueAtual,
	int QuantidadeAdicionada,
	int QuantidadeRemovida,
	int SaldoQuantidade,
	decimal ValorEntradas,
	decimal ValorSaidas,
	decimal ValorTotalMovimentado);

public sealed record RelatorioMovimentacaoEstoqueDTO(
	PeriodoRelatorio Periodo,
	DateTime DataInicio,
	DateTime DataFim,
	DateTime GeradoEmUtc,
	int TotalMovimentacoes,
	int TotalEntradas,
	int TotalSaidas,
	int QuantidadeTotalAdicionada,
	int QuantidadeTotalRemovida,
	int SaldoQuantidade,
	decimal ValorTotalEntradas,
	decimal ValorTotalSaidas,
	decimal SaldoFinanceiro,
	decimal ValorTotalMovimentado,
	IReadOnlyList<ResumoProdutoMovimentacaoDTO> ResumoPorProduto,
	IReadOnlyList<MovimentacaoEstoqueRelatorioItemDTO> Movimentacoes);

