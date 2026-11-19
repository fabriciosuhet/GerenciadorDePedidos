using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.RefreshToken;
using GerenciadorDePedidos.Application.Commands.RevokeToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorDePedidos.API.Controllers;

[Route("api/[controller]")]
public class LoginController(IMediator mediator) : ApiControllerBase(mediator)
{
	[HttpPost]
	[AllowAnonymous]
	public async Task<IActionResult> Login([FromBody] LoginClienteCommand command, CancellationToken cancellationToken)
	{
		var loginResult = await SendAsync(command, cancellationToken);
		return loginResult is null ? NotFound("Email ou senha invalidos") : Ok(loginResult);
	}

	[HttpPost("refresh")]
	[AllowAnonymous]
	public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
	{
		var result = await SendAsync(command, cancellationToken);
		return Ok(result);
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout([FromBody] RevokeTokenCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		return NoContent();
	}
}
