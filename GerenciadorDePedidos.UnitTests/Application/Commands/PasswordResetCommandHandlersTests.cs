using GerenciadorDePedidos.Application.Commands.ChangePassword;
using GerenciadorDePedidos.Application.Commands.ForgotPassword;
using GerenciadorDePedidos.Application.Commands.ResetPassword;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class PasswordResetCommandHandlersTests
{
	[Fact]
	public async Task ForgotPassword_EmailInexistente_DeveRetornarMensagemGenericaSemGerarTokenOuEnviarEmail()
	{
		var loginRepoMock = new Mock<ILoginRepository>();
		loginRepoMock.Setup(r => r.GetByEmailAsync("naoexiste@teste.com", It.IsAny<CancellationToken>()))
			.ReturnsAsync((Login?)null);

		var resetRepoMock = new Mock<IPasswordResetTokenRepository>();
		var authServiceMock = new Mock<IAuthService>();
		var emailServiceMock = new Mock<IEmailService>();
		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ForgotPasswordCommandHandler(
			loginRepoMock.Object,
			resetRepoMock.Object,
			authServiceMock.Object,
			emailServiceMock.Object,
			uowMock.Object);

		var result = await handler.Handle(new ForgotPasswordCommand("naoexiste@teste.com"), CancellationToken.None);

		Assert.Equal(ForgotPasswordCommandHandler.MensagemGenerica, result.Message);
		resetRepoMock.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()), Times.Never);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
		emailServiceMock.Verify(
			e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task ForgotPassword_ContaDesativada_DeveRetornarMensagemGenericaSemGerarToken()
	{
		var cliente = new Cliente("Inativo", "11999999999", Role.Usuario);
		cliente.Desativar();
		var login = new Login(cliente.Id, "inativo@teste.com", "hash", Role.Usuario) { Id = 12 };
		login.Desativar();
		typeof(Login).GetProperty(nameof(Login.Cliente))!.SetValue(login, cliente);

		var loginRepoMock = new Mock<ILoginRepository>();
		loginRepoMock.Setup(r => r.GetByEmailAsync("inativo@teste.com", It.IsAny<CancellationToken>()))
			.ReturnsAsync(login);

		var resetRepoMock = new Mock<IPasswordResetTokenRepository>();
		var authServiceMock = new Mock<IAuthService>();
		var emailServiceMock = new Mock<IEmailService>();
		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ForgotPasswordCommandHandler(
			loginRepoMock.Object,
			resetRepoMock.Object,
			authServiceMock.Object,
			emailServiceMock.Object,
			uowMock.Object);

		var result = await handler.Handle(new ForgotPasswordCommand("inativo@teste.com"), CancellationToken.None);

		Assert.Equal(ForgotPasswordCommandHandler.MensagemGenerica, result.Message);
		resetRepoMock.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()), Times.Never);
		emailServiceMock.Verify(
			e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task ForgotPassword_EmailValido_DeveInvalidarAnterioresPersistirHashEEnviarEmail()
	{
		var cliente = new Cliente("Maria Silva", "11988887777", Role.Usuario);
		var login = new Login(cliente.Id, "maria@teste.com", "hash-atual", Role.Usuario) { Id = 7 };
		typeof(Login).GetProperty(nameof(Login.Cliente))!.SetValue(login, cliente);

		var expiresAt = DateTime.UtcNow.AddMinutes(30);

		var loginRepoMock = new Mock<ILoginRepository>();
		loginRepoMock.Setup(r => r.GetByEmailAsync("maria@teste.com", It.IsAny<CancellationToken>()))
			.ReturnsAsync(login);

		var resetRepoMock = new Mock<IPasswordResetTokenRepository>();
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.GeneratePasswordResetToken()).Returns("raw-reset-token");
		authServiceMock.Setup(a => a.HashToken("raw-reset-token")).Returns("sha256-hash-token");
		authServiceMock.Setup(a => a.GetPasswordResetTokenExpirationUtc()).Returns(expiresAt);

		var emailServiceMock = new Mock<IEmailService>();
		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ForgotPasswordCommandHandler(
			loginRepoMock.Object,
			resetRepoMock.Object,
			authServiceMock.Object,
			emailServiceMock.Object,
			uowMock.Object);

		var result = await handler.Handle(new ForgotPasswordCommand("  maria@teste.com  "), CancellationToken.None);

		Assert.Equal(ForgotPasswordCommandHandler.MensagemGenerica, result.Message);
		resetRepoMock.Verify(r => r.InvalidateAllActiveByLoginIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
		resetRepoMock.Verify(r => r.AddAsync(
			It.Is<PasswordResetToken>(t => t.LoginId == 7 && t.TokenHash == "sha256-hash-token" && t.ExpiresAt == expiresAt),
			It.IsAny<CancellationToken>()), Times.Once);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		emailServiceMock.Verify(
			e => e.SendPasswordResetEmailAsync("maria@teste.com", "Maria Silva", "raw-reset-token", expiresAt, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task ForgotPassword_FalhaNoSmtp_NaoDeveExporErroEDeveRetornarMensagemGenerica()
	{
		var cliente = new Cliente("Carlos", "11988887777", Role.Usuario);
		var login = new Login(cliente.Id, "carlos@teste.com", "hash-atual", Role.Usuario) { Id = 9 };
		typeof(Login).GetProperty(nameof(Login.Cliente))!.SetValue(login, cliente);

		var loginRepoMock = new Mock<ILoginRepository>();
		loginRepoMock.Setup(r => r.GetByEmailAsync("carlos@teste.com", It.IsAny<CancellationToken>()))
			.ReturnsAsync(login);

		var resetRepoMock = new Mock<IPasswordResetTokenRepository>();
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.GeneratePasswordResetToken()).Returns("raw-token");
		authServiceMock.Setup(a => a.HashToken("raw-token")).Returns("hash-token");
		authServiceMock.Setup(a => a.GetPasswordResetTokenExpirationUtc()).Returns(DateTime.UtcNow.AddMinutes(30));

		var emailServiceMock = new Mock<IEmailService>();
		emailServiceMock
			.Setup(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("SMTP indisponível"));

		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ForgotPasswordCommandHandler(
			loginRepoMock.Object,
			resetRepoMock.Object,
			authServiceMock.Object,
			emailServiceMock.Object,
			uowMock.Object);

		var result = await handler.Handle(new ForgotPasswordCommand("carlos@teste.com"), CancellationToken.None);

		Assert.Equal(ForgotPasswordCommandHandler.MensagemGenerica, result.Message);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task ResetPassword_TokenValido_DeveAtualizarSenhaRevogarSessoesELimparLockout()
	{
		var cliente = new Cliente("Ana", "11977776666", Role.Usuario);
		var login = new Login(cliente.Id, "ana@teste.com", "hash-antigo", Role.Usuario) { Id = 4 };
		typeof(Login).GetProperty(nameof(Login.Cliente))!.SetValue(login, cliente);

		var resetToken = new PasswordResetToken(4, "hash-token-valido", DateTime.UtcNow.AddMinutes(20));
		typeof(PasswordResetToken).GetProperty(nameof(PasswordResetToken.Login))!.SetValue(resetToken, login);

		var resetRepoMock = new Mock<IPasswordResetTokenRepository>();
		resetRepoMock.Setup(r => r.GetByTokenHashWithLoginAsync("hash-token-valido", It.IsAny<CancellationToken>()))
			.ReturnsAsync(resetToken);

		var refreshRepoMock = new Mock<IRefreshTokenRepository>();
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("token-valido")).Returns("hash-token-valido");
		authServiceMock.Setup(a => a.HashPassword("Nova@1234")).Returns("hash-novo");

		var lockoutMock = new Mock<ILoginAttemptService>();
		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ResetPasswordCommandHandler(
			resetRepoMock.Object,
			refreshRepoMock.Object,
			authServiceMock.Object,
			lockoutMock.Object,
			uowMock.Object);

		await handler.Handle(new ResetPasswordCommand("token-valido", "Nova@1234", "Nova@1234"), CancellationToken.None);

		Assert.Equal("hash-novo", login.SenhaHash);
		Assert.True(resetToken.IsUsed);
		resetRepoMock.Verify(r => r.InvalidateAllActiveByLoginIdAsync(4, It.IsAny<CancellationToken>()), Times.Once);
		refreshRepoMock.Verify(r => r.RevokeAllActiveByLoginIdAsync(4, It.IsAny<CancellationToken>()), Times.Once);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		lockoutMock.Verify(l => l.ResetAttempts("ana@teste.com"), Times.Once);
	}

	[Fact]
	public async Task ResetPassword_TokenInexistenteOuExpiradoOuUsado_DeveLancarArgumentException()
	{
		var cliente = new Cliente("Ana", "11977776666", Role.Usuario);
		var login = new Login(cliente.Id, "ana@teste.com", "hash-antigo", Role.Usuario) { Id = 4 };
		typeof(Login).GetProperty(nameof(Login.Cliente))!.SetValue(login, cliente);

		var expiredToken = new PasswordResetToken(4, "hash-expired", DateTime.UtcNow.AddMinutes(-5));
		typeof(PasswordResetToken).GetProperty(nameof(PasswordResetToken.Login))!.SetValue(expiredToken, login);

		var usedToken = new PasswordResetToken(4, "hash-used", DateTime.UtcNow.AddMinutes(10));
		usedToken.MarcarComoUtilizado();
		typeof(PasswordResetToken).GetProperty(nameof(PasswordResetToken.Login))!.SetValue(usedToken, login);

		var resetRepoMock = new Mock<IPasswordResetTokenRepository>();
		resetRepoMock.Setup(r => r.GetByTokenHashWithLoginAsync("hash-missing", It.IsAny<CancellationToken>())).ReturnsAsync((PasswordResetToken?)null);
		resetRepoMock.Setup(r => r.GetByTokenHashWithLoginAsync("hash-expired", It.IsAny<CancellationToken>())).ReturnsAsync(expiredToken);
		resetRepoMock.Setup(r => r.GetByTokenHashWithLoginAsync("hash-used", It.IsAny<CancellationToken>())).ReturnsAsync(usedToken);

		var refreshRepoMock = new Mock<IRefreshTokenRepository>();
		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.HashToken("missing")).Returns("hash-missing");
		authServiceMock.Setup(a => a.HashToken("expired")).Returns("hash-expired");
		authServiceMock.Setup(a => a.HashToken("used")).Returns("hash-used");

		var lockoutMock = new Mock<ILoginAttemptService>();
		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ResetPasswordCommandHandler(
			resetRepoMock.Object,
			refreshRepoMock.Object,
			authServiceMock.Object,
			lockoutMock.Object,
			uowMock.Object);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			handler.Handle(new ResetPasswordCommand("missing", "Nova@1234", "Nova@1234"), CancellationToken.None));
		await Assert.ThrowsAsync<ArgumentException>(() =>
			handler.Handle(new ResetPasswordCommand("expired", "Nova@1234", "Nova@1234"), CancellationToken.None));
		await Assert.ThrowsAsync<ArgumentException>(() =>
			handler.Handle(new ResetPasswordCommand("used", "Nova@1234", "Nova@1234"), CancellationToken.None));

		Assert.Equal("hash-antigo", login.SenhaHash);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task ChangePassword_DadosValidos_DeveAlterarSenhaERevogarSessoes()
	{
		var clienteId = Guid.NewGuid();
		var login = new Login(clienteId, "user@teste.com", "hash-atual", Role.Usuario) { Id = 15 };

		var loginRepoMock = new Mock<ILoginRepository>();
		loginRepoMock.Setup(r => r.GetByClienteIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var refreshRepoMock = new Mock<IRefreshTokenRepository>();
		var resetRepoMock = new Mock<IPasswordResetTokenRepository>();

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.VerifyPassword("Atual@123", "hash-atual")).Returns(true);
		authServiceMock.Setup(a => a.VerifyPassword("Nova@456", "hash-atual")).Returns(false);
		authServiceMock.Setup(a => a.HashPassword("Nova@456")).Returns("hash-nova");

		var currentUserMock = new Mock<ICurrentUserService>();
		currentUserMock.Setup(u => u.ClienteId).Returns(clienteId);

		var lockoutMock = new Mock<ILoginAttemptService>();
		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ChangePasswordCommandHandler(
			loginRepoMock.Object,
			refreshRepoMock.Object,
			resetRepoMock.Object,
			authServiceMock.Object,
			currentUserMock.Object,
			lockoutMock.Object,
			uowMock.Object);

		await handler.Handle(new ChangePasswordCommand("Atual@123", "Nova@456", "Nova@456"), CancellationToken.None);

		Assert.Equal("hash-nova", login.SenhaHash);
		refreshRepoMock.Verify(r => r.RevokeAllActiveByLoginIdAsync(15, It.IsAny<CancellationToken>()), Times.Once);
		resetRepoMock.Verify(r => r.InvalidateAllActiveByLoginIdAsync(15, It.IsAny<CancellationToken>()), Times.Once);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		lockoutMock.Verify(l => l.ResetAttempts("user@teste.com"), Times.Once);
	}

	[Fact]
	public async Task ChangePassword_SenhaAtualErradaOuNovaIgualAtual_DeveLancarArgumentException()
	{
		var clienteId = Guid.NewGuid();
		var login = new Login(clienteId, "user@teste.com", "hash-atual", Role.Usuario) { Id = 15 };

		var loginRepoMock = new Mock<ILoginRepository>();
		loginRepoMock.Setup(r => r.GetByClienteIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(login);

		var authServiceMock = new Mock<IAuthService>();
		authServiceMock.Setup(a => a.VerifyPassword("Errada@123", "hash-atual")).Returns(false);
		authServiceMock.Setup(a => a.VerifyPassword("Atual@123", "hash-atual")).Returns(true);

		var currentUserMock = new Mock<ICurrentUserService>();
		currentUserMock.Setup(u => u.ClienteId).Returns(clienteId);

		var uowMock = new Mock<IUnitOfWork>();

		var handler = new ChangePasswordCommandHandler(
			loginRepoMock.Object,
			new Mock<IRefreshTokenRepository>().Object,
			new Mock<IPasswordResetTokenRepository>().Object,
			authServiceMock.Object,
			currentUserMock.Object,
			new Mock<ILoginAttemptService>().Object,
			uowMock.Object);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			handler.Handle(new ChangePasswordCommand("Errada@123", "Nova@456", "Nova@456"), CancellationToken.None));

		await Assert.ThrowsAsync<ArgumentException>(() =>
			handler.Handle(new ChangePasswordCommand("Atual@123", "Atual@123", "Atual@123"), CancellationToken.None));

		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
