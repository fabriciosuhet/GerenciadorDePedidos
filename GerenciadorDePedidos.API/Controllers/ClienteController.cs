using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Application.Queries.GetAllClientes;
using GerenciadorDePedidos.Application.Queries.GetCliente;
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
	public async Task<IActionResult> GetAll(string? query)
	{
		var getAllClientes = new GetAllClientesQuery(query);
		var clientes = await _mediator.Send(getAllClientes);
		if (clientes is null) return NotFound();
		return Ok(clientes);
		
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var getClienteById = new GetClienteQuery(id);
		var cliente = await _mediator.Send(getClienteById);
		if (cliente is null) return NotFound();
		return Ok(cliente);
		
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
		await _mediator.Send(command);
		return NoContent();
	}

	[HttpDelete("deletar-cliente{id}")]
	public async Task<IActionResult> Delete(DeleteClienteCommand command)
	{
		await _mediator.Send(command);
		return Ok("Cliente deletado com sucesso");
	}
}