using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class PedidoViewModel
{
	public int Id { get; private set; }
	public DateTime DataPedido { get; private set; }
	public decimal Total { get; private set; }

	public PedidoViewModel(int id, decimal total)
	{
		Id = id;
		DataPedido = DateTime.Now;
		Total = total;
	}
}