using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetPedido;

public class GetPedidoQuery : IRequest<PedidoViewModel>
{
	public Guid Id { get; private set; }

	public GetPedidoQuery(Guid id)
	{
		Id = id;
	}
}