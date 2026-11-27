using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeleteClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_ClienteExist_ShouldSoftDeleteClienteDesativarLoginERevogarTokens()
	{
		var cliente = new Cliente("Nome", "11999999999", Role.Usuario);
		var login = new Login(cliente.Id, "nome@teste.com", "hash", Role.Usuario) { Id = 7 };

		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var handler = new DeleteClienteCommandHandler(
			clienteRepositoryMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object);

		await handler.Handle(new DeleteClienteCommand(cliente.Id), CancellationToken.None);

		Assert.False(cliente.Ativo);
		Assert.NotNull(cliente.DesativadoEm);
		Assert.False(login.Ativo);
		Assert.NotNull(login.DesativadoEm);
		clienteRepositoryMock.Verify(r => r.Update(cliente), Times.Once);
		loginRepositoryMock.Verify(r => r.Update(login), Times.Once);
		refreshTokenRepositoryMock.Verify(r => r.RevokeAllActiveByLoginIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ClienteDoesNotExist_ThrowsKeyNotFoundException()
	{
		var clienteId = Guid.NewGuid();
		var clienteRepositoryMock = new Mock<IClienteRepository>();
		clienteRepositoryMock.Setup(r => r.GetByIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

		var handler = new DeleteClienteCommandHandler(
			clienteRepositoryMock.Object,
			new Mock<ILoginRepository>().Object,
			new Mock<IRefreshTokenRepository>().Object,
			new Mock<IUnitOfWork>().Object);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
			handler.Handle(new DeleteClienteCommand(clienteId), CancellationToken.None));

		Assert.Equal("O cliente nao foi encontrado", exception.Message);
		clienteRepositoryMock.Verify(r => r.Update(It.IsAny<Cliente>()), Times.Never);
	}
}