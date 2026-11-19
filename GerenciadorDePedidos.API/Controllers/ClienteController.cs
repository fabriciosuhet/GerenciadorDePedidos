using GerenciadorDePedidos.API.Authorization;
using GerenciadorDePedidos.API.Extensions;
using GerenciadorDePedidos.API.Helpers;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Application.Queries.GetAllClientes;
using GerenciadorDePedidos.Application.Queries.GetCliente;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/cliente")]
[AcessoPadrao]
public class ClienteController(IMediator mediator, ICacheService cacheService)
	: ApiControllerBase(mediator, cacheService)
{
	[HttpGet]
	public Task<IActionResult> GetAll([FromQuery] string? query, CancellationToken cancellationToken)
		=> GetCachedOrNotFoundAsync<PagedResultModel<ClienteViewModel>>(
			CacheKeyHelper.GetAllClientesKey(query),
			new GetAllClientesQuery(query),
			"Clientes nao encontrados",
			cancellationToken);

	[HttpGet("{id:guid}")]
	public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
		=> GetCachedOrNotFoundAsync<ClienteDetailsViewModel>(
			CacheKeyHelper.GetClienteByIdKey(id),
			new GetClienteQuery(id),
			"Cliente nao encontrado",
			cancellationToken);

	[HttpPost]
	[AcessoRestrito]
	public async Task<IActionResult> Post([FromBody] CreateClienteCommand command, CancellationToken cancellationToken)
	{
		var result = await SendAsync(command, cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetAllClientesKey(null));
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { id = result.Id });
	}

	[HttpPost("registrar")]
	[AllowAnonymous]
	[EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
	public async Task<IActionResult> Registrar([FromBody] RegisterClienteCommand command, CancellationToken cancellationToken)
	{
		var result = await SendAsync(command, cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetAllClientesKey(null));
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpPut("atualizar-cliente")]
	public async Task<IActionResult> Put([FromBody] UpdateClienteCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetClienteByIdKey(command.Id));
		CacheService?.Remove(CacheKeyHelper.GetAllClientesKey(null));
		return NoContent();
	}

	[HttpDelete("deletar-cliente")]
	[AcessoRestrito]
	public async Task<IActionResult> Delete([FromQuery] Guid id, CancellationToken cancellationToken)
	{
		await SendAsync(new DeleteClienteCommand(id), cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetClienteByIdKey(id));
		CacheService?.Remove(CacheKeyHelper.GetAllClientesKey(null));
		return Ok("Cliente deletado com sucesso");
	}
}