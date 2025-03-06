using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllClientes;

public class GetAllClientesQuery : IRequest<PagedResultModel<ClienteViewModel>>
{
	public string? Query { get; private set; }
	public int PageNumber { get; private set; } = 1;
	public int PageSize { get; private set; } = 10;
	
	public GetAllClientesQuery(string? query)
	{
		Query = query;
	}
}