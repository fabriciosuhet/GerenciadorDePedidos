using System.Net;
using System.Net.Http.Json;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.E2ETests.Infrastructure;

namespace GerenciadorDePedidos.E2ETests;

public class PedidoE2ETests
{
	[Fact]
	public async Task FluxoCompleto_CriacaoConsultaEDelecaoDePedido()
	{
		using var factory = new CustomWebApplicationFactory();
		var (_, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		// Cria usuário comum e dois produtos
		var userResp = await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Maria Silva", Role.Usuario, "Maria@123", "maria@teste.com", "11944443333"));
		var user = await userResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIdResponse>();

		var prodAResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Monitor 27\"", 1200m, 10));
		var prodA = await prodAResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var prodBResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Headset", 300m, 15));
		var prodB = await prodBResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		using var anonClient = factory.CreateClient();
		var userToken = await factory.LoginAsync(anonClient, "maria@teste.com", "Maria@123");
		using var userClient = factory.CreateAuthenticatedClient(userToken);

		// Popula cache de /api/cliente/me antes de criar pedido para validar invalidação posterior
		var meAntes = await userClient.GetFromJsonAsync<ClienteDetailsViewModel>("/api/cliente/me");
		Assert.NotNull(meAntes);
		Assert.Empty(meAntes.Pedidos);

		// 1. Pedido sem itens ou com produto inexistente deve retornar 400 BadRequest
		var emptyPedidoResp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([], user!.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.BadRequest, emptyPedidoResp.StatusCode);

