using System.Reflection;
using GerenciadorDePedidos.Core.Services;
using GerenciadorDePedidos.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.Infrastructure.Extensions;

public static class MessagingExtensions
{
	public static IServiceCollection AddMessaging(
		this IServiceCollection services,
		IConfiguration configuration,
		Assembly? consumersAssembly = null)
	{
		services.AddScoped<IEventBus, MassTransitEventBus>();

		var useInMemory = configuration.GetValue("RabbitMQ:UseInMemory", false);

		services.AddMassTransit(x =>
		{
			if (consumersAssembly is not null)
				x.AddConsumers(consumersAssembly);

			if (useInMemory)
			{
				x.UsingInMemory((context, cfg) =>
				{
					cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromMilliseconds(200)));
					cfg.ConfigureEndpoints(context);
				});
			}
			else
			{
				var host = configuration["RabbitMQ:Host"] ?? "localhost";
				var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
				var username = configuration["RabbitMQ:Username"] ?? "guest";
				var password = configuration["RabbitMQ:Password"] ?? "guest";

				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host(host, virtualHost, h =>
					{
						h.Username(username);
						h.Password(password);
					});

					cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
					cfg.ConfigureEndpoints(context);
				});
			}
		});

		return services;
	}
}
