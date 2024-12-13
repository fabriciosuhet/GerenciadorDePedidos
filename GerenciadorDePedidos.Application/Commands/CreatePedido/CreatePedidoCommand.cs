using GerenciadorDePedidos.Core.Entities;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public class CreatePedidoCommand : IRequest<Guid>
{
	public DateTime DataPedido { get; private set; } = DateTime.Now;
	public List<ItemPedido> ItensPedidos { get; private set; }
	public decimal Total { get; private set; }
	
	
}