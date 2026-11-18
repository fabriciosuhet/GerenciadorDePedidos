using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeleteClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_ClienteExist_ShouldCallDeleteAsync()
	{
		var cliente = new Cliente("Nome", "11999999999", Role.Usuario);

		var clienteRepositoryMock = new Mock<IRepository<Cliente, Guid>>();
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

		var handler = new DeleteClienteCommandHandler(clienteRepositoryMock.Object);
		var command = new DeleteClienteCommand(cliente.Id);

		await handler.Handle(command, CancellationToken.None);

		clienteRepositoryMock.Verify(r => r.DeleteAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
		clienteRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ClienteDoesNotExist_ThrowsKeyNotFoundException()
	{
		var clienteId = Guid.NewGuid();
		var clienteRepositoryMock = new Mock<IRepository<Cliente, Guid>>();
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

		var handler = new DeleteClienteCommandHandler(clienteRepositoryMock.Object);
		var command = new DeleteClienteCommand(clienteId);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O cliente nao foi encontrado", exception.Message);
		clienteRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}