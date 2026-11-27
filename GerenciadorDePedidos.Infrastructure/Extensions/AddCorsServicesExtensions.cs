using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.Infrastructure.Extensions;

public static class CorsPolicies
{
	public const string Default = "DefaultCorsPolicy";
}

public static class AddCorsServicesExtensions
{
	public static IServiceCollection AddCorsService(this IServiceCollection services, IConfiguration configuration)
	{
		var allowedOrigins = configuration
			.GetSection("Cors:AllowedOrigins")
			.Get<string[]>()?
			.Where(o => !string.IsNullOrWhiteSpace(o))
			.Select(o => o.Trim())
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToArray() ?? [];

		services.AddCors(opt =>
		{
			opt.AddPolicy(CorsPolicies.Default, builder =>
			{
				if (allowedOrigins.Length > 0)
				{
					builder.WithOrigins(allowedOrigins)
						.AllowAnyMethod()
						.AllowAnyHeader();
				}
			});
		});

		return services;
	}
}