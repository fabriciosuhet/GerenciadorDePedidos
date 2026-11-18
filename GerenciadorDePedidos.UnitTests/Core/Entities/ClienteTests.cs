using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class ClienteTests
{
	[Fact]
	public void Construtor_DeveCriarClienteComDadosCorretos()
	{
		const string nome = "Fabricio suhet";
		const string telefone = "99999999999";
		const Role role = Role.Usuario;

		var cliente = new Cliente(nome, telefone, role);

		Assert.NotEqual(Guid.Empty, cliente.Id);
		Assert.Equal(nome, cliente.NomeCompleto);
		Assert.Equal(telefone, cliente.Telefone);
		Assert.Equal(role, cliente.Role);
		Assert.Empty(cliente.Pedidos);
	}

	[Fact]
	public void AlterarTelefone_DeveAtualizarTelefoneCorretamente()
	{
		var cliente = new Cliente("Fabricio suhet", "11987654321", Role.Usuario);
		const string novoTelefone = "11912345678";

		cliente.AlterarTelefone(novoTelefone);

		Assert.Equal(novoTelefone, cliente.Telefone);
	}

	[Fact]
	public void AlterarTelefone_TelefoneVazio_DeveLancarExcecao()
	{
		var cliente = new Cliente("Fabricio suhet", "11987654321", Role.Usuario);

		Assert.Throws<ArgumentException>(() => cliente.AlterarTelefone(""));
	}
}