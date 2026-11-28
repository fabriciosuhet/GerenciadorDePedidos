using GerenciadorDePedidos.API.Extensions;
using GerenciadorDePedidos.Application.Commands.ChangePassword;
using GerenciadorDePedidos.Application.Commands.ForgotPassword;
using GerenciadorDePedidos.Application.Commands.LoginCliente;
using GerenciadorDePedidos.Application.Commands.RefreshToken;
using GerenciadorDePedidos.Application.Commands.ResetPassword;
using GerenciadorDePedidos.Application.Commands.RevokeToken;
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

	[HttpPost("refresh")]
	[AllowAnonymous]
	[EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
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

	[HttpPost("esqueci-senha")]
	[AllowAnonymous]
	[EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
	public async Task<IActionResult> EsqueciSenha([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
	{
		var result = await SendAsync(command, cancellationToken);
		return Ok(result);
	}

	[HttpPost("redefinir-senha")]
	[AllowAnonymous]
	[EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
	public async Task<IActionResult> RedefinirSenha([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		return NoContent();
	}

	[HttpPost("alterar-senha")]
	[Authorize]
	[EnableRateLimiting(RateLimitingPolicies.AuthPolicy)]
	public async Task<IActionResult> AlterarSenha([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
	{
		await SendAsync(command, cancellationToken);
		return NoContent();
	}
}
