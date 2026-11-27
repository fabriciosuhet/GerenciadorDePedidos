using GerenciadorDePedidos.API.Extensions;
using GerenciadorDePedidos.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GerenciadorDePedidos.API.Controllers;

[ApiController]
[EnableRateLimiting(RateLimitingPolicies.ApiPadraoPolicy)]
public abstract class ApiControllerBase(IMediator mediator, ICacheService? cacheService = null) : ControllerBase
{
	private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(5);

	protected IMediator Mediator => mediator;
	protected ICacheService? CacheService => cacheService;

	protected static (int PageNumber, int PageSize) NormalizarPaginacao(int pageNumber, int pageSize)
		=> (Math.Clamp(pageNumber, 1, 100_000), Math.Clamp(pageSize, 1, 100));

	protected Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
		=> mediator.Send(request, cancellationToken);

	protected async Task<IActionResult> GetOrNotFoundAsync<TResponse>(
		IRequest<TResponse?> query,
		string notFoundMessage,
		CancellationToken cancellationToken)
	{
		var result = await mediator.Send(query, cancellationToken);
		return result is null ? NotFound(notFoundMessage) : Ok(result);
	}

	protected async Task<IActionResult> GetCachedOrNotFoundAsync<TResponse>(
		string cacheKey,
		IRequest<TResponse?> query,
		string notFoundMessage,
		CancellationToken cancellationToken,
		TimeSpan? expiration = null)
	{
		if (cacheService is not null && cacheService.TryGet<TResponse>(cacheKey, out var cached) && cached is not null)
			return Ok(cached);

		var result = await mediator.Send(query, cancellationToken);
		if (result is null)
			return NotFound(notFoundMessage);

		cacheService?.Set(cacheKey, result, expiration ?? DefaultCacheExpiration);
		return Ok(result);
	}
}
