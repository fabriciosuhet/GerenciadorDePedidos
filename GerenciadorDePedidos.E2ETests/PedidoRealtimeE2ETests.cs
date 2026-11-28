using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using GerenciadorDePedidos.API.Controllers;
using GerenciadorDePedidos.API.Hubs;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.E2ETests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;

namespace GerenciadorDePedidos.E2ETests;

public class PedidoRealtimeE2ETests
{
	[Fact]
	public async Task PedidosHub_ConexaoSemToken_DeveSerRejeitadaCom401Unauthorized()
	{
		using var factory = new CustomWebApplicationFactory();
		using var _ = factory.CreateClient(); // Inicializa o Kestrel host

		await using var anonConnection = new HubConnectionBuilder()
			.WithUrl($"{factory.ServerAddress.TrimEnd('/')}{PedidosHub.Route}")
			.Build();

		var ex = await Assert.ThrowsAsync<HttpRequestException>(() => anonConnection.StartAsync());
		Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
	}

	[Fact]
	public async Task PedidosHub_MudancasDeStatus_DevemNotificarDonoEAdminEmTempoRealSemVazarParaTerceiros()
	{
		using var factory = new CustomWebApplicationFactory();
		var (_, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		var clienteAResp = await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Cliente A Realtime", Role.Usuario, "UserA@123", "clientea.rt@teste.com", "11911110001"));
		var clienteA = await clienteAResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIdResponse>();

		await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Cliente B Realtime", Role.Usuario, "UserB@123", "clienteb.rt@teste.com", "11911110002"));

		var prodResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Teclado Mecânico", 450m, 20));
		var prod = await prodResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		using var anonClient = factory.CreateClient();
		var tokenA = await factory.LoginAsync(anonClient, "clientea.rt@teste.com", "UserA@123");
		var tokenB = await factory.LoginAsync(anonClient, "clienteb.rt@teste.com", "UserB@123");
		using var clientA = factory.CreateAuthenticatedClient(tokenA);

		var eventosClienteA = new ConcurrentQueue<PedidoStatusAtualizadoDTO>();
		var eventosClienteB = new ConcurrentQueue<PedidoStatusAtualizadoDTO>();
		var eventosAdmin = new ConcurrentQueue<PedidoStatusAtualizadoDTO>();

		await using var hubAdmin = CriarConexaoHub(factory.ServerAddress, adminToken, eventosAdmin);
		await using var hubClienteA = CriarConexaoHub(factory.ServerAddress, tokenA, eventosClienteA);
		await using var hubClienteB = CriarConexaoHub(factory.ServerAddress, tokenB, eventosClienteB);

		await Task.WhenAll(hubAdmin.StartAsync(), hubClienteA.StartAsync(), hubClienteB.StartAsync());

		// 1. Cliente A cria pedido -> notifica Pendente e depois Processado (via fila)
		var createResp = await clientA.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = prod!.Id, Quantidade = 2 }], clienteA!.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
		var pedidoCreated = await createResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();
		var pedidoId = pedidoCreated!.Id;

		await AguardarEventoAsync(eventosClienteA, e => e.PedidoId == pedidoId && e.StatusAtual == Status.Processado);
		await AguardarEventoAsync(eventosAdmin, e => e.PedidoId == pedidoId && e.StatusAtual == Status.Processado);

		// 2. Admin conclui entrega -> notifica Processado -> Concluido
		var concluirResp = await adminClient.PatchAsync($"/api/pedido/{pedidoId}/concluir", null);
		Assert.Equal(HttpStatusCode.OK, concluirResp.StatusCode);

		await AguardarEventoAsync(
			eventosClienteA,
			e => e.PedidoId == pedidoId && e.StatusAnterior == Status.Processado && e.StatusAtual == Status.Concluido);
		await AguardarEventoAsync(
			eventosAdmin,
			e => e.PedidoId == pedidoId && e.StatusAnterior == Status.Processado && e.StatusAtual == Status.Concluido);

		// 3. Admin devolve pedido -> notifica Concluido -> Devolvido com MotivoDevolucao
		var devolverResp = await adminClient.PatchAsJsonAsync(
			$"/api/pedido/{pedidoId}/devolver",
			new DevolverPedidoRequest("Produto com tecla defeituosa", RetornarAoEstoque: false));
		Assert.Equal(HttpStatusCode.OK, devolverResp.StatusCode);

		var eventoDevolucaoClienteA = await AguardarEventoAsync(
			eventosClienteA,
			e => e.PedidoId == pedidoId && e.StatusAnterior == Status.Concluido && e.StatusAtual == Status.Devolvido);
		Assert.Equal("Produto com tecla defeituosa", eventoDevolucaoClienteA.MotivoDevolucao);
		Assert.Equal(900m, eventoDevolucaoClienteA.Total);

		await AguardarEventoAsync(
			eventosAdmin,
			e => e.PedidoId == pedidoId && e.StatusAtual == Status.Devolvido);

		// 4. Isolamento de dono: Cliente B NÃO deve receber nenhum evento de pedidos do Cliente A
		await Task.Delay(250);
		Assert.Empty(eventosClienteB);
	}

	private static HubConnection CriarConexaoHub(
		string serverAddress,
		string accessToken,
		ConcurrentQueue<PedidoStatusAtualizadoDTO> sink)
	{
		var connection = new HubConnectionBuilder()
			.WithUrl($"{serverAddress.TrimEnd('/')}{PedidosHub.Route}", options =>
			{
				options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
			})
			.Build();

		connection.On<PedidoStatusAtualizadoDTO>(
			PedidosHub.EventoPedidoStatusAtualizado,
			notificacao => sink.Enqueue(notificacao));

		return connection;
	}

	private static async Task<PedidoStatusAtualizadoDTO> AguardarEventoAsync(
		ConcurrentQueue<PedidoStatusAtualizadoDTO> fila,
		Func<PedidoStatusAtualizadoDTO, bool> predicado)
	{
		var deadline = DateTime.UtcNow.AddSeconds(10);
		while (DateTime.UtcNow < deadline)
		{
			var match = fila.FirstOrDefault(predicado);
			if (match is not null)
				return match;

			await Task.Delay(50);
		}

		throw new TimeoutException($"Evento SignalR esperado não foi recebido dentro do timeout. Total recebido: {fila.Count}.");
	}
}

