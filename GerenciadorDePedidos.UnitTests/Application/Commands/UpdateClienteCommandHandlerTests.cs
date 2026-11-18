using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class UpdateClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_ClienteExist_ShouldUpdateClienteAndLogin()
	{
		var cliente = new Cliente("Nome teste", "11111111111", Role.Usuario);
		var login = new Login(cliente.Id, "old@test.com", "hash", Role.Usuario);

		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(login);
		loginRepositoryMock.Setup(r => r.GetByEmailAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync((Login?)null);

		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var handler = new UpdateClienteCommandHandler(
			clienteRepositoryMock.Object,
			loginRepositoryMock.Object,
			unitOfWorkMock.Object);

		var command = new UpdateClienteCommand(cliente.Id, "new@test.com", "11999999999");

		await handler.Handle(command, CancellationToken.None);

		Assert.Equal("11999999999", cliente.Telefone);
		Assert.Equal("new@test.com", login.Email);
		clienteRepositoryMock.Verify(r => r.Update(cliente), Times.Once);
		loginRepositoryMock.Verify(r => r.Update(login), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ClienteNaoExiste_DeveLancarKeyNotFoundException()
	{
		var clienteId = Guid.NewGuid();
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

		var loginRepositoryMock = new Mock<ILoginRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var handler = new UpdateClienteCommandHandler(
			clienteRepositoryMock.Object,
			loginRepositoryMock.Object,
			unitOfWorkMock.Object);

		var command = new UpdateClienteCommand(clienteId, "novo@email.com", "99999999999");

		await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		clienteRepositoryMock.Verify(r => r.Update(It.IsAny<Cliente>()), Times.Never);
	}
}