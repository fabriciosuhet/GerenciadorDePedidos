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
	[Fact]
	public async Task RefreshHandle_TokenAtivo_DeveRotacionarERetornarNovosTokens()
	{
		var clienteId = Guid.NewGuid();
		var login = new Login(clienteId, "user@teste.com", "hash", Role.Usuario) { Id = 5 };
		var existingToken = new RefreshToken(5, "old-refresh", DateTime.UtcNow.AddDays(3));
		typeof(RefreshToken).GetProperty(nameof(RefreshToken.Login))!.SetValue(existingToken, login);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.GenerateJwtToken(clienteId, "user@teste.com", nameof(Role.Usuario))).Returns("new-access");
		authServiceMock.Setup(a => a.GenerateRefreshToken()).Returns("new-refresh");
		authServiceMock.Setup(a => a.GetRefreshTokenExpirationUtc()).Returns(DateTime.UtcNow.AddDays(7));

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("old-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RefreshTokenCommandHandler(authServiceMock.Object, repoMock.Object, unitOfWorkMock.Object);

		var result = await handler.Handle(new RefreshTokenCommand("old-refresh"), CancellationToken.None);

		Assert.Equal("user@teste.com", result.Email);
		Assert.Equal("new-access", result.Token);
		Assert.Equal("new-refresh", result.RefreshToken);
		Assert.True(existingToken.IsRevoked);
		Assert.Equal("new-refresh", existingToken.ReplacedByToken);
		repoMock.Verify(r => r.AddAsync(
			It.Is<RefreshToken>(rt => rt.Token == "new-refresh" && rt.LoginId == 5),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RefreshHandle_TokenRevogadoReutilizado_DeveRevogarTodosOsTokensELancarExcecao()
	{
		var existingToken = new RefreshToken(5, "reused-refresh", DateTime.UtcNow.AddDays(3));
		existingToken.Revogar("other-token");

		var authServiceMock = new Mock<IAuthService>();
		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("reused-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

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
		var existingToken = new RefreshToken(5, "expired-refresh", DateTime.UtcNow.AddMinutes(-1));

		var authServiceMock = new Mock<IAuthService>();
		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("expired-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RefreshTokenCommandHandler(authServiceMock.Object, repoMock.Object, unitOfWorkMock.Object);

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new RefreshTokenCommand("expired-refresh"), CancellationToken.None));
	}

	[Fact]
	public async Task RevokeHandle_TokenExistente_DeveRevogarESalvar()
	{
		var existingToken = new RefreshToken(5, "active-refresh", DateTime.UtcNow.AddDays(3));
		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("active-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RevokeTokenCommandHandler(repoMock.Object, unitOfWorkMock.Object);

		await handler.Handle(new RevokeTokenCommand("active-refresh"), CancellationToken.None);

		Assert.True(existingToken.IsRevoked);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task RevokeHandle_TokenInexistente_DeveLancarKeyNotFoundException()
	{
		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RevokeTokenCommandHandler(repoMock.Object, unitOfWorkMock.Object);

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

		var existingToken = new RefreshToken(8, "token-inativo", DateTime.UtcNow.AddDays(3));
		typeof(RefreshToken).GetProperty(nameof(RefreshToken.Login))!.SetValue(existingToken, login);

		var repoMock = new Mock<IRefreshTokenRepository>();
		repoMock.Setup(r => r.GetByTokenWithLoginAsync("token-inativo", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var handler = new RefreshTokenCommandHandler(new Mock<IAuthService>().Object, repoMock.Object, unitOfWorkMock.Object);

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new RefreshTokenCommand("token-inativo"), CancellationToken.None));

		repoMock.Verify(r => r.RevokeAllActiveByLoginIdAsync(8, It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}