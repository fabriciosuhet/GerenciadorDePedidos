using System.Net;
using System.Net.Http.Json;
using GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;
using GerenciadorDePedidos.Application.Commands.UpdateProduto;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.E2ETests.Infrastructure;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.E2ETests;

public class ProdutoEstoqueE2ETests
{
	[Fact]
	public async Task FluxoCompleto_ProdutoEMovimentacaoEstoque_ComPermissoes()
	{
		using var factory = new CustomWebApplicationFactory();
		var (adminId, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		// Cria usuário comum para validar restrições de RBAC
		await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Comprador", Role.Usuario, "User@1234", "comprador@teste.com", "11955554444"));
		using var anonClient = factory.CreateClient();
		var userToken = await factory.LoginAsync(anonClient, "comprador@teste.com", "User@1234");
		using var userClient = factory.CreateAuthenticatedClient(userToken);

		// 1. Usuario comum não pode criar produto nem ver movimentação de estoque
		var createAsUser = await userClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Mouse", 100m, 10));
		Assert.Equal(HttpStatusCode.Forbidden, createAsUser.StatusCode);

		var movAsUser = await userClient.GetAsync("/api/produto/movimentacao-estoque");
		Assert.Equal(HttpStatusCode.Forbidden, movAsUser.StatusCode);

		// 2. Admin cria dois produtos
		var prod1Resp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Notebook Pro", 4500m, 10));
		Assert.Equal(HttpStatusCode.Created, prod1Resp.StatusCode);
		var prod1 = await prod1Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var prod2Resp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Teclado Mecânico", 350m, 20));
		Assert.Equal(HttpStatusCode.Created, prod2Resp.StatusCode);
		var prod2 = await prod2Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		// 3. Usuario comum pode listar e consultar produto por ID
		var listAsUser = await userClient.GetFromJsonAsync<PagedResultModel<ProdutoViewModel>>("/api/produto?query=Notebook");
		Assert.NotNull(listAsUser);
		Assert.Single(listAsUser.Items);
		Assert.Equal("Notebook Pro", listAsUser.Items[0].Nome);

		var prodDetails = await userClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prod1!.Id}");
		Assert.Equal(10, prodDetails!.Estoque);

		// 4. Admin adiciona e remove estoque e verifica movimentações
		var addResp = await adminClient.PutAsJsonAsync(
			"/api/produto/adicionar-estoque",
			new AdicionarProdutoEstoqueCommand(prod1.Id, 5, adminId));
		Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);

		var remResp = await adminClient.PutAsJsonAsync(
			"/api/produto/remover-estoque",
			new RemoverProdutoEstoqueCommand(prod2!.Id, 3, adminId));
		Assert.Equal(HttpStatusCode.OK, remResp.StatusCode);

		// Tentativa de remover além do estoque disponível deve retornar 400
		var overRemoveResp = await adminClient.PutAsJsonAsync(
			"/api/produto/remover-estoque",
			new RemoverProdutoEstoqueCommand(prod1.Id, 999, adminId));
		Assert.Equal(HttpStatusCode.BadRequest, overRemoveResp.StatusCode);

