using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.ExportarRelatorioMovimentacaoEstoque;

public sealed record ExportarRelatorioMovimentacaoEstoqueQuery(
	PeriodoRelatorio Periodo,
	FormatoRelatorio Formato,
	DateTime? DataReferencia = null) : IRequest<ArquivoRelatorioDTO>;

