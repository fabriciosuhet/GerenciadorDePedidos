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
				var jwtKey = cfg["Jwt:Key"];
				if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
					throw new InvalidOperationException("Jwt:Key deve ser configurada com pelo menos 32 bytes.");

				opt.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ClockSkew = TimeSpan.Zero,
					ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

					ValidIssuer = cfg["Jwt:Issuer"],
					ValidAudience = cfg["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
				};

				// Permite autenticação JWT via query string (?access_token=...) exclusivamente nas rotas do SignalR (/hubs)
				opt.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						var accessToken = context.Request.Query["access_token"];
						var path = context.HttpContext.Request.Path;

						if (!string.IsNullOrEmpty(accessToken)
							&& path.StartsWithSegments("/hubs")
							&& string.IsNullOrEmpty(context.Request.Headers.Authorization))
						{
							context.Token = accessToken;
						}

						return Task.CompletedTask;
					}
				};
			});

		return services;
	}

	public static IServiceCollection AddAuthService(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<ICurrentUserService, CurrentUserService>();
		services.AddScoped<ILoginAttemptService, LoginAttemptService>();
		return services;
	}
}