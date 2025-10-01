using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
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