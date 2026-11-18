using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class Cliente : BaseEntity<Guid>
{
	public string NomeCompleto { get; private set; } = string.Empty;
	public string Telefone { get; private set; } = string.Empty;
	public Role Role { get; private set; }
	public Guid PedidoId { get; private set; }
	public List<MovimentacaoEstoque> MovimentacaoEstoque { get; private set; } = [];
	public List<Pedido> Pedidos { get; private set; } = [];

	protected Cliente()
	{
	}

	public Cliente(string nomeCompleto, string telefone, Role role)
	{
		Id = Guid.NewGuid();
		NomeCompleto = nomeCompleto;
		Telefone = telefone;
		Role = role;
	}

	public void AlterarTelefone(string telefone)
	{
		if (string.IsNullOrWhiteSpace(telefone))
			throw new ArgumentException("O telefone não pode ser vazio.");

		Telefone = telefone;
	}
}

