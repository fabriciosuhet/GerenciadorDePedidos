using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public class CreatePedidoCommand : IRequest<Guid>
{
	public List<ItemPedidoDTO> ItensPedidos { get; set; }
	public Guid ClienteId { get; set; }
	
}