		// Verifica estoque atualizado (incluindo invalidação de cache)
		var prod1Updated = await userClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prod1.Id}");
		Assert.Equal(15, prod1Updated!.Estoque);

		// Verifica movimentações filtradas e consulta individual pelo ID exato (bug corrigido no repositório)
		var movimentacoes = await adminClient.GetFromJsonAsync<PagedResultModel<MovimentacaoEstoqueResponseDTO>>(
			"/api/produto/movimentacao-estoque");
		Assert.NotNull(movimentacoes);
		Assert.Equal(2, movimentacoes.TotalCount);

		var movRemocao = movimentacoes.Items.First(m => m.ProdutoId == prod2.Id);
		var movDetails = await adminClient.GetFromJsonAsync<MovimentacaoEstoqueDetailsDTO>(
			$"/api/produto/movimentacao-estoque/{movRemocao.Id}");
		Assert.NotNull(movDetails);
		Assert.Equal(prod2.Id, movDetails.ProdutoId);
		Assert.Equal(Tipo.Remocao, movDetails.TipoMovimentacao);
		Assert.Equal(3, movDetails.Quantidade);

		// 5. Atualiza e remove produto
		var updateProdResp = await adminClient.PutAsJsonAsync(
			"/api/produto/atualizar-produto",
			new UpdateProdutoCommand(prod1.Id, "Notebook Pro Max", 5000m, 15));
		Assert.Equal(HttpStatusCode.NoContent, updateProdResp.StatusCode);

		var prodAfterPut = await userClient.GetFromJsonAsync<ProdutoDetailsViewModel>($"/api/produto/{prod1.Id}");
		Assert.Equal("Notebook Pro Max", prodAfterPut!.Nome);

		var deleteProdResp = await adminClient.DeleteAsync($"/api/produto/remover-produto?id={prod1.Id}");
		Assert.Equal(HttpStatusCode.OK, deleteProdResp.StatusCode);

		var getDeletedProd = await userClient.GetAsync($"/api/produto/{prod1.Id}");
		Assert.Equal(HttpStatusCode.NotFound, getDeletedProd.StatusCode);

		// 6. Paginação em GET /api/produto e invalidação de páginas em cache ao criar novos produtos
		for (var i = 1; i <= 11; i++)
		{
			var resp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand($"Item Pag {i:D2}", 10m + i, 5));
			Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
		}

		var pagina1 = await userClient.GetFromJsonAsync<PagedResultModel<ProdutoViewModel>>("/api/produto?pageNumber=1&pageSize=10");
		var pagina2 = await userClient.GetFromJsonAsync<PagedResultModel<ProdutoViewModel>>("/api/produto?pageNumber=2&pageSize=10");
		Assert.NotNull(pagina1);
		Assert.NotNull(pagina2);
		Assert.Equal(12, pagina1.TotalCount);
		Assert.Equal(10, pagina1.Items.Count);
		Assert.Equal(2, pagina2.Items.Count);

		// Cria mais um produto e valida que o cache da página 2 foi invalidado
		var extraResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Item Pag 12", 99m, 5));
		Assert.Equal(HttpStatusCode.Created, extraResp.StatusCode);

		var pagina2Atualizada = await userClient.GetFromJsonAsync<PagedResultModel<ProdutoViewModel>>("/api/produto?pageNumber=2&pageSize=10");
		Assert.NotNull(pagina2Atualizada);
		Assert.Equal(13, pagina2Atualizada.TotalCount);
		Assert.Equal(3, pagina2Atualizada.Items.Count);
		Assert.Contains(pagina2Atualizada.Items, p => p.Nome == "Item Pag 12");
	}

	[Fact]
	public async Task DownloadRelatorioMovimentacaoEstoque_CsvEPdf_PorSemanaMesAno_EControleAcesso()
	{
		using var factory = new CustomWebApplicationFactory();
		var (adminId, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		// Cria usuário comum para validar restrição de acesso (apenas Admin)
		await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Comprador Relatorio", Role.Usuario, "User@1234", "relatorio@teste.com", "11944443333"));
		using var anonClient = factory.CreateClient();
		var userToken = await factory.LoginAsync(anonClient, "relatorio@teste.com", "User@1234");
		using var userClient = factory.CreateAuthenticatedClient(userToken);

		var forbiddenResp = await userClient.GetAsync("/api/produto/movimentacao-estoque/relatorio?periodo=Mes&formato=Csv");
		Assert.Equal(HttpStatusCode.Forbidden, forbiddenResp.StatusCode);

		// Cria produtos e registra movimentações de entrada e saída
		var prod1Resp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Monitor 4K", 2500m, 10));
		Assert.Equal(HttpStatusCode.Created, prod1Resp.StatusCode);
		var prod1 = await prod1Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var prod2Resp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Headset Pro", 400m, 15));
		Assert.Equal(HttpStatusCode.Created, prod2Resp.StatusCode);
		var prod2 = await prod2Resp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		var addResp = await adminClient.PutAsJsonAsync(
			"/api/produto/adicionar-estoque",
			new AdicionarProdutoEstoqueCommand(prod1!.Id, 4, adminId));
		Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);

		var remResp = await adminClient.PutAsJsonAsync(
			"/api/produto/remover-estoque",
			new RemoverProdutoEstoqueCommand(prod2!.Id, 3, adminId));
		Assert.Equal(HttpStatusCode.OK, remResp.StatusCode);

		// 1. Download CSV (Mensal)
		var csvResp = await adminClient.GetAsync("/api/produto/movimentacao-estoque/relatorio?periodo=Mes&formato=Csv");
		Assert.Equal(HttpStatusCode.OK, csvResp.StatusCode);
		Assert.Equal("text/csv", csvResp.Content.Headers.ContentType?.MediaType);
		Assert.Contains(".csv", csvResp.Content.Headers.ContentDisposition?.FileNameStar
			?? csvResp.Content.Headers.ContentDisposition?.FileName ?? string.Empty);

		var csvBytes = await csvResp.Content.ReadAsByteArrayAsync();
		// Verifica BOM UTF-8
		Assert.True(csvBytes.Length > 3);
		Assert.Equal(0xEF, csvBytes[0]);
		Assert.Equal(0xBB, csvBytes[1]);
		Assert.Equal(0xBF, csvBytes[2]);

		var csvText = System.Text.Encoding.UTF8.GetString(csvBytes, 3, csvBytes.Length - 3);
		Assert.Contains("RESUMO DO PERIODO", csvText);
		Assert.Contains("RESUMO POR PRODUTO", csvText);
		Assert.Contains("MOVIMENTACOES DETALHADAS", csvText);
		Assert.Contains("Monitor 4K", csvText);
		Assert.Contains("Headset Pro", csvText);
		Assert.Contains("10000,00", csvText); // 4 * 2500
		Assert.Contains("1200,00", csvText);  // 3 * 400

		// 2. Download PDF (Semanal e Anual)
		foreach (var periodo in new[] { "Semana", "Ano" })
		{
			var pdfResp = await adminClient.GetAsync($"/api/produto/movimentacao-estoque/relatorio?periodo={periodo}&formato=Pdf");
			Assert.Equal(HttpStatusCode.OK, pdfResp.StatusCode);
			Assert.Equal("application/pdf", pdfResp.Content.Headers.ContentType?.MediaType);
			Assert.Contains(".pdf", pdfResp.Content.Headers.ContentDisposition?.FileNameStar
				?? pdfResp.Content.Headers.ContentDisposition?.FileName ?? string.Empty);

			var pdfBytes = await pdfResp.Content.ReadAsByteArrayAsync();
			Assert.True(pdfBytes.Length > 500);
			var pdfHeader = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5);
			Assert.Equal("%PDF-", pdfHeader);
		}

		// 3. Período passado sem movimentações ainda gera PDF válido sem erro
		var pdfVazioResp = await adminClient.GetAsync(
			"/api/produto/movimentacao-estoque/relatorio?periodo=Ano&formato=Pdf&dataReferencia=2020-01-15");
		Assert.Equal(HttpStatusCode.OK, pdfVazioResp.StatusCode);
		var pdfVazioBytes = await pdfVazioResp.Content.ReadAsByteArrayAsync();
		Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdfVazioBytes, 0, 5));
	}

	[Fact]
	public async Task AlertaEstoqueBaixo_PorEvento_EResolucaoAutomatica_EConcorrenciaOtimista()
	{
		using var factory = new CustomWebApplicationFactory();
		var (adminId, adminToken) = await factory.BootstrapAdminAsync();
		using var adminClient = factory.CreateAuthenticatedClient(adminToken);

		// Cria usuário comum para validar RBAC da rota de alertas
		await adminClient.PostAsJsonAsync(
			"/api/cliente",
			new CreateClienteCommand("Comprador Alerta", Role.Usuario, "User@1234", "alerta@teste.com", "11933334444"));
		using var anonClient = factory.CreateClient();
		var userToken = await factory.LoginAsync(anonClient, "alerta@teste.com", "User@1234");
		using var userClient = factory.CreateAuthenticatedClient(userToken);

		var alertasForbidden = await userClient.GetAsync("/api/produto/alertas-estoque");
		Assert.Equal(HttpStatusCode.Forbidden, alertasForbidden.StatusCode);

		// 1. Cria produto com estoque inicial 6 e estoque mínimo 5
		var prodResp = await adminClient.PostAsJsonAsync("/api/produto", new CreateProdutoCommand("Webcam HD", 300m, 6, 5));
		Assert.Equal(HttpStatusCode.Created, prodResp.StatusCode);
		var prod = await prodResp.Content.ReadFromJsonAsync<CustomWebApplicationFactory.CreatedIntIdResponse>();

		// 2. Remove 2 unidades (estoque = 4 <= 5) -> dispara EstoqueBaixoEvent -> EstoqueBaixoConsumer persiste alerta
		var remResp = await adminClient.PutAsJsonAsync(
			"/api/produto/remover-estoque",
			new RemoverProdutoEstoqueCommand(prod!.Id, 2, adminId));
		Assert.Equal(HttpStatusCode.OK, remResp.StatusCode);

		var alertasPendentes = await AguardarAlertasPendentesAsync(adminClient, prod.Id);
		Assert.Single(alertasPendentes);
		Assert.Equal("Webcam HD", alertasPendentes[0].ProdutoNome);
		Assert.Equal(4, alertasPendentes[0].EstoqueAtual);
		Assert.Equal(5, alertasPendentes[0].EstoqueMinimo);
		Assert.False(alertasPendentes[0].Resolvido);

		// 3. Repõe estoque acima do mínimo (+10 -> estoque = 14 > 5) -> alerta é resolvido automaticamente
		var addResp = await adminClient.PutAsJsonAsync(
			"/api/produto/adicionar-estoque",
			new AdicionarProdutoEstoqueCommand(prod.Id, 10, adminId));
		Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);

		var pendentesAposReposicao = await adminClient.GetFromJsonAsync<List<AlertaEstoqueViewModel>>(
			"/api/produto/alertas-estoque?apenasPendentes=true");
		Assert.NotNull(pendentesAposReposicao);
		Assert.Empty(pendentesAposReposicao);

		var resolvidos = await adminClient.GetFromJsonAsync<List<AlertaEstoqueViewModel>>(
			"/api/produto/alertas-estoque?apenasPendentes=false");
		Assert.NotNull(resolvidos);
		Assert.Contains(resolvidos, a => a.ProdutoId == prod.Id && a.Resolvido && a.ResolvidoEm != null);

		// 4. Concorrência otimista: duas operações simultâneas no mesmo produto devem detectar conflito via Versao
		using var scope1 = factory.Services.CreateScope();
		using var scope2 = factory.Services.CreateScope();
		var db1 = scope1.ServiceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();
		var db2 = scope2.ServiceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();

		var prodCtx1 = await db1.Produtos.SingleAsync(p => p.Id == prod.Id);
		var prodCtx2 = await db2.Produtos.SingleAsync(p => p.Id == prod.Id);

		prodCtx1.RemoverEstoque(1);
		await db1.SaveChangesAsync();

		prodCtx2.RemoverEstoque(1);
		await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db2.SaveChangesAsync());
	}

	private static async Task<List<AlertaEstoqueViewModel>> AguardarAlertasPendentesAsync(
		HttpClient adminClient,
		int produtoId)
	{
		var deadline = DateTime.UtcNow.AddSeconds(10);
		List<AlertaEstoqueViewModel> alertas = [];

		while (DateTime.UtcNow < deadline)
		{
			alertas = await adminClient.GetFromJsonAsync<List<AlertaEstoqueViewModel>>(
				"/api/produto/alertas-estoque?apenasPendentes=true") ?? [];

			if (alertas.Any(a => a.ProdutoId == produtoId))
				return alertas;

			await Task.Delay(100);
		}

		return alertas;
	}
}
