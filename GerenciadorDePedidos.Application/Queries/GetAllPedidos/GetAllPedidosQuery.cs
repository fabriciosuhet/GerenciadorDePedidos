using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public class GetAllPedidosQuery : IRequest<List<PedidoViewModel>>
{
	public string Query { get; private set; }

	public GetAllPedidosQuery(string query)
	{
		Query = query;
	}
}