using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.Infrastructure.Extensions;

public static class AddCorsServicesExtensions
{
	public static IServiceCollection AddCorsService(this IServiceCollection services)
	{
		services.AddCors(opt =>
		{
			opt.AddPolicy("AllowAll", builder =>
			{
				builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader();
			});
		});
		
		return services;
	}
}