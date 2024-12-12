using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class ClienteViewModel
{
	public string NomeCompleto { get; private set; }
	public string Email { get; private set; }
	public string Telefone { get; private set; }
	public List<Pedido> Pedidos  { get; private set; }

	public ClienteViewModel(string nomeCompleto, string email, string telefone)
	{
		NomeCompleto = nomeCompleto;
		Email = email;
		Telefone = telefone;
		Pedidos = new List<Pedido>();
	}
}