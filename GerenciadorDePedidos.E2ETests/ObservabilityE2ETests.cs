using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Services;
using GerenciadorDePedidos.E2ETests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.E2ETests;

public class ObservabilityE2ETests
{
	[Fact]
	public async Task HealthChecks_LivenessEReadiness_DevemRetornarHealthyComJsonEstruturado()
	{
		using var factory = new CustomWebApplicationFactory();
		using var client = factory.CreateClient();

		// 1. Liveness (/health)
		var livenessResp = await client.GetAsync("/health");
		Assert.Equal(HttpStatusCode.OK, livenessResp.StatusCode);
		Assert.True(livenessResp.Headers.TryGetValues(AppDiagnostics.CorrelationIdHeader, out var generatedIds));
		Assert.False(string.IsNullOrWhiteSpace(generatedIds.Single()));

		using var livenessJson = JsonDocument.Parse(await livenessResp.Content.ReadAsStringAsync());
		Assert.Equal("Healthy", livenessJson.RootElement.GetProperty("status").GetString());

		// 2. Readiness (/health/ready) deve verificar banco de dados e bus MassTransit
		var readinessResp = await client.GetAsync("/health/ready");
		Assert.Equal(HttpStatusCode.OK, readinessResp.StatusCode);

		using var readinessJson = JsonDocument.Parse(await readinessResp.Content.ReadAsStringAsync());
		Assert.Equal("Healthy", readinessJson.RootElement.GetProperty("status").GetString());

		var checks = readinessJson.RootElement.GetProperty("checks").EnumerateArray().ToList();
		Assert.Contains(checks, c =>
			c.GetProperty("name").GetString() == "database" &&
			c.GetProperty("status").GetString() == "Healthy");
		Assert.Contains(checks, c =>
			c.GetProperty("name").GetString() == "masstransit-bus" &&
			c.GetProperty("status").GetString() == "Healthy");
	}

	[Fact]
	public async Task CorrelationId_DeveEcoarHeaderCustomizadoEPropagarEmEscoposAssincronos()
	{
		using var factory = new CustomWebApplicationFactory();
		var (adminId, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		// 1. CorrelationContext preserva isolamento em escopos aninhados
		var correlationContext = factory.Services.GetRequiredService<ICorrelationContext>();
		Assert.Null(correlationContext.CorrelationId);
		using (correlationContext.BeginScope("outer-corr"))
		{
			Assert.Equal("outer-corr", correlationContext.CorrelationId);
			using (correlationContext.BeginScope("inner-corr"))
			{
				Assert.Equal("inner-corr", correlationContext.CorrelationId);
			}
			Assert.Equal("outer-corr", correlationContext.CorrelationId);
		}
		Assert.Null(correlationContext.CorrelationId);

		// 2. Header X-Correlation-ID enviado pelo cliente é ecoado na resposta e propagado até o consumidor
		var customCorrelationId = Guid.NewGuid().ToString("D");

		var prodResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Cabo HDMI", 50m, 10));
		Assert.Equal(HttpStatusCode.Created, prodResp.StatusCode);
		var prod = await prodResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		using var pedidoRequest = new HttpRequestMessage(HttpMethod.Post, "/api/pedido")
		{
			Content = JsonContent.Create(new CreatePedidoCommand(
				[new ItemPedidoDTO { ProdutoId = prod!.Id, Quantidade = 1 }],
				adminId,
				Status.Pendente))
		};
		pedidoRequest.Headers.Add(AppDiagnostics.CorrelationIdHeader, customCorrelationId);

		var pedidoResp = await adminClient.SendAsync(pedidoRequest);
		Assert.Equal(HttpStatusCode.Created, pedidoResp.StatusCode);
		Assert.True(pedidoResp.Headers.TryGetValues(AppDiagnostics.CorrelationIdHeader, out var echoedIds));
		Assert.Equal(customCorrelationId, echoedIds.Single());

		var pedidoCreated = await pedidoResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();
		var deadline = DateTime.UtcNow.AddSeconds(10);
		PedidoResponseDTO? pedidoDetails = null;
		while (DateTime.UtcNow < deadline)
		{
			pedidoDetails = await adminClient.GetFromJsonAsync<PedidoResponseDTO>($"/api/pedido/{pedidoCreated!.Id}");
			if (pedidoDetails?.Status == Status.Concluido)
				break;
			await Task.Delay(100);
		}

		Assert.NotNull(pedidoDetails);
		Assert.Equal(Status.Concluido, pedidoDetails.Status);
	}
}

