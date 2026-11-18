using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.Core.Entities;

public class Login : BaseEntity<int>
{
	public Guid ClienteId { get; private set; }
	public Cliente Cliente { get; private set; } = null!;
	public string Email { get; private set; } = string.Empty;
	public string SenhaHash { get; private set; } = string.Empty;
	public Role Role { get; private set; }

	protected Login()
	{
	}

	public Login(Guid clienteId, string email, string senhaHash, Role role)
	{
		ClienteId = clienteId;
		Email = email;
		SenhaHash = senhaHash;
		Role = role;
	}

	public void AlterarEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("O email não pode ser vazio.");

		Email = email;
	}
}
