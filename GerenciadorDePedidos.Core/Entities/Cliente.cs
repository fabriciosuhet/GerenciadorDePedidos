using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class Cliente : BaseEntity
{
	public string NomeCompleto { get; private set; }
	public string Email { get; private set; }
	public string Telefone { get; private set; }
	public string Senha { get; private set; }
	public Role Role { get; private set; }
	
	public Guid PedidoId { get; private set; }
	
	public List<Pedido> Pedidos  { get; private set; }

	public Cliente()
	{
		
	}
	
	public Cliente(string nomeCompleto, string email, string telefone, string senha, Role role)
	{
		NomeCompleto = nomeCompleto;
		Email = email;
		Telefone = telefone;
		Senha = senha;
		Role = role;
		
		Pedidos = new List<Pedido>();
	}

	public void AlterarEmail(string email)
	{
		Email = email;
	}

	public void AlterarTelefone(string telefone)
	{
		Telefone = telefone;
	}
}