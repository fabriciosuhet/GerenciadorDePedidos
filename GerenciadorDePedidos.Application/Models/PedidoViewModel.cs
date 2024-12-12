using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class PedidoViewModel
{
	public DateTime DataPedido { get; private set; } = DateTime.Now;
	public List<ItemPedido> ItensPedidos { get; private set; }
	public decimal Total { get; private set; }

	public PedidoViewModel(List<ItemPedido> itensPedidos, decimal total)
	{
		ItensPedidos = itensPedidos;
		Total = total;
	}
}