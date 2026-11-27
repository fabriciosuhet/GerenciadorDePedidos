using GerenciadorDePedidos.API.Authorization;
using GerenciadorDePedidos.Application.Queries.GetDashboard;
using GerenciadorDePedidos.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/dashboard")]
[AcessoRestrito]
public class DashboardController(IMediator mediator) : ApiControllerBase(mediator)
{
	[HttpGet]
	public async Task<IActionResult> GetResumo(
		[FromQuery] PeriodoRelatorio? periodo = null,
		[FromQuery] DateTime? dataReferencia = null,
		[FromQuery] int topProdutos = 5,
		CancellationToken cancellationToken = default)
		=> Ok(await SendAsync(new GetDashboardQuery(periodo, dataReferencia, topProdutos), cancellationToken));
}