		var missingProdResp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = 9999, Quantidade = 1 }], user.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.BadRequest, missingProdResp.StatusCode);

		// 2. Usuario cria pedido válido com múltiplos itens (entra como Pendente na fila)
		var createPedidoResp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand(
				[
					new ItemPedidoDTO { ProdutoId = prodA!.Id, Quantidade = 2 },
					new ItemPedidoDTO { ProdutoId = prodB!.Id, Quantidade = 3 }
				],
				user.Id,
				Status.Pendente));
		Assert.Equal(HttpStatusCode.Created, createPedidoResp.StatusCode);
		var pedidoCreated = await createPedidoResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		// Aguarda o MassTransit + PedidoCriadoConsumer processar e concluir o pedido
		var pedidoDetails = await AguardarStatusPedidoAsync(userClient, pedidoCreated!.Id, Status.Concluido);
		Assert.Equal(Status.Concluido, pedidoDetails.Status);
		Assert.Equal(3300m, pedidoDetails.Total);
		Assert.Equal("Maria Silva", pedidoDetails.ClienteNome);
		Assert.Equal(2, pedidoDetails.ItensPedidos.Count);

		// /api/cliente/me deve refletir o novo pedido (cache de cliente invalidado após criação e processamento)
		var meDepois = await userClient.GetFromJsonAsync<ClienteDetailsViewModel>("/api/cliente/me");
		Assert.NotNull(meDepois);
		Assert.Contains(meDepois.Pedidos, p => p.Id == pedidoCreated.Id && p.Status == Status.Concluido);

		// Valida que o estoque dos produtos foi reduzido automaticamente após conclusão
		var prodAAtualizado = await userClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prodA.Id}");
		var prodBAtualizado = await userClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prodB.Id}");
		Assert.Equal(8, prodAAtualizado!.Estoque);
		Assert.Equal(12, prodBAtualizado!.Estoque);

		// Valida que as movimentações de saída foram registradas automaticamente
		var movimentacoes = await adminClient.GetFromJsonAsync<PagedResultModel<MovimentacaoEstoqueResponseDTO>>(
			"/api/produto/movimentacao-estoque");
		Assert.NotNull(movimentacoes);
		Assert.Equal(2, movimentacoes.TotalCount);

		// Listagem paginada também deve refletir o total calculado e o status Concluido
		var pedidosPaged = await userClient.GetFromJsonAsync<PagedResultModel<PedidoViewModel>>("/api/pedido?query=Concluido");
		Assert.NotNull(pedidosPaged);
		Assert.Single(pedidosPaged.Items);
		Assert.Equal(3300m, pedidosPaged.Items[0].Total);

		// 3. Pedido com quantidade acima do estoque deve ser cancelado pela fila sem alterar estoque
		var canceladoResp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand(
				[new ItemPedidoDTO { ProdutoId = prodA.Id, Quantidade = 99 }],
				user.Id,
				Status.Pendente));
		Assert.Equal(HttpStatusCode.Created, canceladoResp.StatusCode);
		var pedidoCanceladoCreated = await canceladoResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var pedidoCancelado = await AguardarStatusPedidoAsync(userClient, pedidoCanceladoCreated!.Id, Status.Cancelado);
		Assert.Equal(Status.Cancelado, pedidoCancelado.Status);

		var prodAAposCancelamento = await userClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prodA.Id}");
		Assert.Equal(8, prodAAposCancelamento!.Estoque);

		// 3b. Isolamento de dono e dashboard administrativo
		var mariaPedidosAll = await userClient.GetFromJsonAsync<PagedResultModel<PedidoViewModel>>("/api/pedido");
		Assert.NotNull(mariaPedidosAll);
		Assert.Equal(2, mariaPedidosAll.TotalCount);

		var dashboardResp = await adminClient.GetAsync("/api/dashboard");
		Assert.Equal(HttpStatusCode.OK, dashboardResp.StatusCode);
		var dashboard = await dashboardResp.Content.ReadFromJsonAsync<DashboardViewModel>();
		Assert.NotNull(dashboard);
		Assert.Equal(1, dashboard.PedidosConcluidos);
		Assert.Equal(1, dashboard.PedidosCancelados);
		Assert.Equal(3300m, dashboard.FaturamentoTotalConcluido);

		await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Joao Souza", Role.Usuario, "Joao@123", "joao@teste.com", "11933332222"));
		var joaoToken = await factory.LoginAsync(anonClient, "joao@teste.com", "Joao@123");
		using var joaoClient = factory.CreateAuthenticatedClient(joaoToken);

		var joaoPedidos = await joaoClient.GetFromJsonAsync<PagedResultModel<PedidoViewModel>>("/api/pedido");
		Assert.NotNull(joaoPedidos);
		Assert.Empty(joaoPedidos.Items);
		Assert.Equal(0, joaoPedidos.TotalCount);

		var joaoGetMariaPedido = await joaoClient.GetAsync($"/api/pedido/{pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.Forbidden, joaoGetMariaPedido.StatusCode);

		var joaoCreateForMaria = await joaoClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = prodA.Id, Quantidade = 1 }], user.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.Forbidden, joaoCreateForMaria.StatusCode);

		// 4. Usuario comum não pode deletar pedido (403 Forbidden)
		var deleteAsUser = await userClient.DeleteAsync($"/api/pedido/deletar-pedido?id={pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.Forbidden, deleteAsUser.StatusCode);

		// 5. Admin deleta pedido e consulta posterior retorna 404 (cache invalidado)
		var deleteAsAdmin = await adminClient.DeleteAsync($"/api/pedido/deletar-pedido?id={pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.OK, deleteAsAdmin.StatusCode);

		var getAfterDelete = await adminClient.GetAsync($"/api/pedido/{pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
	}

	private static async Task<PedidoResponseDTO> AguardarStatusPedidoAsync(
		HttpClient client,
		int pedidoId,
		Status statusEsperado)
	{
		var deadline = DateTime.UtcNow.AddSeconds(10);
		PedidoResponseDTO? pedido = null;

		while (DateTime.UtcNow < deadline)
		{
			pedido = await client.GetFromJsonAsync<PedidoResponseDTO>($"/api/pedido/{pedidoId}");
			if (pedido is not null && pedido.Status == statusEsperado)
				return pedido;

			await Task.Delay(100);
		}

		return pedido!;
	}
}
