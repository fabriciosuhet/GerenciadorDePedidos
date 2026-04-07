using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class UpdateClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_ClienteExist_ShouldUpdateCliente()
	{
		// Arrange 
		var clienteId = Guid.NewGuid();
		var nome = "Nome teste";
		var email = "test@test.com";
		var telefone = "123456";
		var role = Role.Usuario;
			
		var clienteMock = new Cliente(nome, telefone, role);
		var clienteRepositoryMock = new Mock<IRepository<Cliente, Guid>>();
		
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(clienteMock);
		
		var handler = new UpdateClienteCommandHandler(clienteRepositoryMock.Object);
		var command = new UpdateClienteCommand{Id = clienteId, Email = email, Telefone = telefone};
		
		// Act
		await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.Equal(telefone, clienteMock.Telefone);
		clienteRepositoryMock.Verify(
			r => r.UpdateAsync(It.Is<Cliente>(c => c.Telefone.Equals(telefone))),
			Times.Once);
	}

	[Fact]
	public async Task Handler_ClienteNaoExiste_DeveLancarUmaExcecao()
	{
		// Arrange
		var clienteId = Guid.NewGuid();
		var clienteRepositoryMock = new Mock<IRepository<Cliente, Guid>>();

        clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync((Cliente)null);
		
		var handler = new UpdateClienteCommandHandler(clienteRepositoryMock.Object);
		var command = new UpdateClienteCommand { Id = clienteId, Email = "novo@email.com", Telefone = "99999999999" };
		
		// Act e Assert
		await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));

		clienteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Cliente>()), Times.Never);
	}
}