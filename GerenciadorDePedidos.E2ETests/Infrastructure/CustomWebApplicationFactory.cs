using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GerenciadorDePedidos.E2ETests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly string _databaseName = $"GerenciadorDePedidos_E2E_{Guid.NewGuid()}";
	private IHost? _kestrelHost;

	public string ServerAddress { get; private set; } = string.Empty;

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");

		builder.ConfigureAppConfiguration((_, config) =>
		{
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Jwt:Key"] = "ChaveSecretaExclusivaParaTestesE2EComMaisDe32Bytes!",
				["Jwt:Issuer"] = "GerenciadorDePedidos",
				["Jwt:Audience"] = "ClientesProdutos",
				["AdminSeed:NomeCompleto"] = "Administrador",
				["AdminSeed:Telefone"] = "11999999999",
				["AdminSeed:Email"] = "admin@teste.com",
				["AdminSeed:Password"] = "Admin@123"
			});
		});

		builder.ConfigureServices(services =>
		{
			services.RemoveAll(typeof(DbContextOptions<GerenciadorDePedidosDbContext>));
			services.RemoveAll(typeof(GerenciadorDePedidosDbContext));

			services.AddDbContext<GerenciadorDePedidosDbContext>(options =>
				options.UseInMemoryDatabase(_databaseName));
		});
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		var testHost = builder.Build();

		builder.ConfigureWebHost(webHostBuilder =>
		{
			webHostBuilder.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
		});

		_kestrelHost = builder.Build();
		_kestrelHost.Start();

		var server = _kestrelHost.Services.GetRequiredService<IServer>();
		var addresses = server.Features.Get<IServerAddressesFeature>();
		ServerAddress = addresses!.Addresses.First();
		ClientOptions.BaseAddress = new Uri(ServerAddress);

		testHost.Start();
		return testHost;
	}

	public new HttpClient CreateClient()
	{
		_ = Services;
		return new HttpClient { BaseAddress = new Uri(ServerAddress) };
	}

	public async Task<(Guid ClienteId, string Token)> BootstrapAdminAsync(
		string email = "admin@teste.com",
		string senha = "Admin@123")
	{
		using var client = CreateClient();
		var token = await LoginAsync(client, email, senha);

		using var authClient = CreateAuthenticatedClient(token);
		var clientes = await authClient.GetFromJsonAsync<PagedResultModel<ClienteViewModel>>("/api/cliente");
		var adminId = clientes!.Items.First().Id;

		return (adminId, token);
	}

	public async Task<string> LoginAsync(HttpClient client, string email, string password)
	{
		var loginResponse = await client.PostAsJsonAsync("/api/login", new LoginClienteCommand(email, password));
		if (!loginResponse.IsSuccessStatusCode)
		{
			var body = await loginResponse.Content.ReadAsStringAsync();
			throw new HttpRequestException($"Login falhou ({(int)loginResponse.StatusCode}): {body}");
		}

		var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginClienteViewModel>();
		return loginResult!.Token;
	}

	public HttpClient CreateAuthenticatedClient(string token)
	{
		var client = CreateClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		return client;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
			_kestrelHost?.Dispose();
	}

	public sealed record CreatedIdResponse(Guid Id);
	public sealed record CreatedIntIdResponse(int Id);
}
