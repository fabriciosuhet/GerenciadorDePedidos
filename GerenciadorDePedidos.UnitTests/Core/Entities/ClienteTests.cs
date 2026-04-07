using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class ClienteTests
{
	[Fact]
	public void Construtor_DeveCriarClienteComDadosCorretos()
	{
		// Arrange (Preparar)
		const string nome = "Fabricio suhet";
		const string email = "fabricio@gmail.com";
		const string telefone = "99999999999";
		const string senha = "Teste@123";
		const Role role = Role.Usuario;
		
		// Act (Agir)
		var cliente = new Cliente(nome, telefone, role);

        Assert.Multiple(() =>
        {
            // Assert (Verificar)
            Assert.That(cliente.NomeCompleto, Is.EqualTo(nome));
            Assert.That(cliente.Telefone, Is.EqualTo(telefone));
            Assert.That(cliente.Role, Is.EqualTo(role));
            Assert.That(cliente.Pedidos, Is.Empty);
        });
    }

	[Fact]
	public void AlterarTelefone_DeveAtualizarTelefoneCorretamente()
	{
		// Arrange 
		var cliente = new Cliente("Fabricio suhet", "11987654321", Role.Usuario);
		const string novoTelefone = "11912345678";
		
		// Act
		cliente.AlterarTelefone(novoTelefone);
		
		// Assert
		Assert.That(cliente.Telefone, Is.EqualTo(novoTelefone));
		
	}
	
}