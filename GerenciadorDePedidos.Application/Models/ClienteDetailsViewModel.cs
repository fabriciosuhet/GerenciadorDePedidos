using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class ClienteDetailsViewModel
{
	public string NomeCompleto { get; private set; }
	public string Telefone { get; private set; }
	
	public List<PedidoRespondeDTO> Pedidos  { get; private set; }

	public ClienteDetailsViewModel(string nomeCompleto, string telefone, List<PedidoRespondeDTO> pedidos)
	{
		NomeCompleto = nomeCompleto;
		Telefone = telefone;
		Pedidos = pedidos;
	}
}