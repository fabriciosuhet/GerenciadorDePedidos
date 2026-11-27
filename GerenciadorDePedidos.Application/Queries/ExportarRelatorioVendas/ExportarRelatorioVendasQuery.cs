using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.ExportarRelatorioVendas;

public sealed record ExportarRelatorioVendasQuery(
	PeriodoRelatorio Periodo,
	FormatoRelatorio Formato,
	DateTime? DataReferencia = null) : IRequest<ArquivoRelatorioDTO>;

