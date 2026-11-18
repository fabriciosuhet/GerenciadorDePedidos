using GerenciadorDePedidos.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.Infrastructure.Extensions;

public static class AuthorizationPolicies
{
	public const string AcessoPadrao = "AcessoPadrao";
	public const string AcessoRestrito = "AcessoRestrito";
}

public static class AuthorizationController
{
	public static IServiceCollection AddPolicyController(this IServiceCollection services)
	{
		services.AddAuthorization(opt =>
		{
			opt.AddPolicy(AuthorizationPolicies.AcessoPadrao, policy =>
				policy.RequireRole(nameof(Role.Admin), nameof(Role.Usuario)));

			opt.AddPolicy(AuthorizationPolicies.AcessoRestrito, policy =>
				policy.RequireRole(nameof(Role.Admin)));
		});

		return services;
	}
}
