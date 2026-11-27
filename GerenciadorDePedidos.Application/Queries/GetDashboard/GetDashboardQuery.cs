using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Enums;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetDashboard;

public sealed record GetDashboardQuery(
	PeriodoRelatorio? Periodo = null,
	DateTime? DataReferencia = null,
	int TopProdutos = 5) : IRequest<DashboardViewModel>;
