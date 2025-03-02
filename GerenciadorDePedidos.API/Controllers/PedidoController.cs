using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Application.Queries.GetAllPedidos;
using GerenciadorDePedidos.Application.Queries.GetPedido;
using GerenciadorDePedidos.Core.Enums;
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
	public PedidoController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetAll(string? query)
	{
		var getAllPedidos = new GetAllPedidosQuery(query);
		var pedidos = await _mediator.Send(getAllPedidos);
		return Ok(pedidos);
	}

	[HttpGet("{id:guid}")]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var getPedidoById = new GetPedidoQuery(id);
		var pedido = await _mediator.Send(getPedidoById);
		if (pedido is null)
			return NotFound();
		
		return Ok(pedido);
	}

	[HttpPost]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> Post([FromBody] CreatePedidoCommand command)
	{
		var pedidoId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new {id = pedidoId}, command);
	}
	
	[HttpDelete("deletar-pedido/{id:guid}")]
	[Authorize(Roles = $"{nameof(Role.Admin)}")]
	public async Task<IActionResult> Delete(DeletePedidoCommand command)
	{
		await _mediator.Send(command);
		return Ok("pedido deletado com sucesso");
	}

	
	
}