using GerenciadorDePedidos.API.Authorization;
using GerenciadorDePedidos.Application.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/dashboard")]
[AcessoRestrito]
public class DashboardController(IMediator mediator) : ApiControllerBase(mediator)
{
	[HttpGet]
	public async Task<IActionResult> GetResumo(CancellationToken cancellationToken)
		=> Ok(await SendAsync(new GetDashboardQuery(), cancellationToken));
}
