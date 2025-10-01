using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public class CreatePedidoCommand : IRequest<int>
{
	public List<ItemPedidoDTO> ItensPedidos { get; set; }
	public Guid ClienteId { get; set; }
	public Status Status { get; set; }
	
}