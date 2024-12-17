using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Application.Queries.GetAllPedidos;
using GerenciadorDePedidos.Application.Queries.GetPedido;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[Route("api/pedido")]
public class PedidoController : ControllerBase
{
	private readonly IMediator _mediator;
	
	[HttpGet]
	public async Task<IActionResult> GetAll(string? query)
	{
		var getAllPedidos = new GetAllPedidosQuery(query);
		var pedidos = await _mediator.Send(getAllPedidos);
		return Ok(pedidos);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var getPedidoById = new GetPedidoQuery(id);
		var pedido = await _mediator.Send(getPedidoById);
		return Ok(pedido);
	}

	[HttpPost]
	public async Task<IActionResult> Post([FromBody] CreatePedidoCommand command)
	{
		var pedidoId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new {id = pedidoId}, command);
	}
	
	[HttpDelete("deletar-pedido/{id}")]
	public async Task<IActionResult> Delete(DeletePedidoCommand command)
	{
		await _mediator.Send(command);
		return Ok("pedido deletado com sucesso");
	}
	
}