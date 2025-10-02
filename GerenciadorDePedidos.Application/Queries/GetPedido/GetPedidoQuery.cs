using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetPedido;

public class GetPedidoQuery : IRequest<PedidoRespondeDTO>
{
	public int Id { get; private set; }

	public GetPedidoQuery(int id)
	{
		Id = id;
	}
}