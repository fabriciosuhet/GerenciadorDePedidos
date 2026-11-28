using System.Net;
using System.Net.Http.Json;
using GerenciadorDePedidos.Application.Commands.ChangePassword;
using GerenciadorDePedidos.Application.Commands.ForgotPassword;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.RefreshToken;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;
using GerenciadorDePedidos.Application.Commands.ResetPassword;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.E2ETests.Infrastructure;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.E2ETests;

public class PasswordResetE2ETests
{
	[Fact]
	public async Task FluxoCompleto_EsqueciSenha_ERedefinirSenha_DeveInvalidarTokensAntigos_RevogarSessoes_EPersistirApenasHash()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		// 1. Registra usuário e obtém sessão inicial (com refresh token)
		var regResp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("Usuario Reset", "Senha@1234", "reset.user@teste.com", "11988887777"));
		Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);
		var registered = await regResp.Content.ReadFromJsonAsync<CreateClienteViewModel>();
		Assert.NotNull(registered);

		// 2. Esqueci senha para e-mail inexistente deve retornar 200 com mensagem genérica e NÃO enviar e-mail
		var unknownResp = await anonClient.PostAsJsonAsync(
			"/api/login/esqueci-senha",
			new ForgotPasswordCommand("desconhecido@teste.com"));
		Assert.Equal(HttpStatusCode.OK, unknownResp.StatusCode);
		var unknownBody = await unknownResp.Content.ReadFromJsonAsync<ForgotPasswordViewModel>();
		Assert.Equal(ForgotPasswordCommandHandler.MensagemGenerica, unknownBody!.Message);
		Assert.Empty(factory.EmailService.SentEmails);

		// 3. Primeira solicitação para e-mail existente gera token e envia e-mail
		var forgot1Resp = await anonClient.PostAsJsonAsync(
			"/api/login/esqueci-senha",
			new ForgotPasswordCommand("reset.user@teste.com"));
		Assert.Equal(HttpStatusCode.OK, forgot1Resp.StatusCode);
		var forgot1Body = await forgot1Resp.Content.ReadFromJsonAsync<ForgotPasswordViewModel>();
		Assert.Equal(ForgotPasswordCommandHandler.MensagemGenerica, forgot1Body!.Message);
		Assert.Single(factory.EmailService.SentEmails);
		var primeiroToken = factory.EmailService.SentEmails.Last().ResetToken;

		// 4. Verifica no banco que apenas o hash SHA-256 (64 hex chars) foi persistido, nunca o token puro
		using (var scope = factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();
			var stored = await db.PasswordResetTokens.AsNoTracking().ToListAsync();
			Assert.Single(stored);
			Assert.NotEqual(primeiroToken, stored[0].TokenHash);
			Assert.Equal(64, stored[0].TokenHash.Length);
		}

		// 5. Segunda solicitação invalida o primeiro token e emite um novo
		var forgot2Resp = await anonClient.PostAsJsonAsync(
			"/api/login/esqueci-senha",
			new ForgotPasswordCommand("reset.user@teste.com"));
		Assert.Equal(HttpStatusCode.OK, forgot2Resp.StatusCode);
		Assert.Equal(2, factory.EmailService.SentEmails.Count);
		var segundoToken = factory.EmailService.SentEmails.Last().ResetToken;
		Assert.NotEqual(primeiroToken, segundoToken);

		// Tentativa de usar o primeiro token (revogado) deve retornar 400 BadRequest
		var resetWithOldTokenResp = await anonClient.PostAsJsonAsync(
			"/api/login/redefinir-senha",
			new ResetPasswordCommand(primeiroToken, "NovaSenha@123", "NovaSenha@123"));
		Assert.Equal(HttpStatusCode.BadRequest, resetWithOldTokenResp.StatusCode);

		// 6. Redefine a senha com o segundo token válido -> 204 NoContent
		var resetOkResp = await anonClient.PostAsJsonAsync(
			"/api/login/redefinir-senha",
			new ResetPasswordCommand(segundoToken, "NovaSenha@123", "NovaSenha@123"));
		Assert.Equal(HttpStatusCode.NoContent, resetOkResp.StatusCode);

		// 7. Reuso do mesmo token deve retornar 400 BadRequest (single-use)
		var reuseTokenResp = await anonClient.PostAsJsonAsync(
			"/api/login/redefinir-senha",
			new ResetPasswordCommand(segundoToken, "OutraSenha@123", "OutraSenha@123"));
		Assert.Equal(HttpStatusCode.BadRequest, reuseTokenResp.StatusCode);

		// 8. Refresh token obtido antes da redefinição deve estar revogado (401 Unauthorized)
		var oldRefreshResp = await anonClient.PostAsJsonAsync(
			"/api/login/refresh",
			new RefreshTokenCommand(registered.RefreshToken));
		Assert.Equal(HttpStatusCode.Unauthorized, oldRefreshResp.StatusCode);

		// 9. Senha antiga não autentica mais, nova senha autentica normalmente
		var loginOldResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("reset.user@teste.com", "Senha@1234"));
		Assert.Equal(HttpStatusCode.NotFound, loginOldResp.StatusCode);

		var loginNewResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("reset.user@teste.com", "NovaSenha@123"));
		Assert.Equal(HttpStatusCode.OK, loginNewResp.StatusCode);
	}

	[Fact]
	public async Task RedefinirSenha_AposLockout_DeveLimparBloqueioEPermitirLoginComNovaSenha()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		var regResp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("Usuario Lockout Reset", "Senha@1234", "lockout.reset@teste.com", "11966665555"));
		Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

		// 5 falhas consecutivas acionam o lockout
		for (var i = 0; i < 5; i++)
		{
			var failResp = await anonClient.PostAsJsonAsync(
				"/api/login",
				new LoginClienteCommand("lockout.reset@teste.com", "Errada@123"));
			Assert.Equal(HttpStatusCode.NotFound, failResp.StatusCode);
		}

		// Solicita e conclui redefinição de senha
		var forgotResp = await anonClient.PostAsJsonAsync(
			"/api/login/esqueci-senha",
			new ForgotPasswordCommand("lockout.reset@teste.com"));
		Assert.Equal(HttpStatusCode.OK, forgotResp.StatusCode);
		var token = factory.EmailService.SentEmails.Single().ResetToken;

		var resetResp = await anonClient.PostAsJsonAsync(
			"/api/login/redefinir-senha",
			new ResetPasswordCommand(token, "Desbloqueada@123", "Desbloqueada@123"));
		Assert.Equal(HttpStatusCode.NoContent, resetResp.StatusCode);

		// Login imediato com nova senha deve funcionar (lockout limpo)
		var loginResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("lockout.reset@teste.com", "Desbloqueada@123"));
		Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
	}

	[Fact]
	public async Task AlterarSenhaAutenticado_DeveValidarSenhaAtual_RevogarRefreshTokens_EPermitirLoginComNovaSenha()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		// Sem autenticação deve retornar 401
		var unauthResp = await anonClient.PostAsJsonAsync(
			"/api/login/alterar-senha",
			new ChangePasswordCommand("Senha@1234", "Nova@5678", "Nova@5678"));
		Assert.Equal(HttpStatusCode.Unauthorized, unauthResp.StatusCode);

		var regResp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("Usuario Alterar", "Senha@1234", "alterar@teste.com", "11955554444"));
		Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);
		var registered = await regResp.Content.ReadFromJsonAsync<CreateClienteViewModel>();
		Assert.NotNull(registered);

		using var userClient = factory.CreateAuthenticatedClient(registered.Token);

		// Senha atual incorreta -> 400
		var wrongCurrentResp = await userClient.PostAsJsonAsync(
			"/api/login/alterar-senha",
			new ChangePasswordCommand("Errada@1234", "Nova@5678", "Nova@5678"));
		Assert.Equal(HttpStatusCode.BadRequest, wrongCurrentResp.StatusCode);

		// Nova senha igual à atual -> 400
		var samePasswordResp = await userClient.PostAsJsonAsync(
			"/api/login/alterar-senha",
			new ChangePasswordCommand("Senha@1234", "Senha@1234", "Senha@1234"));
		Assert.Equal(HttpStatusCode.BadRequest, samePasswordResp.StatusCode);

		// Alteração válida -> 204
		var changeOkResp = await userClient.PostAsJsonAsync(
			"/api/login/alterar-senha",
			new ChangePasswordCommand("Senha@1234", "Nova@5678", "Nova@5678"));
		Assert.Equal(HttpStatusCode.NoContent, changeOkResp.StatusCode);

		// Refresh token anterior deve ter sido revogado
		var refreshResp = await anonClient.PostAsJsonAsync(
			"/api/login/refresh",
			new RefreshTokenCommand(registered.RefreshToken));
		Assert.Equal(HttpStatusCode.Unauthorized, refreshResp.StatusCode);

		// Login com a nova senha deve funcionar
		var loginResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("alterar@teste.com", "Nova@5678"));
		Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
	}
}

