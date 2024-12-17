using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Application.Queries.GetAllClientes;
using GerenciadorDePedidos.Application.Queries.GetCliente;
using GerenciadorDePedidos.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[Route("api/cliente")]
[Authorize]
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
	[AllowAnonymous]
	public async Task<IActionResult> Post([FromBody] CreateClienteCommand command)
	{
		var clienteId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new { id = clienteId}, command);
	}

	[HttpPut("atualizar-cliente{id}")]
	[Authorize(Roles = nameof(Role.Admin))]
	[Authorize(Roles = nameof(Role.Usuario))]
	public async Task<IActionResult> Put(Guid id, [FromBody] UpdateClienteCommand command)
	{
		await _mediator.Send(command);
		return NoContent();
	}

	[HttpDelete("deletar-cliente{id}")]
	[Authorize(Roles = nameof(Role.Admin))]
	public async Task<IActionResult> Delete(DeleteClienteCommand command)
	{
		await _mediator.Send(command);
		return Ok("Cliente deletado com sucesso");
	}

	[HttpPut("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login([FromBody] LoginClienteCommand command)
	{
		var loginClienteViewModel = await _mediator.Send(command);
		if (loginClienteViewModel == null) return BadRequest();
		return Ok(loginClienteViewModel);
	}
}