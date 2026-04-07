using GerenciadorDePedidos.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.Infrastructure.Extensions
{
    public static class AuthorizationController
    {
        public static IServiceCollection AddPolicyController(this IServiceCollection services)
        {
            services.AddAuthorization(opt => opt.AddPolicy("AcessoPadrao", policy =>
                policy.RequireRole(nameof(Role.Admin), nameof(Role.Usuario))));

            services.AddAuthorization(opt => opt.AddPolicy("AcessoRestrito", policy =>
                policy.RequireRole(nameof(Role.Admin))));

            return services;
        }
    }
}
