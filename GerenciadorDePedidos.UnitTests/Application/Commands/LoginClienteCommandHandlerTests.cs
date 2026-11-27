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
	public async Task Handle_CredenciaisValidas_DeveRetornarViewModelComAccessERefreshTokenEPersistirHash()
	{
		const string email = "email@teste.com";
		const string senha = "Teste@123";
		const string senhaHash = "hashSenha123";
		const string tokenFake = "token_jwt_fake";
		const string refreshTokenFake = "refresh_token_fake";
		const string refreshTokenHash = "hashed_refresh_token_fake";
		const Role role = Role.Usuario;

		var clienteId = Guid.NewGuid();
		var login = new Login(clienteId, email, senhaHash, role) { Id = 10 };

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.VerifyPassword(senha, senhaHash)).Returns(true);
		authServiceMock.Setup(a => a.GenerateJwtToken(clienteId, email, role.ToString())).Returns(tokenFake);
		authServiceMock.Setup(a => a.GenerateRefreshToken()).Returns(refreshTokenFake);
		authServiceMock.Setup(a => a.HashToken(refreshTokenFake)).Returns(refreshTokenHash);
		authServiceMock.Setup(a => a.GetRefreshTokenExpirationUtc()).Returns(DateTime.UtcNow.AddDays(7));

		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var loginAttemptServiceMock = new Mock<ILoginAttemptService>();

		var handler = new LoginClienteCommandHandler(
			authServiceMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object,
			loginAttemptServiceMock.Object);
		var command = new LoginClienteCommand(email, senha);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(email, result.Email);
		Assert.Equal(tokenFake, result.Token);
		Assert.Equal(refreshTokenFake, result.RefreshToken);
		loginAttemptServiceMock.Verify(a => a.ResetAttempts(email), Times.Once);
		refreshTokenRepositoryMock.Verify(r => r.AddAsync(
			It.Is<RefreshToken>(rt => rt.LoginId == 10 && rt.Token == refreshTokenHash),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_EmailNaoEncontrado_DeveExecutarDummyVerifyRegistrarFalhaERetornarNull()
	{
		const string email = "email@teste.com";

		var authServiceMock = new Mock<IAuthService>();
		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((Login?)null);

		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var loginAttemptServiceMock = new Mock<ILoginAttemptService>();

		var handler = new LoginClienteCommandHandler(
			authServiceMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object,
			loginAttemptServiceMock.Object);
		var command = new LoginClienteCommand(email, "Teste@123");

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Null(result);
		authServiceMock.Verify(a => a.VerifyPassword("Teste@123", It.IsAny<string>()), Times.Once);
		loginAttemptServiceMock.Verify(a => a.RegisterFailedAttempt(email), Times.Once);
		refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_ContaBloqueadaPorLockout_DeveRetornarNullSemConsultarBanco()
	{
		const string email = "bloqueado@teste.com";

		var authServiceMock = new Mock<IAuthService>();
		var loginRepositoryMock = new Mock<ILoginRepository>();
		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var loginAttemptServiceMock = new Mock<ILoginAttemptService>();
		loginAttemptServiceMock.Setup(a => a.IsLockedOut(email)).Returns(true);

		var handler = new LoginClienteCommandHandler(
			authServiceMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object,
			loginAttemptServiceMock.Object);

		var result = await handler.Handle(new LoginClienteCommand(email, "Teste@123"), CancellationToken.None);

		Assert.Null(result);
		authServiceMock.Verify(a => a.VerifyPassword("Teste@123", It.IsAny<string>()), Times.Once);
		loginRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_SenhaInvalida_DeveRegistrarFalhaERetornarNull()
	{
		const string email = "email@teste.com";
		const string senhaHash = "hashSenha123";
		var login = new Login(Guid.NewGuid(), email, senhaHash, Role.Usuario);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.VerifyPassword("Errada", senhaHash)).Returns(false);

		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var loginAttemptServiceMock = new Mock<ILoginAttemptService>();

		var handler = new LoginClienteCommandHandler(
			authServiceMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object,
			loginAttemptServiceMock.Object);
		var command = new LoginClienteCommand(email, "Errada");

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Null(result);
		loginAttemptServiceMock.Verify(a => a.RegisterFailedAttempt(email), Times.Once);
		refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_ClienteDesativado_DeveRetornarNull()
	{
		const string email = "inativo@teste.com";
		const string senhaHash = "hashSenha123";
		var cliente = new Cliente("Inativo", "11999999999", Role.Usuario);
		cliente.Desativar();
		var login = new Login(cliente.Id, email, senhaHash, Role.Usuario);
		typeof(Login).GetProperty(nameof(Login.Cliente))!.SetValue(login, cliente);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.VerifyPassword("Teste@123", senhaHash)).Returns(true);

		var loginRepositoryMock = new Mock<ILoginRepository>();
		loginRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var loginAttemptServiceMock = new Mock<ILoginAttemptService>();

		var handler = new LoginClienteCommandHandler(
			authServiceMock.Object,
			loginRepositoryMock.Object,
			refreshTokenRepositoryMock.Object,
			unitOfWorkMock.Object,
			loginAttemptServiceMock.Object);

		var result = await handler.Handle(new LoginClienteCommand(email, "Teste@123"), CancellationToken.None);

		Assert.Null(result);
		refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}