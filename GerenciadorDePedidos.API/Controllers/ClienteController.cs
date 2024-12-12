using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[Route("api/cliente")]
public class ClienteController : ControllerBase
{
	private readonly IMediator _mediator;

	public ClienteController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet]
	public IActionResult GetAll(string? query)
	{
		// return NotFound();
		return Ok();
	}

	[HttpGet("{id}")]
	public IActionResult GetById(Guid id)
	{
		return Ok();
	}

	[HttpPost]
	public async Task<IActionResult> Post([FromBody] CreateClienteCommand command)
	{
		var clienteId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new { id = clienteId}, command);
	}

	[HttpPut("atualizar-cliente{id}")]
	public async Task<IActionResult> Put(Guid id, [FromBody] UpdateClienteCommand command)
	{
		var clienteId = await _mediator.Send(command);
		return NoContent();
	}

	[HttpDelete("deletar-cliente{id}")]
	public async Task<IActionResult> Delete(DeleteClienteCommand command)
	{
		await _mediator.Send(command);
		return Ok("Cliente deletado com sucesso");
	}
}