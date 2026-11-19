using System.Security.Claims;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Services;
using Microsoft.AspNetCore.Http;

namespace GerenciadorDePedidos.Infrastructure.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
	public Guid? ClienteId
	{
		get
		{
			var rawId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? httpContextAccessor.HttpContext?.User.FindFirstValue("clienteId");

			return Guid.TryParse(rawId, out var id) ? id : null;
		}
	}

	public bool IsAdmin
		=> httpContextAccessor.HttpContext?.User.IsInRole(nameof(Role.Admin)) ?? false;
}
