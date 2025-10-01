using GerenciadorDePedidos.API.Helpers;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Application.Queries.GetAllClientes;
using GerenciadorDePedidos.Application.Queries.GetCliente;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Services;
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
	private readonly ICacheService _cacheService;

	public ClienteController(IMediator mediator, ICacheService cacheService)
	{
		_mediator = mediator;
		_cacheService = cacheService;
	}

	[HttpGet]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetAll(string? query)
	{
		var cacheKey = CacheKeyHelper.GetAllClientesKey(query);

		if (!_cacheService.TryGet<PagedResultModel<ClienteViewModel>>(cacheKey, out var clientes))
		{
			var getAllClientes = new GetAllClientesQuery(query);
			clientes = await _mediator.Send(getAllClientes);

			if (clientes is null) return NotFound("Clientes nao encontrados");
			
			_cacheService.Set(cacheKey, clientes, TimeSpan.FromMinutes(5));
		}
		return Ok(clientes);
		
	}

	[HttpGet("{id:guid}")]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var cacheKey = CacheKeyHelper.GetClienteByIdKey(id);

		if (!_cacheService.TryGet<ClienteDetailsViewModel>(cacheKey, out var cliente))
		{
			var getClienteById = new GetClienteQuery(id);
			cliente = await _mediator.Send(getClienteById);
			if (cliente is null) return NotFound("Cliente nao encontrado");
			
			_cacheService.Set(cacheKey, cliente, TimeSpan.FromMinutes(5));
		}
		
		return Ok(cliente);
	}

	[HttpPost]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	// Cadastro de Cliente
	public async Task<IActionResult> Post([FromBody] CreateClienteCommand command)
	{
		var clienteId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new { id = clienteId}, command);
	}
	
	[HttpPost("cadastro-inicial")]
	[AllowAnonymous]
	// Cadastro de Cliente
	public async Task<IActionResult> PostInicial([FromBody] CreateClienteCommand command)
	{
		var clienteId = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new { id = clienteId}, command);
	}
	

	[HttpPut("atualizar-cliente")]
	[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Usuario)}")]
	public async Task<IActionResult> Put(Guid id, [FromBody] UpdateClienteCommand command)
	{
		await _mediator.Send(command);
		return NoContent();
	}

	[HttpDelete("deletar-cliente/{id:guid}")]
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