using GerenciadorDePedidos.Core.Entities;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public class CreatePedidoCommand : IRequest<Guid>
{
	public DateTime DataPedido { get; set; } = DateTime.Now;
	public List<ItemPedido> ItensPedidos { get; set; }
	public decimal Total { get; set; }
	
	
}