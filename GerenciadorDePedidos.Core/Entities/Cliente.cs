using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class Cliente : BaseEntity<Guid>
{
	public string NomeCompleto { get; private set; }
	public string Telefone { get; private set; }
	public Role Role { get; private set; }
	
	public Guid PedidoId { get; private set; }
	public List<MovimentacaoEstoque> MovimentacaoEstoque { get; set; } = [];
	public List<Pedido> Pedidos { get; private set; } = [];

	public Cliente()
	{
		Id = Guid.NewGuid();
    }
	
	public Cliente(string nomeCompleto, string telefone, Role role) : this()
	{
		NomeCompleto = nomeCompleto;
		Telefone = telefone;
		Role = role;
	}

	public void AlterarTelefone(string telefone)
	{
		Telefone = telefone;
	}
}

