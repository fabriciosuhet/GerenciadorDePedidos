using GerenciadorDePedidos.Application.Commands.RefreshToken;
using GerenciadorDePedidos.Application.Commands.RevokeToken;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class RefreshAndRevokeTokenCommandHandlerTests
{
	private static ICurrentUserService CreateUser(Guid clienteId, bool isAdmin = false)
	{
		var mock = new Mock<ICurrentUserService>();
		mock.Setup(u => u.ClienteId).Returns(clienteId);
		mock.Setup(u => u.IsAdmin).Returns(isAdmin);
		return mock.Object;
	}

	[Fact]
	public async Task RefreshHandle_TokenAtivo_DeveRotacionarPersistindoHashERetornarNovosTokens()
	{
		var clienteId = Guid.NewGuid();
		var login = new Login(clienteId, "user@teste.com", "hash", Role.Usuario) { Id = 5 };
		var existingToken = new RefreshToken(5, "hash-old-refresh", DateTime.UtcNow.AddDays(3));
		typeof(RefreshToken).GetProperty(nameof(RefreshToken.Login))!.SetValue(existingToken, login);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("old-refresh")).Returns("hash-old-refresh");
		authServiceMock.Setup(a => a.HashToken("new-refresh")).Returns("hash-new-refresh");
		authServiceMock.Setup(a => a.GenerateJwtToken(clienteId, "user@teste.com", nameof(Role.Usuario))).Returns("new-access");
		authServiceMock.Setup(a => a.GenerateRefreshToken()).Returns("new-refresh");
		authServiceMock.Setup(a => a.GetRefreshTokenExpirationUtc()).Returns(DateTime.UtcNow.AddDays(7));

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("hash-old-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RefreshTokenCommandHandler(authServiceMock.Object, repoMock.Object, unitOfWorkMock.Object);

		var result = await handler.Handle(new RefreshTokenCommand("old-refresh"), CancellationToken.None);

		Assert.Equal("user@teste.com", result.Email);
		Assert.Equal("new-access", result.Token);
		Assert.Equal("new-refresh", result.RefreshToken);
		Assert.True(existingToken.IsRevoked);
		Assert.Equal("hash-new-refresh", existingToken.ReplacedByToken);
		repoMock.Verify(r => r.AddAsync(
			It.Is<RefreshToken>(rt => rt.Token == "hash-new-refresh" && rt.LoginId == 5),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RefreshHandle_TokenRevogadoReutilizado_DeveRevogarTodosOsTokensELancarExcecao()
	{
		var existingToken = new RefreshToken(5, "hash-reused-refresh", DateTime.UtcNow.AddDays(3));
		existingToken.Revogar("hash-other-token");

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("reused-refresh")).Returns("hash-reused-refresh");

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("hash-reused-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RefreshTokenCommandHandler(authServiceMock.Object, repoMock.Object, unitOfWorkMock.Object);

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new RefreshTokenCommand("reused-refresh"), CancellationToken.None));

		repoMock.Verify(r => r.RevokeAllActiveByLoginIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RefreshHandle_TokenExpirado_DeveLancarUnauthorizedAccessException()
	{
		var existingToken = new RefreshToken(5, "hash-expired-refresh", DateTime.UtcNow.AddMinutes(-1));

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("expired-refresh")).Returns("hash-expired-refresh");

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("hash-expired-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RefreshTokenCommandHandler(authServiceMock.Object, repoMock.Object, unitOfWorkMock.Object);

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new RefreshTokenCommand("expired-refresh"), CancellationToken.None));
	}

	[Fact]
	public async Task RevokeHandle_TokenDoProprioUsuario_DeveRevogarESalvar()
	{
		var clienteId = Guid.NewGuid();
		var login = new Login(clienteId, "user@teste.com", "hash", Role.Usuario) { Id = 5 };
		var existingToken = new RefreshToken(5, "hash-active-refresh", DateTime.UtcNow.AddDays(3));
		typeof(RefreshToken).GetProperty(nameof(RefreshToken.Login))!.SetValue(existingToken, login);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("active-refresh")).Returns("hash-active-refresh");

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("hash-active-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RevokeTokenCommandHandler(
			authServiceMock.Object,
			repoMock.Object,
			unitOfWorkMock.Object,
			CreateUser(clienteId));

		await handler.Handle(new RevokeTokenCommand("active-refresh"), CancellationToken.None);

		Assert.True(existingToken.IsRevoked);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RevokeHandle_TokenDeOutroUsuario_DeveLancarUnauthorizedAccessException()
	{
		var donoId = Guid.NewGuid();
		var invasorId = Guid.NewGuid();
		var login = new Login(donoId, "dono@teste.com", "hash", Role.Usuario) { Id = 5 };
		var existingToken = new RefreshToken(5, "hash-token-dono", DateTime.UtcNow.AddDays(3));
		typeof(RefreshToken).GetProperty(nameof(RefreshToken.Login))!.SetValue(existingToken, login);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("token-dono")).Returns("hash-token-dono");

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("hash-token-dono", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RevokeTokenCommandHandler(
			authServiceMock.Object,
			repoMock.Object,
			unitOfWorkMock.Object,
			CreateUser(invasorId, isAdmin: false));

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new RevokeTokenCommand("token-dono"), CancellationToken.None));

		Assert.False(existingToken.IsRevoked);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task RevokeHandle_TokenInexistente_DeveLancarKeyNotFoundException()
	{
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("missing")).Returns("hash-missing");

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("hash-missing", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RevokeTokenCommandHandler(
			authServiceMock.Object,
			repoMock.Object,
			unitOfWorkMock.Object,
			CreateUser(Guid.NewGuid()));

		await Assert.ThrowsAsync<KeyNotFoundException>(() =>
			handler.Handle(new RevokeTokenCommand("missing"), CancellationToken.None));
	}

	[Fact]
	public async Task RefreshHandle_LoginDesativado_DeveRevogarTokensELancarUnauthorizedAccessException()
	{
		var cliente = new Cliente("Desativado", "11999999999", Role.Usuario);
		cliente.Desativar();
		var login = new Login(cliente.Id, "desativado@teste.com", "hash", Role.Usuario) { Id = 8 };
		login.Desativar();
		typeof(Login).GetProperty(nameof(Login.Cliente))!.SetValue(login, cliente);

		var existingToken = new RefreshToken(8, "hash-token-inativo", DateTime.UtcNow.AddDays(3));
		typeof(RefreshToken).GetProperty(nameof(RefreshToken.Login))!.SetValue(existingToken, login);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("token-inativo")).Returns("hash-token-inativo");

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("hash-token-inativo", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RefreshTokenCommandHandler(authServiceMock.Object, repoMock.Object, unitOfWorkMock.Object);

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new RefreshTokenCommand("token-inativo"), CancellationToken.None));

		repoMock.Verify(r => r.RevokeAllActiveByLoginIdAsync(8, It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}