using GerenciadorDePedidos.API.Authorization;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Application.Queries.GetAllPedidos;
using GerenciadorDePedidos.Application.Queries.GetPedido;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/pedido")]
[AcessoPadrao]
public class PedidoController(IMediator mediator, ICacheService cacheService)
	: ApiControllerBase(mediator, cacheService)
{
	[HttpGet]
	public Task<IActionResult> GetAll([FromQuery] string? query, CancellationToken cancellationToken)
		=> GetCachedOrNotFoundAsync<PagedResultModel<PedidoViewModel>>(
			CacheKeyHelper.GetAllPedidosKey(query),
			new GetAllPedidosQuery(query),
			"Pedidos nao encontrados",
			cancellationToken);

	[HttpGet("{id:int}")]
	public Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
		=> GetCachedOrNotFoundAsync<PedidoResponseDTO>(
			CacheKeyHelper.GetPedidoByIdKey(id),
			new GetPedidoQuery(id),
			"Pedido nao encontrado",
			cancellationToken);

	[HttpPost]
	public async Task<IActionResult> Post([FromBody] CreatePedidoCommand command, CancellationToken cancellationToken)
	{
		var pedidoId = await SendAsync(command, cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetAllPedidosKey(null));
		return CreatedAtAction(nameof(GetById), new { id = pedidoId }, new { id = pedidoId });
	}

	[HttpDelete("deletar-pedido")]
	[AcessoRestrito]
	public async Task<IActionResult> Delete([FromQuery] int id, CancellationToken cancellationToken)
	{
		await SendAsync(new DeletePedidoCommand(id), cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetPedidoByIdKey(id));
		CacheService?.Remove(CacheKeyHelper.GetAllPedidosKey(null));
		return Ok("pedido deletado com sucesso");
	}
}