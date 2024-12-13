using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllClientes;

public class GetAllClientesQuery : IRequest<List<ClienteViewModel>>
{
	public string? Query { get; private set; }

	public GetAllClientesQuery(string query)
	{
		Query = query;
	}
}