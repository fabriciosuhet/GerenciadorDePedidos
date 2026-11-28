using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;

namespace GerenciadorDePedidos.UnitTests.Core.Entities;

public class PasswordResetTokenTests
{
	[Fact]
	public void Construtor_DadosValidos_DeveCriarTokenAtivo()
	{
		var expiresAt = DateTime.UtcNow.AddMinutes(30);
		var token = new PasswordResetToken(10, "hash-sha256-valido", expiresAt);

		Assert.Equal(10, token.LoginId);
		Assert.Equal("hash-sha256-valido", token.TokenHash);
		Assert.Equal(expiresAt, token.ExpiresAt);
		Assert.True(token.IsActive);
		Assert.False(token.IsUsed);
		Assert.False(token.IsRevoked);
		Assert.False(token.IsExpired);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Construtor_LoginIdInvalido_DeveLancarArgumentException(int loginIdInvalido)
	{
		Assert.Throws<ArgumentException>(() =>
			new PasswordResetToken(loginIdInvalido, "hash-valido", DateTime.UtcNow.AddMinutes(30)));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Construtor_TokenHashVazio_DeveLancarArgumentException(string hashInvalido)
	{
		Assert.Throws<ArgumentException>(() =>
			new PasswordResetToken(1, hashInvalido, DateTime.UtcNow.AddMinutes(30)));
	}

	[Fact]
	public void MarcarComoUtilizado_TokenAtivo_DeveMarcarEDesativar()
	{
		var token = new PasswordResetToken(1, "hash-valido", DateTime.UtcNow.AddMinutes(30));

		token.MarcarComoUtilizado();

		Assert.True(token.IsUsed);
		Assert.NotNull(token.UsedAt);
		Assert.False(token.IsActive);
	}

	[Fact]
	public void MarcarComoUtilizado_TokenJaUtilizado_DeveLancarInvalidOperationException()
	{
		var token = new PasswordResetToken(1, "hash-valido", DateTime.UtcNow.AddMinutes(30));
		token.MarcarComoUtilizado();

		Assert.Throws<InvalidOperationException>(() => token.MarcarComoUtilizado());
	}

	[Fact]
	public void MarcarComoUtilizado_TokenRevogado_DeveLancarInvalidOperationException()
	{
		var token = new PasswordResetToken(1, "hash-valido", DateTime.UtcNow.AddMinutes(30));
		token.Revogar();

		Assert.True(token.IsRevoked);
		Assert.False(token.IsActive);
		Assert.Throws<InvalidOperationException>(() => token.MarcarComoUtilizado());
	}

	[Fact]
	public void MarcarComoUtilizado_TokenExpirado_DeveLancarInvalidOperationException()
	{
		var token = new PasswordResetToken(1, "hash-valido", DateTime.UtcNow.AddMinutes(-1));

		Assert.True(token.IsExpired);
		Assert.False(token.IsActive);
		Assert.Throws<InvalidOperationException>(() => token.MarcarComoUtilizado());
	}

	[Fact]
	public void Revogar_TokenJaUtilizado_NaoDeveAlterarRevokedAt()
	{
		var token = new PasswordResetToken(1, "hash-valido", DateTime.UtcNow.AddMinutes(30));
		token.MarcarComoUtilizado();

		token.Revogar();

		Assert.True(token.IsUsed);
		Assert.False(token.IsRevoked);
		Assert.Null(token.RevokedAt);
	}

	[Fact]
	public void Login_AlterarSenha_DeveAtualizarHashOuRejeitarVazio()
	{
		var login = new Login(Guid.NewGuid(), "user@teste.com", "hash-antigo", Role.Usuario);

		login.AlterarSenha("hash-novo");
		Assert.Equal("hash-novo", login.SenhaHash);

		Assert.Throws<ArgumentException>(() => login.AlterarSenha(""));
		Assert.Throws<ArgumentException>(() => login.AlterarSenha("   "));
	}
}

