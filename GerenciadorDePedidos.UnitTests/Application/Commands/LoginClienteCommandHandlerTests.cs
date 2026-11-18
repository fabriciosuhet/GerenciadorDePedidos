using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class LoginClienteCommandHandlerTests
{
	[Fact]
	public async Task Handle_CredenciaisValidas_DeveRetornarViewModelComToken()
	{
		const string email = "email@teste.com";
		const string senha = "Teste@123";
		const string senhaHash = "hashSenha123";
		const string tokenFake = "token_jwt_fake";
		const Role role = Role.Usuario;

		var login = new Login(Guid.NewGuid(), email, senhaHash, role);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.VerifyPassword(senha, senhaHash)).Returns(true);
		authServiceMock.Setup(a => a.GenerateJwtToken(email, role.ToString())).Returns(tokenFake);

		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var handler = new LoginClienteCommandHandler(authServiceMock.Object, loginRepositoryMock.Object);
		var command = new LoginClienteCommand(email, senha);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(email, result.Email);
		Assert.Equal(tokenFake, result.Token);
	}

	[Fact]
	public async Task Handle_EmailNaoEncontrado_DeveRetornarNull()
	{
		const string email = "email@teste.com";

		var authServiceMock = new Mock<IAuthService>();
		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((Login?)null);

		var handler = new LoginClienteCommandHandler(authServiceMock.Object, loginRepositoryMock.Object);
		var command = new LoginClienteCommand(email, "Teste@123");

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Null(result);
	}

	[Fact]
	public async Task Handle_SenhaInvalida_DeveRetornarNull()
	{
		const string email = "email@teste.com";
		const string senhaHash = "hashSenha123";
		var login = new Login(Guid.NewGuid(), email, senhaHash, Role.Usuario);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.VerifyPassword("Errada", senhaHash)).Returns(false);

		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var handler = new LoginClienteCommandHandler(authServiceMock.Object, loginRepositoryMock.Object);
		var command = new LoginClienteCommand(email, "Errada");

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Null(result);
	}
}