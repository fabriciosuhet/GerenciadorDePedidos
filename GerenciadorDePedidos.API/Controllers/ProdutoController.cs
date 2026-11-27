using GerenciadorDePedidos.API.Authorization;
using GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Commands.DeleteProduto;
using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;
using GerenciadorDePedidos.Application.Commands.ResolverAlertaEstoque;
using GerenciadorDePedidos.Application.Commands.UpdateProduto;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Application.Queries.ExportarRelatorioMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetAlertasEstoque;
using GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetAllProdutos;
using GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetProduto;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/produto")]
[AcessoPadrao]
public class ProdutoController(IMediator mediator, ICacheService cacheService)
	: ApiControllerBase(mediator, cacheService)
{
	[HttpGet]
	public Task<IActionResult> GetAll(
		[FromQuery] string? query,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 10,
		CancellationToken cancellationToken = default)
	{
		var (pagina, tamanho) = NormalizarPaginacao(pageNumber, pageSize);
		return GetCachedOrNotFoundAsync<PagedResultModel<ProdutoViewModel>>(
			CacheKeyHelper.GetAllProdutosKey(query, pagina, tamanho),
			new GetAllProdutosQuery(query, pagina, tamanho),
			"Produtos nao encontrados",
			cancellationToken);
	}

	[HttpGet("{id:int}")]
	public Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
		=> GetCachedOrNotFoundAsync<ProdutoDetailsViewModel>(
			CacheKeyHelper.GetProdutoByIdKey(id),
			new GetProdutoQuery(id),
			"Produto nao encontrado",
			cancellationToken);

	[HttpGet("movimentacao-estoque")]
	[AcessoRestrito]
	public Task<IActionResult> GetAllMovimentacaoEstoque(
		[FromQuery] string? query,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 10,
		CancellationToken cancellationToken = default)
	{
		var (pagina, tamanho) = NormalizarPaginacao(pageNumber, pageSize);
		return GetOrNotFoundAsync(
			new GetAllMovimentacaoEstoqueQuery(query, pagina, tamanho),
			"Movimentacoes nao encontradas",
			cancellationToken);
	}

	[HttpGet("movimentacao-estoque/{id:int}")]
	[AcessoRestrito]
	public Task<IActionResult> GetMovimentacaoEstoque(int id, CancellationToken cancellationToken)
		=> GetOrNotFoundAsync(
			new GetMovimentacaoEstoqueQuery(id),
			"Movimentacao nao encontrada",
			cancellationToken);

	[HttpGet("movimentacao-estoque/relatorio")]
	[AcessoRestrito]
	public async Task<IActionResult> DownloadRelatorioMovimentacaoEstoque(
		[FromQuery] PeriodoRelatorio periodo = PeriodoRelatorio.Mes,
		[FromQuery] FormatoRelatorio formato = FormatoRelatorio.Pdf,
		[FromQuery] DateTime? dataReferencia = null,
		CancellationToken cancellationToken = default)
	{
		var arquivo = await SendAsync(
			new ExportarRelatorioMovimentacaoEstoqueQuery(periodo, formato, dataReferencia),
			cancellationToken);

		return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
	}

	[HttpGet("alertas-estoque")]
	[AcessoRestrito]
	public async Task<IActionResult> GetAlertasEstoque(
		[FromQuery] bool? apenasPendentes = true,
		CancellationToken cancellationToken = default)
		=> Ok(await SendAsync(new GetAlertasEstoqueQuery(apenasPendentes), cancellationToken));

	[HttpPatch("alertas-estoque/{id:int}/resolver")]
	[AcessoRestrito]
	public async Task<IActionResult> ResolverAlertaEstoque(int id, CancellationToken cancellationToken)
	{
		await SendAsync(new ResolverAlertaEstoqueCommand(id), cancellationToken);
		return NoContent();
	}

	[HttpPost]
	[AcessoRestrito]
	public async Task<IActionResult> Post([FromBody] CreateProdutoCommand command, CancellationToken cancellationToken)
	{
		var produtoId = await SendAsync(command, cancellationToken);
		CacheService?.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix);
		return CreatedAtAction(nameof(GetById), new { id = produtoId }, new { id = produtoId });
	}

	[HttpPut("atualizar-produto")]
	[AcessoRestrito]
	public async Task<IActionResult> Put([FromBody] UpdateProdutoCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		InvalidarCacheProduto(command.Id);
		return NoContent();
	}

	[HttpPut("adicionar-estoque")]
	[AcessoRestrito]
	public async Task<IActionResult> PutAddEstoque([FromBody] AdicionarProdutoEstoqueCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		InvalidarCacheProduto(command.ProdutoId);
		return Ok("Produto adicionado com sucesso ao estoque.");
	}

	[HttpPut("remover-estoque")]
	[AcessoRestrito]
	public async Task<IActionResult> PutRemoveEstoque([FromBody] RemoverProdutoEstoqueCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		InvalidarCacheProduto(command.ProdutoId);
		return Ok("Produto removido com sucesso do estoque.");
	}

	[HttpDelete("remover-produto")]
	[AcessoRestrito]
	public async Task<IActionResult> Delete([FromQuery] int id, CancellationToken cancellationToken)
	{
		await SendAsync(new DeleteProdutoCommand(id), cancellationToken);
		InvalidarCacheProduto(id);
		return Ok("Produto removido com sucesso");
	}

	private void InvalidarCacheProduto(int id)
	{
		CacheService?.Remove(CacheKeyHelper.GetProdutoByIdKey(id));
		CacheService?.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix);
	}
}