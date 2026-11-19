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
}
