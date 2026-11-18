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

		// 1. Pedido sem itens ou com produto inexistente deve retornar 400 BadRequest
		var emptyPedidoResp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([], user!.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.BadRequest, emptyPedidoResp.StatusCode);

		var missingProdResp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = 9999, Quantidade = 1 }], user.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.BadRequest, missingProdResp.StatusCode);

		// 2. Usuario cria pedido válido com múltiplos itens
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

		// Total esperado: (2 * 1200) + (3 * 300) = 3300
		var pedidoDetails = await userClient.GetFromJsonAsync<PedidoResponseDTO>($"/api/pedido/{pedidoCreated!.Id}");
		Assert.NotNull(pedidoDetails);
		Assert.Equal(3300m, pedidoDetails.Total);
		Assert.Equal("Maria Silva", pedidoDetails.ClienteNome);
		Assert.Equal(2, pedidoDetails.ItensPedidos.Count);

		// Listagem paginada também deve refletir o total calculado
		var pedidosPaged = await userClient.GetFromJsonAsync<PagedResultModel<PedidoViewModel>>("/api/pedido?query=Pendente");
		Assert.NotNull(pedidosPaged);
		Assert.Single(pedidosPaged.Items);
		Assert.Equal(3300m, pedidosPaged.Items[0].Total);

		// 3. Usuario comum não pode deletar pedido (403 Forbidden)
		var deleteAsUser = await userClient.DeleteAsync($"/api/pedido/deletar-pedido?id={pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.Forbidden, deleteAsUser.StatusCode);

		// 4. Admin deleta pedido e consulta posterior retorna 404 (cache invalidado)
		var deleteAsAdmin = await adminClient.DeleteAsync($"/api/pedido/deletar-pedido?id={pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.OK, deleteAsAdmin.StatusCode);

		var getAfterDelete = await adminClient.GetAsync($"/api/pedido/{pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
	}
}
