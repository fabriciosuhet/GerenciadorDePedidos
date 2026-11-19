using System.Net;
using System.Net.Http.Json;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.RefreshToken;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;
using GerenciadorDePedidos.Application.Commands.RevokeToken;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.E2ETests.Infrastructure;

namespace GerenciadorDePedidos.E2ETests;

public class AuthAndClienteE2ETests
{
	[Fact]
	public async Task FluxoCompleto_SeedAdmin_AutoRegistro_Login_Permissoes_Atualizacao_ERemocao()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		// 1. Endpoints protegidos devem exigir autenticação
		var unauthorizedResp = await anonClient.GetAsync("/api/cliente");
		Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResp.StatusCode);

		// 2. Auto-registro público com payload inválido deve retornar 400 BadRequest
		var invalidResp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("", "123", "email-invalido", ""));
		Assert.Equal(HttpStatusCode.BadRequest, invalidResp.StatusCode);

		// 3. Admin criado automaticamente via AdminSeeder deve conseguir autenticar
		var (adminId, adminToken) = await factory.BootstrapAdminAsync();
		Assert.NotEqual(Guid.Empty, adminId);
		Assert.False(string.IsNullOrWhiteSpace(adminToken));

		// 4. Login inválido deve retornar 404 NotFound
		var invalidLoginResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("admin@teste.com", "SenhaErrada@1"));
		Assert.Equal(HttpStatusCode.NotFound, invalidLoginResp.StatusCode);

		// 5. Auto-registro público cria sempre Role.Usuario e retorna Id + Token JWT pronto para uso
		var registerResp = await anonClient.PostAsJsonAsync("/api/cliente/registrar", new
		{
			NomeCompleto = "Usuario Auto Registro",
			Senha = "User@1234",
			Email = "autoregistro@teste.com",
			Telefone = "11977776666",
			Role = Role.Admin // Mesmo se enviado no JSON, é ignorado e registrado como Usuario
		});
		Assert.Equal(HttpStatusCode.Created, registerResp.StatusCode);
		var registered = await registerResp.Content.ReadFromJsonAsync<CreateClienteViewModel>();
		Assert.NotNull(registered);
		Assert.NotEqual(Guid.Empty, registered.Id);
		Assert.False(string.IsNullOrWhiteSpace(registered.Token));

		// 6. Token retornado pelo auto-registro já autentica como Usuario, mas não dá acesso de Admin
		using var userClient = factory.CreateAuthenticatedClient(registered.Token);

		var listAsUser = await userClient.GetAsync("/api/cliente");
		Assert.Equal(HttpStatusCode.OK, listAsUser.StatusCode);

		var createAsUser = await userClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Outro", Role.Admin, "User@1234", "outro@teste.com", "11966665555"));
		Assert.Equal(HttpStatusCode.Forbidden, createAsUser.StatusCode);

		var deleteAsUser = await userClient.DeleteAsync($"/api/cliente/deletar-cliente?id={registered.Id}");
		Assert.Equal(HttpStatusCode.Forbidden, deleteAsUser.StatusCode);

		// 7. Admin consegue criar outro usuário ou Admin e gerenciar clientes
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);
		var createByAdminResp = await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Segundo Admin", Role.Admin, "Admin@456", "admin2@teste.com", "11955554444"));
		Assert.Equal(HttpStatusCode.Created, createByAdminResp.StatusCode);

		// 8. Consulta por ID popula cache; atualização deve invalidar cache e refletir novos dados
		var getBeforeUpdate = await adminClient.GetFromJsonAsync<ClienteDetailsViewModel>($"/api/cliente/{registered.Id}");
		Assert.Equal("11977776666", getBeforeUpdate!.Telefone);

		var updateResp = await adminClient.PutAsJsonAsync(
			"/api/cliente/atualizar-cliente",
			new UpdateClienteCommand(registered.Id, "user.novo@teste.com", "11911112222"));
		Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

		var getAfterUpdate = await adminClient.GetFromJsonAsync<ClienteDetailsViewModel>($"/api/cliente/{registered.Id}");
		Assert.Equal("11911112222", getAfterUpdate!.Telefone);

		// Login com novo email deve funcionar após atualização
		var newToken = await factory.LoginAsync(anonClient, "user.novo@teste.com", "User@1234");
		Assert.False(string.IsNullOrWhiteSpace(newToken));

		// 9. Admin deleta o cliente e consulta posterior retorna 404
		var deleteResp = await adminClient.DeleteAsync($"/api/cliente/deletar-cliente?id={registered.Id}");
		Assert.Equal(HttpStatusCode.OK, deleteResp.StatusCode);

		var getDeleted = await adminClient.GetAsync($"/api/cliente/{registered.Id}");
		Assert.Equal(HttpStatusCode.NotFound, getDeleted.StatusCode);
	}

	[Fact]
	public async Task RateLimiting_ExcessoDeTentativasDeLogin_DeveRetornar429TooManyRequests()
	{
		using var factory = new CustomWebApplicationFactory();
		using var client = factory.CreateClient();

		HttpStatusCode? ultimoStatus = null;

		for (var i = 0; i < 12; i++)
		{
			var resp = await client.PostAsJsonAsync(
				"/api/login",
				new LoginClienteCommand("admin@teste.com", "SenhaErrada@1"));
			ultimoStatus = resp.StatusCode;

			if (ultimoStatus == HttpStatusCode.TooManyRequests)
				break;
		}

		Assert.Equal(HttpStatusCode.TooManyRequests, ultimoStatus);
	}

	[Fact]
	public async Task RefreshToken_RotacaoDeteccaoDeReusoELogout_DeveFuncionar()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		// 1. Login retorna Access Token e Refresh Token
		var loginResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("admin@teste.com", "Admin@123"));
		Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

		var login1 = await loginResp.Content.ReadFromJsonAsync<LoginClienteViewModel>();
		Assert.NotNull(login1);
		Assert.False(string.IsNullOrWhiteSpace(login1.Token));
		Assert.False(string.IsNullOrWhiteSpace(login1.RefreshToken));

		// 2. Refresh rotaciona o par de tokens
		var refreshResp = await anonClient.PostAsJsonAsync(
			"/api/login/refresh",
			new RefreshTokenCommand(login1.RefreshToken));
		Assert.Equal(HttpStatusCode.OK, refreshResp.StatusCode);

		var login2 = await refreshResp.Content.ReadFromJsonAsync<LoginClienteViewModel>();
		Assert.NotNull(login2);
		Assert.NotEqual(login1.RefreshToken, login2.RefreshToken);

		// Novo access token autentica normalmente
		using var authClient = factory.CreateAuthenticatedClient(login2.Token);
		var clientesResp = await authClient.GetAsync("/api/cliente");
		Assert.Equal(HttpStatusCode.OK, clientesResp.StatusCode);

		// 3. Reuso do refresh token antigo deve retornar 401 e revogar o novo token também
		var reuseResp = await anonClient.PostAsJsonAsync(
			"/api/login/refresh",
			new RefreshTokenCommand(login1.RefreshToken));
		Assert.Equal(HttpStatusCode.Unauthorized, reuseResp.StatusCode);

		var invalidatedResp = await anonClient.PostAsJsonAsync(
			"/api/login/refresh",
			new RefreshTokenCommand(login2.RefreshToken));
		Assert.Equal(HttpStatusCode.Unauthorized, invalidatedResp.StatusCode);

		// 4. Logout autenticado revoga o refresh token atual
		var login3 = await (await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("admin@teste.com", "Admin@123")))
			.Content.ReadFromJsonAsync<LoginClienteViewModel>();

		using var authClient3 = factory.CreateAuthenticatedClient(login3!.Token);
		var logoutResp = await authClient3.PostAsJsonAsync(
			"/api/login/logout",
			new RevokeTokenCommand(login3.RefreshToken));
		Assert.Equal(HttpStatusCode.NoContent, logoutResp.StatusCode);

		var refreshAfterLogout = await anonClient.PostAsJsonAsync(
			"/api/login/refresh",
			new RefreshTokenCommand(login3.RefreshToken));
		Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
	}
}
