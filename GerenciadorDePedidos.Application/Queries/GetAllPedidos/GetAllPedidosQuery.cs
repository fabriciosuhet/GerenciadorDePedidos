using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public sealed record GetAllPedidosQuery(
	string? Query = null,
	int PageNumber = 1,
	int PageSize = 10) : IRequest<PagedResultModel<PedidoViewModel>>;