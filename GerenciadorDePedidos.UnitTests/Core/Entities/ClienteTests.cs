using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class ClienteTests
{
	[Fact]
	public void TestIfEditEmailClienteWorks()
	{
		var cliente = new Cliente(
			"Nome teste",
			"email@teste.com",
			"99999999",
			"Senhateste@1",
			Role.Usuario
			);
		cliente.AlterarEmail("novoemail@teste.com");

		Assert.NotNull(cliente);
		
	}
	
}