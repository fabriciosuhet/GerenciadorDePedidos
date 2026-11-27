using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.ExportarRelatorioMovimentacaoEstoque;

public sealed class ExportarRelatorioMovimentacaoEstoqueQueryHandler(
	IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
	IRelatorioMovimentacaoEstoqueGenerator relatorioGenerator)
	: IRequestHandler<ExportarRelatorioMovimentacaoEstoqueQuery, ArquivoRelatorioDTO>
{
	public async Task<ArquivoRelatorioDTO> Handle(
		ExportarRelatorioMovimentacaoEstoqueQuery request,
		CancellationToken cancellationToken)
	{
		var geradoEmUtc = DateTime.UtcNow;
		var referencia = (request.DataReferencia ?? geradoEmUtc).Date;
		var (dataInicio, dataFimExclusiva) = CalcularIntervalo(request.Periodo, referencia);

		var movimentacoes = await movimentacaoEstoqueRepository.GetByPeriodoAsync(
			dataInicio,
			dataFimExclusiva,
			cancellationToken);

		var itens = movimentacoes
			.Select(me =>
			{
				var precoUnitario = me.PrecoUnitario > 0m
					? me.PrecoUnitario
					: me.Produto?.Preco ?? 0m;
				var valorTotal = me.Quantidade * precoUnitario;

				return new MovimentacaoEstoqueRelatorioItemDTO(
					me.Id,
					me.DataMovimentacao,
					me.TipoMovimentacao,
					me.Motivo,
					me.PedidoId,
					me.ProdutoId,
					me.Produto?.Nome ?? "Produto Desconhecido",
					me.Quantidade,
					precoUnitario,
					valorTotal,
					me.Produto?.Estoque ?? 0,
					me.ClienteId,
					me.Cliente?.NomeCompleto ?? "Desconhecido");
			})
			.ToList();

		var totalEntradas = itens.Count(i => i.TipoMovimentacao == Tipo.Adicao);
		var totalSaidas = itens.Count(i => i.TipoMovimentacao == Tipo.Remocao);
		var quantidadeAdicionada = itens.Where(i => i.TipoMovimentacao == Tipo.Adicao).Sum(i => i.Quantidade);
		var quantidadeRemovida = itens.Where(i => i.TipoMovimentacao == Tipo.Remocao).Sum(i => i.Quantidade);
		var valorEntradas = itens.Where(i => i.TipoMovimentacao == Tipo.Adicao).Sum(i => i.ValorTotal);
		var valorSaidas = itens.Where(i => i.TipoMovimentacao == Tipo.Remocao).Sum(i => i.ValorTotal);

		var resumoPorProduto = itens
			.GroupBy(i => i.ProdutoId)
			.Select(grupo =>
			{
				var primeiro = grupo.First();
				var qtdAdd = grupo.Where(i => i.TipoMovimentacao == Tipo.Adicao).Sum(i => i.Quantidade);
				var qtdRem = grupo.Where(i => i.TipoMovimentacao == Tipo.Remocao).Sum(i => i.Quantidade);
				var valEnt = grupo.Where(i => i.TipoMovimentacao == Tipo.Adicao).Sum(i => i.ValorTotal);
				var valSai = grupo.Where(i => i.TipoMovimentacao == Tipo.Remocao).Sum(i => i.ValorTotal);

				return new ResumoProdutoMovimentacaoDTO(
					primeiro.ProdutoId,
					primeiro.ProdutoNome,
					primeiro.PrecoUnitario,
					primeiro.EstoqueAtualProduto,
					qtdAdd,
					qtdRem,
					qtdAdd - qtdRem,
					valEnt,
					valSai,
					valEnt + valSai);
			})
			.OrderByDescending(r => r.ValorTotalMovimentado)
			.ThenBy(r => r.ProdutoNome)
			.ToList();

		var relatorio = new RelatorioMovimentacaoEstoqueDTO(
			request.Periodo,
			dataInicio,
			dataFimExclusiva.AddDays(-1),
			geradoEmUtc,
			itens.Count,
			totalEntradas,
			totalSaidas,
			quantidadeAdicionada,
			quantidadeRemovida,
			quantidadeAdicionada - quantidadeRemovida,
			valorEntradas,
			valorSaidas,
			valorEntradas - valorSaidas,
			valorEntradas + valorSaidas,
			resumoPorProduto,
			itens);

		return relatorioGenerator.Gerar(relatorio, request.Formato);
	}

	public static (DateTime DataInicio, DateTime DataFimExclusiva) CalcularIntervalo(
		PeriodoRelatorio periodo,
		DateTime dataReferencia)
		=> PeriodoRelatorioHelper.CalcularIntervalo(periodo, dataReferencia);
}

