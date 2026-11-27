using System.Net;
using System.Net.Http.Json;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.UpdateProduto;
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

		// 4. Soft Delete permite desativar produto e cliente mesmo com pedidos vinculados, preservando histórico e bloqueando login
		var deleteProdutoComPedido = await adminClient.DeleteAsync($"/api/produto/remover-produto?id={prodA.Id}");
		Assert.Equal(HttpStatusCode.OK, deleteProdutoComPedido.StatusCode);

		var getProdutoDesativado = await adminClient.GetAsync($"/api/produto/{prodA.Id}");
		Assert.Equal(HttpStatusCode.NotFound, getProdutoDesativado.StatusCode);

		var deleteClienteComPedido = await adminClient.DeleteAsync($"/api/cliente/deletar-cliente?id={user.Id}");
		Assert.Equal(HttpStatusCode.OK, deleteClienteComPedido.StatusCode);

		var getClienteDesativado = await adminClient.GetAsync($"/api/cliente/{user.Id}");
		Assert.Equal(HttpStatusCode.NotFound, getClienteDesativado.StatusCode);

		// Login de cliente desativado deve ser bloqueado (404 NotFound)
		var loginDesativadoResp = await anonClient.PostAsJsonAsync(
			"/api/login",
			new LoginClienteCommand("maria@teste.com", "Maria@123"));
		Assert.Equal(HttpStatusCode.NotFound, loginDesativadoResp.StatusCode);

		// Histórico do pedido continua acessível para o Admin com nome do cliente e produtos preservados
		var pedidoHistorico = await adminClient.GetFromJsonAsync<PedidoResponseDTO>($"/api/pedido/{pedidoCreated.Id}");
		Assert.NotNull(pedidoHistorico);
		Assert.Equal("Maria Silva", pedidoHistorico.ClienteNome);
		Assert.Equal(2, pedidoHistorico.ItensPedidos.Count);

		// 5. Usuario comum não pode deletar pedido (403 Forbidden)
		var deleteAsUser = await userClient.DeleteAsync($"/api/pedido/deletar-pedido?id={pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.Forbidden, deleteAsUser.StatusCode);

		// 6. Admin deleta pedido e consulta posterior retorna 404 (cache invalidado)
		var deleteAsAdmin = await adminClient.DeleteAsync($"/api/pedido/deletar-pedido?id={pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.OK, deleteAsAdmin.StatusCode);

		var getAfterDelete = await adminClient.GetAsync($"/api/pedido/{pedidoCreated.Id}");
		Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
	}

	[Fact]
	public async Task CancelarPedidoConcluido_DeveEstornarEstoqueEPreservarSnapshotDePrecoNoRelatorio()
	{
		using var factory = new CustomWebApplicationFactory();
		var (_, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		// Cria dois clientes e um produto com preço inicial de R$ 1.000,00
		var mariaResp = await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Maria Estorno", Role.Usuario, "Maria@123", "maria.estorno@teste.com", "11944440000"));
		var maria = await mariaResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIdResponse>();

		await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Outro Usuario", Role.Usuario, "Outro@123", "outro.estorno@teste.com", "11944440001"));

		var prodResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Monitor Ultra", 1000m, 10));
		var prod = await prodResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		using var anonClient = factory.CreateClient();
		var mariaToken = await factory.LoginAsync(anonClient, "maria.estorno@teste.com", "Maria@123");
		var outroToken = await factory.LoginAsync(anonClient, "outro.estorno@teste.com", "Outro@123");
		using var mariaClient = factory.CreateAuthenticatedClient(mariaToken);
		using var outroClient = factory.CreateAuthenticatedClient(outroToken);

		// 1. Maria cria pedido de 2 unidades -> aguarda conclusão (estoque vai de 10 para 8)
		var pedidoResp = await mariaClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = prod!.Id, Quantidade = 2 }], maria!.Id, Status.Pendente));
		Assert.Equal(HttpStatusCode.Created, pedidoResp.StatusCode);
		var pedidoCreated = await pedidoResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var pedidoConcluido = await AguardarStatusPedidoAsync(mariaClient, pedidoCreated!.Id, Status.Concluido);
		Assert.Equal(Status.Concluido, pedidoConcluido.Status);

		var prodAposVenda = await mariaClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prod.Id}");
		Assert.Equal(8, prodAposVenda!.Estoque);

		// 2. Admin altera o preço do produto para R$ 1.500,00 após a venda
		var updateProdResp = await adminClient.PutAsJsonAsync(
			"/api/produto/atualizar-produto",
			new UpdateProdutoCommand(prod.Id, "Monitor Ultra", 1500m, 8));
		Assert.Equal(HttpStatusCode.NoContent, updateProdResp.StatusCode);

		// 3. Outro usuário não pode cancelar pedido de Maria (403 Forbidden)
		var cancelOutroResp = await outroClient.PatchAsync($"/api/pedido/{pedidoCreated.Id}/cancelar", null);
		Assert.Equal(HttpStatusCode.Forbidden, cancelOutroResp.StatusCode);

		// 4. Maria cancela seu pedido -> estoque estornado de 8 para 10
		var cancelMariaResp = await mariaClient.PatchAsync($"/api/pedido/{pedidoCreated.Id}/cancelar", null);
		Assert.Equal(HttpStatusCode.OK, cancelMariaResp.StatusCode);
		var cancelResult = await cancelMariaResp.Content.ReadFromJsonAsync<CancelarPedidoViewModel>();
		Assert.NotNull(cancelResult);
		Assert.Equal(Status.Cancelado, cancelResult.Status);
		Assert.True(cancelResult.EstoqueEstornado);
		Assert.Equal(2, cancelResult.UnidadesEstornadas);

		var prodAposEstorno = await mariaClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prod.Id}");
		Assert.Equal(10, prodAposEstorno!.Estoque);

		// 5. Segunda tentativa de cancelamento deve retornar 409 Conflict
		var cancelDuplicadoResp = await mariaClient.PatchAsync($"/api/pedido/{pedidoCreated.Id}/cancelar", null);
		Assert.Equal(HttpStatusCode.Conflict, cancelDuplicadoResp.StatusCode);

		// 6. Detalhes da movimentação e relatório CSV preservam o snapshot de R$ 1.000,00 + motivos Venda e Estorno
		var movimentacoes = await adminClient.GetFromJsonAsync<PagedResultModel<MovimentacaoEstoqueResponseDTO>>(
			"/api/produto/movimentacao-estoque");
		Assert.NotNull(movimentacoes);
		Assert.Equal(2, movimentacoes.TotalCount);

		var csvResp = await adminClient.GetAsync("/api/produto/movimentacao-estoque/relatorio?periodo=Mes&formato=Csv");
		Assert.Equal(HttpStatusCode.OK, csvResp.StatusCode);
		var csvText = await csvResp.Content.ReadAsStringAsync();
		Assert.Contains("Venda (Pedido)", csvText);
		Assert.Contains("Estorno", csvText);
		Assert.Contains("1000,00", csvText);
		Assert.Contains("2000,00", csvText);
		Assert.DoesNotContain("3000,00", csvText); // Não usou o novo preço de R$ 1.500,00 retroativamente
	}

	[Fact]
	public async Task DashboardAnalyticsPorPeriodo_ERelatorioVendasCsvEPdf_DeveRetornarMetricasERankings()
	{
		using var factory = new CustomWebApplicationFactory();
		var (_, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		// Cria cliente comum e dois produtos
		var clienteResp = await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Carlos Analytics", Role.Usuario, "Carlos@123", "carlos@teste.com", "11988887777"));
		var cliente = await clienteResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIdResponse>();

		var prod1Resp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Notebook Gamer", 5000m, 10));
		var prod1 = await prod1Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var prod2Resp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Teclado RGB", 500m, 20));
		var prod2 = await prod2Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		using var anonClient = factory.CreateClient();
		var userToken = await factory.LoginAsync(anonClient, "carlos@teste.com", "Carlos@123");
		using var userClient = factory.CreateAuthenticatedClient(userToken);

		// Usuário comum não pode baixar relatório de vendas (403 Forbidden)
		var forbiddenResp = await userClient.GetAsync("/api/pedido/relatorio?periodo=Mes&formato=Csv");
		Assert.Equal(HttpStatusCode.Forbidden, forbiddenResp.StatusCode);

		// Cria 2 pedidos concluídos (5000 + 1000 = 6000) e 1 pedido cancelado por falta de estoque
		var p1Resp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = prod1!.Id, Quantidade = 1 }], cliente!.Id, Status.Pendente));
		var p1 = await p1Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();
		await AguardarStatusPedidoAsync(userClient, p1!.Id, Status.Concluido);

		var p2Resp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = prod2!.Id, Quantidade = 2 }], cliente.Id, Status.Pendente));
		var p2 = await p2Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();
		await AguardarStatusPedidoAsync(userClient, p2!.Id, Status.Concluido);

		var p3Resp = await userClient.PostAsJsonAsync(
			"/api/pedido",
			new CreatePedidoCommand([new ItemPedidoDTO { ProdutoId = prod1.Id, Quantidade = 50 }], cliente.Id, Status.Pendente));
		var p3 = await p3Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();
		await AguardarStatusPedidoAsync(userClient, p3!.Id, Status.Cancelado);

		// 1. Dashboard filtrado por Mês com Top Produtos, Ticket Médio e Taxa de Cancelamento
		var dashboardMes = await adminClient.GetFromJsonAsync<DashboardViewModel>("/api/dashboard?periodo=Mes&topProdutos=2");
		Assert.NotNull(dashboardMes);
		Assert.Equal(PeriodoRelatorio.Mes, dashboardMes.Periodo);
		Assert.Equal(3, dashboardMes.TotalPedidos);
		Assert.Equal(2, dashboardMes.PedidosConcluidos);
		Assert.Equal(1, dashboardMes.PedidosCancelados);
		Assert.Equal(6000m, dashboardMes.FaturamentoTotalConcluido);
		Assert.Equal(3000m, dashboardMes.TicketMedio);
		Assert.Equal(33.33m, dashboardMes.TaxaCancelamentoPercentual);
		Assert.NotNull(dashboardMes.TopProdutos);
		Assert.Equal(2, dashboardMes.TopProdutos!.Count);
		Assert.Equal("Notebook Gamer", dashboardMes.TopProdutos[0].ProdutoNome);
		Assert.Equal(5000m, dashboardMes.TopProdutos[0].FaturamentoTotal);

		// Dashboard de período passado sem pedidos deve retornar zeros
		var dashboardVazio = await adminClient.GetFromJsonAsync<DashboardViewModel>(
			"/api/dashboard?periodo=Ano&dataReferencia=2020-05-01");
		Assert.NotNull(dashboardVazio);
		Assert.Equal(0, dashboardVazio.TotalPedidos);
		Assert.Equal(0m, dashboardVazio.FaturamentoTotalConcluido);
		Assert.Empty(dashboardVazio.TopProdutos!);

		// 2. Relatório de Vendas em CSV (Mensal)
		var csvResp = await adminClient.GetAsync("/api/pedido/relatorio?periodo=Mes&formato=Csv");
		Assert.Equal(HttpStatusCode.OK, csvResp.StatusCode);
		Assert.Equal("text/csv", csvResp.Content.Headers.ContentType?.MediaType);

		var csvBytes = await csvResp.Content.ReadAsByteArrayAsync();
		Assert.Equal(0xEF, csvBytes[0]);
		Assert.Equal(0xBB, csvBytes[1]);
		Assert.Equal(0xBF, csvBytes[2]);

		var csvText = System.Text.Encoding.UTF8.GetString(csvBytes, 3, csvBytes.Length - 3);
		Assert.Contains("RESUMO EXECUTIVO", csvText);
		Assert.Contains("TOP PRODUTOS", csvText);
		Assert.Contains("CLIENTES MAIS ATIVOS", csvText);
		Assert.Contains("PEDIDOS DETALHADOS", csvText);
		Assert.Contains("Notebook Gamer", csvText);
		Assert.Contains("Teclado RGB", csvText);
		Assert.Contains("Carlos Analytics", csvText);
		Assert.Contains("6000,00", csvText);
		Assert.Contains("3000,00", csvText);

		// 3. Relatório de Vendas em PDF (Semanal e Anual vazio)
		var pdfResp = await adminClient.GetAsync("/api/pedido/relatorio?periodo=Semana&formato=Pdf");
		Assert.Equal(HttpStatusCode.OK, pdfResp.StatusCode);
		Assert.Equal("application/pdf", pdfResp.Content.Headers.ContentType?.MediaType);
		var pdfBytes = await pdfResp.Content.ReadAsByteArrayAsync();
		Assert.True(pdfBytes.Length > 500);
		Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5));

		var pdfVazioResp = await adminClient.GetAsync(
			"/api/pedido/relatorio?periodo=Ano&formato=Pdf&dataReferencia=2020-01-10");
		Assert.Equal(HttpStatusCode.OK, pdfVazioResp.StatusCode);
		var pdfVazioBytes = await pdfVazioResp.Content.ReadAsByteArrayAsync();
		Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdfVazioBytes, 0, 5));
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
