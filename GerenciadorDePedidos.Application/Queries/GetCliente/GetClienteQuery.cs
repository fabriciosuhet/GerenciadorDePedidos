using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetCliente;

public class GetClienteQuery : IRequest<ClienteDetailsViewModel>
{
	public Guid Id { get; private set; }

	public GetClienteQuery(Guid id)
	{
		Id = id;
	}
}