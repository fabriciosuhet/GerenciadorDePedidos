using GerenciadorDePedidos.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace GerenciadorDePedidos.API.Authorization;

/// <summary>
/// Permite acesso para usuários autenticados com perfil Admin ou Usuario.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AcessoPadraoAttribute : AuthorizeAttribute
{
	public AcessoPadraoAttribute() => Policy = AuthorizationPolicies.AcessoPadrao;
}

/// <summary>
/// Restringe o acesso exclusivamente para usuários com perfil Admin.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AcessoRestritoAttribute : AuthorizeAttribute
{
	public AcessoRestritoAttribute() => Policy = AuthorizationPolicies.AcessoRestrito;
}
