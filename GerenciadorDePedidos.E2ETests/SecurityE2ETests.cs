using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;
using GerenciadorDePedidos.Application.Commands.RevokeToken;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.E2ETests.Infrastructure;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.E2ETests;

public class SecurityE2ETests
{
	[Fact]
	public async Task RefreshToken_DeveSerPersistidoComHashSha256ENaoEmTextoPuro()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		var loginResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("admin@teste.com", "Admin@123"));
		Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

		var login = await loginResp.Content.ReadFromJsonAsync<LoginClienteViewModel>();
		Assert.NotNull(login);
		Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));

		using var scope = factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();
		var storedTokens = await db.RefreshTokens.AsNoTracking().ToListAsync();

		Assert.NotEmpty(storedTokens);
		Assert.DoesNotContain(storedTokens, t => t.Token == login.RefreshToken);
		Assert.All(storedTokens, t => Assert.Equal(64, t.Token.Length));
	}

	[Fact]
	public async Task Logout_UsuarioNaoPodeRevogarRefreshTokenDeOutroUsuario()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		var reg1Resp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("Usuario Um", "User@1234", "user1@teste.com", "11911111111"));
		Assert.Equal(HttpStatusCode.Created, reg1Resp.StatusCode);
		var user1 = await reg1Resp.Content.ReadFromJsonAsync<CreateClienteViewModel>();

		var reg2Resp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("Usuario Dois", "User@1234", "user2@teste.com", "11922222222"));
		Assert.Equal(HttpStatusCode.Created, reg2Resp.StatusCode);
		var user2 = await reg2Resp.Content.ReadFromJsonAsync<CreateClienteViewModel>();

		using var user2Client = factory.CreateAuthenticatedClient(user2!.Token);
		var revokeOtherResp = await user2Client.PostAsJsonAsync(
			"/api/login/logout",
			new RevokeTokenCommand(user1!.RefreshToken));

		Assert.Equal(HttpStatusCode.Forbidden, revokeOtherResp.StatusCode);
	}

	[Fact]
	public async Task LoginLockout_AposCincoFalhas_DeveBloquearMesmoComSenhaCorreta()
	{
		using var factory = new CustomWebApplicationFactory();
		using var anonClient = factory.CreateClient();

		var regResp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("Alvo Lockout", "User@1234", "lockout@teste.com", "11933333333"));
		Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

		for (var i = 0; i < 5; i++)
		{
			var failResp = await anonClient.PostAsJsonAsync(
				"/api/login",
				new LoginClienteCommand("lockout@teste.com", "SenhaErrada@1"));
			Assert.Equal(HttpStatusCode.NotFound, failResp.StatusCode);
		}

		// 6a tentativa com senha correta deve falhar por lockout ativo
		var blockedResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("lockout@teste.com", "User@1234"));
		Assert.Equal(HttpStatusCode.NotFound, blockedResp.StatusCode);
	}

	[Fact]
	public async Task RelatorioVendasCsv_DeveNeutralizarFormulaInjectionNoNomeDoCliente()
	{
		using var factory = new CustomWebApplicationFactory();
		var (_, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);
		using var anonClient = factory.CreateClient();

		var prodResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("=CMD|'/C calc'!A0", 100m, 10));
		Assert.Equal(HttpStatusCode.Created, prodResp.StatusCode);
		var prod = await prodResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var regResp = await anonClient.PostAsJsonAsync(
			"/api/cliente/registrar",
			new RegisterClienteCommand("=HYPERLINK(\"http://evil.local\";\"Clique\")", "User@1234", "csv@teste.com", "11944444444"));
		Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);
		var user = await regResp.Content.ReadFromJsonAsync<CreateClienteViewModel>();

		using var userClient = factory.CreateAuthenticatedClient(user!.Token);
		var pedidoResp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = prod!.Id, Quantidade = 1 }], user.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.Created, pedidoResp.StatusCode);

		var csvResp = await adminClient.GetAsync("/api/pedido/relatorio?periodo=Mes&formato=Csv");
		Assert.Equal(HttpStatusCode.OK, csvResp.StatusCode);

		var csvBytes = await csvResp.Content.ReadAsByteArrayAsync();
		var csvText = Encoding.UTF8.GetString(csvBytes, 3, csvBytes.Length - 3);

		// A célula maliciosa deve ser prefixada com apóstrofo e envolvida em aspas
		Assert.Contains("\"'=HYPERLINK(\"\"http://evil.local\"\";\"\"Clique\"\")\"", csvText);
		Assert.DoesNotContain(";=HYPERLINK", csvText);
	}

	[Fact]
	public async Task CorrelationId_HeaderComCaracteresInvalidos_DeveSerDescartadoESubstituidoPorGuid()
	{
		using var factory = new CustomWebApplicationFactory();
		using var client = factory.CreateClient();

		using var req = new HttpRequestMessage(HttpMethod.Get, "/health");
		req.Headers.TryAddWithoutValidation(AppDiagnostics.CorrelationIdHeader, "malicioso com espaco <script>");

		var resp = await client.SendAsync(req);
		Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
		Assert.True(resp.Headers.TryGetValues(AppDiagnostics.CorrelationIdHeader, out var values));
		var returnedId = values.Single();
		Assert.NotEqual("malicioso com espaco <script>", returnedId);
		Assert.True(Guid.TryParse(returnedId, out _));
	}

	[Fact]
	public async Task HealthCheckReady_NaoDeveExporCampoErrorNoPayload()
	{
		using var factory = new CustomWebApplicationFactory();
		using var client = factory.CreateClient();

		var resp = await client.GetAsync("/health/ready");
		Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

		using var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
		foreach (var check in json.RootElement.GetProperty("checks").EnumerateArray())
		{
			Assert.False(check.TryGetProperty("error", out _));
		}
	}

	[Fact]
	public async Task MovimentacaoEstoque_DeveRegistrarSempreOUsuarioAutenticadoIgnorandoClienteIdForjado()
	{
		using var factory = new CustomWebApplicationFactory();
		var (adminId, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		var prodResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("SSD NVMe", 500m, 10));
		Assert.Equal(HttpStatusCode.Created, prodResp.StatusCode);
		var prod = await prodResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var outroClienteId = Guid.NewGuid();
		var addResp = await adminClient.PutAsJsonAsync(
			"/api/produto/adicionar-estoque",
			new { ProdutoId = prod!.Id, Quantidade = 5, ClienteId = outroClienteId });
		Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);

		using var scope = factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();
		var mov = await db.MovimentacaoEstoques.AsNoTracking().SingleAsync(m => m.ProdutoId == prod.Id);

		Assert.Equal(adminId, mov.ClienteId);
		Assert.NotEqual(outroClienteId, mov.ClienteId);
	}
}

