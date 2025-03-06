using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public class GetAllPedidosQuery : IRequest<PagedResultModel<PedidoViewModel>>
{
	public string? Query { get; private set; }
	public int PageNumber { get; private set; } = 1;
	public int PageSize { get; private set; } = 10;
		

	public GetAllPedidosQuery(string? query)
	{
		Query = query;
	}
}