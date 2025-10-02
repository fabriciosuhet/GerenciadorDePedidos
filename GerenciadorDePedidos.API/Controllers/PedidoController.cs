using GerenciadorDePedidos.API.Helpers;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Application.Queries.GetAllPedidos;
using GerenciadorDePedidos.Application.Queries.GetPedido;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[Authorize]
[ApiController]
[Route("api/pedido")]
public class PedidoController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ICacheService _cacheService;

	public PedidoController(IMediator mediator, ICacheService cacheService)
	{
		_mediator = mediator;
		_cacheService = cacheService;
	}

	[HttpGet]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetAll(string? query)
	{
		var cacheKey = CacheKeyHelper.GetAllPedidosKey(query);

		if (!_cacheService.TryGet<PagedResultModel<PedidoViewModel>>(cacheKey, out var pedidos))
		{
			var getAllPedidos = new GetAllPedidosQuery(query);
			pedidos = await _mediator.Send(getAllPedidos);
			if (pedidos is null) return NotFound("Pedidos nao encontrados");
			
			_cacheService.Set(cacheKey, pedidos, TimeSpan.FromMicroseconds(5));
		}
		return Ok(pedidos);
	}

	[HttpGet("{id:int}")]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetById(int id)
	{
		var cacheKey = CacheKeyHelper.GetPedidoByIdKey(id);

		if (!_cacheService.TryGet<PedidoRespondeDTO>(cacheKey, out var pedido))
		{
			var getPedidoById = new GetPedidoQuery(id);
			pedido = await _mediator.Send(getPedidoById);
			if (pedido is null) return NotFound("Pedido nao encontrado");
			
			_cacheService.Set(cacheKey, pedido, TimeSpan.FromMicroseconds(5));
		}
		return Ok(pedido);
	}

	[HttpPost]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> Post([FromBody] CreatePedidoCommand command)
	{
		
		var pedidoId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new {id = pedidoId}, command);
	}
	
	[HttpDelete("deletar-pedido/{id:int}")]
	[Authorize(Roles = $"{nameof(Role.Admin)}")]
	public async Task<IActionResult> Delete(DeletePedidoCommand command)
	{
		await _mediator.Send(command);
		return Ok("pedido deletado com sucesso");
	}

	
	
}