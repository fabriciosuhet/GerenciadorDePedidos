using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllClientes;

public sealed record GetAllClientesQuery(
	string? Query = null,
	int PageNumber = 1,
	int PageSize = 10) : IRequest<PagedResultModel<ClienteViewModel>>;