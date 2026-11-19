using GerenciadorDePedidos.API.Extensions;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/[controller]")]
public class LoginController(IMediator mediator) : ApiControllerBase(mediator)
{
	[HttpPost]
	[AllowAnonymous]
	[EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
	public async Task<IActionResult> Login([FromBody] LoginClienteCommand command, CancellationToken cancellationToken)
	{
		var loginResult = await SendAsync(command, cancellationToken);
		return loginResult is null ? NotFound("Email ou senha invalidos") : Ok(loginResult);
	}
}
