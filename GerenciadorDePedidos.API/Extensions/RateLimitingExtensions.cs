using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace GerenciadorDePedidos.API.Extensions;

public static class RateLimitingPolicies
{
	public const string AuthPolicy = "AuthPolicy";
	public const string ApiPadraoPolicy = "ApiPadraoPolicy";
}

public static class RateLimitingExtensions
{
	public static IServiceCollection AddRateLimitingService(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var authPermitLimit = configuration.GetValue("RateLimiting:AuthPermitLimit", 10);
		var apiPermitLimit = configuration.GetValue("RateLimiting:ApiPermitLimit", 100);
		var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);
		var window = TimeSpan.FromSeconds(windowSeconds);

		services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			options.OnRejected = async (context, cancellationToken) =>
			{
				context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
				await context.HttpContext.Response.WriteAsJsonAsync(
					new { error = "Muitas requisições. Tente novamente em instantes." },
					cancellationToken);
			};

			// Política estrita por IP para rotas públicas de login e auto-registro (proteção contra brute force)
			options.AddPolicy(RateLimitingPolicies.AuthPolicy, httpContext =>
			{
				var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

				return RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: $"auth:{ip}",
					factory: _ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = authPermitLimit,
						Window = window,
						QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
						QueueLimit = 0,
						AutoReplenishment = true
					});
			});

			// Política padrão para os demais endpoints da API (por usuário autenticado ou IP)
			options.AddPolicy(RateLimitingPolicies.ApiPadraoPolicy, httpContext =>
			{
				var userOrIp = httpContext.User.FindFirst("userName")?.Value
					?? httpContext.Connection.RemoteIpAddress?.ToString()
					?? "unknown";

				return RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: $"api:{userOrIp}",
					factory: _ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = apiPermitLimit,
						Window = window,
						QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
						QueueLimit = 0,
						AutoReplenishment = true
					});
			});
		});

		return services;
	}
}
