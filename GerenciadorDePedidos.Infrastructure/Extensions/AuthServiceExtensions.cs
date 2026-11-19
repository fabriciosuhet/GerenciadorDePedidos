using System.Text;
using GerenciadorDePedidos.Core.Services;
using GerenciadorDePedidos.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorDePedidos.Infrastructure.Extensions;

public static class AuthServiceExtensions
{
	public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer();

		services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
			.Configure<IConfiguration>((opt, cfg) =>
			{
				opt.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ClockSkew = TimeSpan.Zero,

					ValidIssuer = cfg["Jwt:Issuer"],
					ValidAudience = cfg["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(cfg["Jwt:Key"] ?? string.Empty))
				};
			});

		return services;
	}

	public static IServiceCollection AddAuthService(this IServiceCollection services)
	{
		services.AddScoped<IAuthService, AuthService>();
		return services;
	}
}