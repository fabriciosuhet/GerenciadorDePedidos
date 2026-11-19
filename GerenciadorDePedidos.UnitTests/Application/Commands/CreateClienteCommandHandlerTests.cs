using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class CreateClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_ComandoValido_DeveCriarClienteERetornarIdEToken()
	{
		var command = new CreateClienteCommand("Nome teste", Role.Admin, "Teste@123", "email@teste.com", "123456789");

		var clienteRepositoryMock = new Mock<IClienteRepository>();
		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync((Login?)null);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashPassword("Teste@123")).Returns("hashedPassword");
		authServiceMock.Setup(a => a.GenerateJwtToken("email@teste.com", nameof(Role.Admin))).Returns("jwt-token");
		authServiceMock.Setup(a => a.GenerateRefreshToken()).Returns("refresh-token");
		authServiceMock.Setup(a => a.GetRefreshTokenExpirationUtc()).Returns(DateTime.UtcNow.AddDays(7));

		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var handler = new CreateClienteCommandHandler(
			authServiceMock.Object,
			clienteRepositoryMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.NotEqual(Guid.Empty, result.Id);
		Assert.Equal("jwt-token", result.Token);
		Assert.Equal("refresh-token", result.RefreshToken);
		clienteRepositoryMock.Verify(r => r.AddAsync(
			It.Is<Cliente>(c => c.NomeCompleto == "Nome teste" && c.Telefone == "123456789" && c.Role == Role.Admin),
			It.IsAny<CancellationToken>()), Times.Once);
		loginRepositoryMock.Verify(r => r.AddAsync(
			It.Is<Login>(l => l.Email == "email@teste.com" && l.SenhaHash == "hashedPassword"),
			It.IsAny<CancellationToken>()), Times.Once);
		refreshTokenRepositoryMock.Verify(r => r.AddAsync(
			It.Is<RefreshToken>(rt => rt.Token == "refresh-token"),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	[Fact]
	public async Task Handle_EmailDuplicado_DeveLancarArgumentException()
	{
		var command = new CreateClienteCommand("Nome Teste", Role.Usuario, "Teste@123", "email@teste.com", "123456789");

		var clienteRepositoryMock = new Mock<IClienteRepository>();
		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Login(Guid.NewGuid(), command.Email, "hash", Role.Usuario));

		var authServiceMock = new Mock<IAuthService>();
		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var handler = new CreateClienteCommandHandler(
			authServiceMock.Object,
			clienteRepositoryMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object);

		await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		clienteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task RegisterHandler_DeveSempreForcarRoleUsuario()
	{
		var esperado = new CreateClienteViewModel(Guid.NewGuid(), "user-token", "user-refresh-token");
		var senderMock = new Mock<ISender>();
		senderMock
			.Setup(s => s.Send(It.IsAny<CreateClienteCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(esperado);

		var handler = new RegisterClienteCommandHandler(senderMock.Object);
		var command = new RegisterClienteCommand("Novo Usuario", "User@1234", "novo@teste.com", "11999999999");

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(esperado, result);
		senderMock.Verify(s => s.Send(
			It.Is<CreateClienteCommand>(c =>
				c.NomeCompleto == "Novo Usuario" &&
				c.Role == Role.Usuario &&
				c.Email == "novo@teste.com"),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}