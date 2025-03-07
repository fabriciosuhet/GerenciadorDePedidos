using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeleteClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_ClienteExist_ShouldCallDeleteAsync()
	{
		// Arrange
		var clienteId = Guid.NewGuid();
		var clienteMock = new Cliente { Id = clienteId };
		
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(clienteMock);
		
		var handler = new DeleteClienteCommandHandler(clienteRepositoryMock.Object);
		var command = new DeleteClienteCommand(clienteId);
		
		// Act
		await handler.Handle(command, CancellationToken.None);
		
		
		// Assert
		clienteRepositoryMock.Verify(r => r.DeleteAsync(clienteId), Times.Once);
	}

	[Fact]
	public async Task Handle_ClienteDoesNotExist_ThrowsShouldExcpetion()
	{
		// Arrange
		var clienteId = Guid.NewGuid();
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync((Cliente)null);
		
		var handler = new DeleteClienteCommandHandler(clienteRepositoryMock.Object);
		var command = new DeleteClienteCommand(clienteId);
		
		// Act e Assert
		var excpetion = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O cliente nao foi encontrado", excpetion.Message);
		clienteRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
	}
}