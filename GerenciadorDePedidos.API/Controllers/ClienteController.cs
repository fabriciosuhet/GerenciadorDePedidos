using GerenciadorDePedidos.API.Authorization;
using GerenciadorDePedidos.API.Extensions;
using GerenciadorDePedidos.Application.Commands.CreateCliente;
using GerenciadorDePedidos.Application.Commands.DeleteCliente;
using GerenciadorDePedidos.Application.Commands.RegisterCliente;
using GerenciadorDePedidos.Application.Commands.UpdateCliente;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Application.Queries.GetAllClientes;
using GerenciadorDePedidos.Application.Queries.GetCliente;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/cliente")]
[AcessoPadrao]
public class ClienteController(
	IMediator mediator,
	ICacheService cacheService,
	ICurrentUserService currentUserService)
	: ApiControllerBase(mediator, cacheService)
{
	[HttpGet]
	[AcessoRestrito]
	public Task<IActionResult> GetAll(
		[FromQuery] string? query,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 10,
		CancellationToken cancellationToken = default)
	{
		var (pagina, tamanho) = NormalizarPaginacao(pageNumber, pageSize);
		return GetCachedOrNotFoundAsync<PagedResultModel<ClienteViewModel>>(
			CacheKeyHelper.GetAllClientesKey(query, pagina, tamanho),
			new GetAllClientesQuery(query, pagina, tamanho),
			"Clientes nao encontrados",
			cancellationToken);
	}

	[HttpGet("me")]
	public Task<IActionResult> GetMe(CancellationToken cancellationToken)
	{
		if (currentUserService.ClienteId is null)
			return Task.FromResult<IActionResult>(Unauthorized());

		return GetById(currentUserService.ClienteId.Value, cancellationToken);
	}

	[HttpGet("{id:guid}")]
	public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
	{
		if (!currentUserService.IsAdmin && currentUserService.ClienteId != id)
			return Task.FromResult<IActionResult>(Forbid());

		return GetCachedOrNotFoundAsync<ClienteDetailsViewModel>(
			CacheKeyHelper.GetClienteByIdKey(id),
			new GetClienteQuery(id),
			"Cliente nao encontrado",
			cancellationToken);
	}

	[HttpPost]
	[AcessoRestrito]
	public async Task<IActionResult> Post([FromBody] CreateClienteCommand command, CancellationToken cancellationToken)
	{
		var result = await SendAsync(command, cancellationToken);
		CacheService?.RemoveByPrefix(CacheKeyHelper.AllClientesPrefix);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { id = result.Id });
	}

	[HttpPost("registrar")]
	[AllowAnonymous]
	[EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
	public async Task<IActionResult> Registrar([FromBody] RegisterClienteCommand command, CancellationToken cancellationToken)
	{
		var result = await SendAsync(command, cancellationToken);
		CacheService?.RemoveByPrefix(CacheKeyHelper.AllClientesPrefix);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpPut("atualizar-cliente")]
	public async Task<IActionResult> Put([FromBody] UpdateClienteCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetClienteByIdKey(command.Id));
		CacheService?.RemoveByPrefix(CacheKeyHelper.AllClientesPrefix);
		return NoContent();
	}

	[HttpDelete("deletar-cliente")]
	[AcessoRestrito]
	public async Task<IActionResult> Delete([FromQuery] Guid id, CancellationToken cancellationToken)
	{
		await SendAsync(new DeleteClienteCommand(id), cancellationToken);
		CacheService?.Remove(CacheKeyHelper.GetClienteByIdKey(id));
		CacheService?.RemoveByPrefix(CacheKeyHelper.AllClientesPrefix);
		return Ok("Cliente deletado com sucesso");
	}
}