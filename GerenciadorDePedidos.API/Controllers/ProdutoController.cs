using GerenciadorDePedidos.Application.Commands;
using GerenciadorDePedidos.Application.Commands.AdicionarPedidoEstoque;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Commands.DeleteProduto;
using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;
using GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetAllProdutos;
using GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetProduto;
using GerenciadorDePedidos.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[Route("api/produto")]
[Authorize]
public class ProdutoController : ControllerBase
{
	private readonly IMediator _mediator;

	public ProdutoController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetAll(string? query)
	{
		var getAllProdutos = new GetAllProdutosQuery(query);
		var produtos = await _mediator.Send(getAllProdutos);
		if (produtos is null) return NotFound();
		return Ok(produtos);
	}

	[HttpGet("movimentacao-estoque")]
	[Authorize(Roles = $"{nameof(Role.Admin)}")]
	public async Task<IActionResult> GetAllMovimentacaoEstoque(string? query)
	{
		var getAllMovimentacoes = new GetAllMovimentacaoEstoqueQuery(query);
		var movimentacoes = await _mediator.Send(getAllMovimentacoes);
		if (movimentacoes is null) return NotFound("Movimentacoes nao encontradas");
		return Ok(movimentacoes);
	}

	[HttpGet("movimentacao-estoque/{id:guid}")]
	[Authorize(Roles = $"{nameof(Role.Admin)}")]
	public async Task<IActionResult> GetMovimentacaoEstoque(Guid id)
	{
		var getMovimentacao = new GetMovimentacaoEstoqueQuery(id);
		var movimentacao = await _mediator.Send(getMovimentacao);
		if (movimentacao is null) return NotFound("Movimentacao nao encontrada");
		return Ok(movimentacao);
	}
	

	[HttpGet("{id:guid}")]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var getProdutoById = new GetProdutoQuery(id);
		var produto = await _mediator.Send(getProdutoById);
		if(produto is null) return NotFound();
		return Ok(produto);
	}

	[HttpPost]
	[Authorize(Roles = nameof(Role.Admin))]
	public async Task<IActionResult> Post([FromBody] CreateProdutoCommand command)
	{
		var produtoId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new { id = produtoId }, command);
	}

	[HttpPut("atualizar-produto/{id:guid}")]
	[Authorize(Roles = nameof(Role.Admin))]
	public async Task<IActionResult> Put(Guid id, [FromBody] UpdateProdutoCommand command)
	{
		await _mediator.Send(command);
		return NoContent();
	}
	
	[HttpPut("adicionar-estoque/{id:guid}")]
	[Authorize(Roles = $"{nameof(Role.Admin)}")]
	public async Task<IActionResult> PutAddEstoque(AdicionarProdutoEstoqueCommand command)
	{
		await _mediator.Send(command);
		return Ok("Produto adicionado com sucesso ao estoque.");
	}
	
	[HttpPut("remover-estoque/{id:guid}")]
	[Authorize(Roles = $"{nameof(Role.Admin)}")]
	public async Task<IActionResult> PutRemoveEstoque(RemoverProdutoEstoqueCommand command)
	{
		await _mediator.Send(command);
		return Ok("Produto removido com sucesso do estoque.");
	}

	[HttpDelete("remover-produto/{id:guid}")]
	[Authorize(Roles = nameof(Role.Admin))]
	public async Task<IActionResult> Delete(DeleteProdutoCommand command)
	{
		await _mediator.Send(command);
		return Ok("Produto removido com sucesso");
	}
}