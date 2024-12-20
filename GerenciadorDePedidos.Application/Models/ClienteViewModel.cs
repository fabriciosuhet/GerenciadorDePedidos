using GerenciadorDePedidos.Core.Entities;

namespace GerenciadorDePedidos.Application.Models;

public class ClienteViewModel
{
	public Guid Id { get; private set; }
	public string NomeCompleto { get; private set; }
	public string Email { get; private set; }
	public string Telefone { get; private set; }

	public ClienteViewModel(Guid id, string nomeCompleto, string email, string telefone)
	{
		Id = id;
		NomeCompleto = nomeCompleto;
		Email = email;
		Telefone = telefone;
	}
}