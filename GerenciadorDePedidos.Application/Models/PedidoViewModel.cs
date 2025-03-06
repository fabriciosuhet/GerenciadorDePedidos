using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class PedidoViewModel
{
	public Guid Id { get; private set; }
	public DateTime DataPedido { get; private set; }
	public decimal Total { get; private set; }

	public PedidoViewModel(Guid id, decimal total)
	{
		Id = id;
		DataPedido = DateTime.Now;
		Total = total;
	}
}