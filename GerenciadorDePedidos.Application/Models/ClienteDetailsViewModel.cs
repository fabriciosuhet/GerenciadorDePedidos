using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class ClienteDetailsViewModel
{
	public string NomeCompleto { get; private set; }
	public string Email { get; private set; }
	public string Telefone { get; private set; }
	
	public List<PedidoRespondeDTO> Pedidos  { get; private set; }

	public ClienteDetailsViewModel(string nomeCompleto, string email, string telefone, List<PedidoRespondeDTO> pedidos)
	{
		NomeCompleto = nomeCompleto;
		Email = email;
		Telefone = telefone;
		Pedidos = pedidos;
	}
}