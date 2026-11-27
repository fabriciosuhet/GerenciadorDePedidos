using System.Reflection;
using GerenciadorDePedidos.Core.Services;
using GerenciadorDePedidos.Infrastructure.Messaging;
using GerenciadorDePedidos.Infrastructure.Observability;
using GerenciadorDePedidos.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
		services.AddSingleton<ICorrelationContext, CorrelationContext>();
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
					cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);
					cfg.UseMessageRetry(r =>
					{
						r.Handle<DbUpdateConcurrencyException>();
						r.Interval(3, TimeSpan.FromMilliseconds(200));
					});
					cfg.ConfigureEndpoints(context);
				});
			}
			else
			{
				x.AddEntityFrameworkOutbox<GerenciadorDePedidosDbContext>(o =>
				{
					o.UseMySql();
					o.UseBusOutbox();
				});

				x.AddConfigureEndpointsCallback((context, _, endpointConfigurator) =>
				{
					endpointConfigurator.UseEntityFrameworkOutbox<GerenciadorDePedidosDbContext>(context);
				});

				x.UsingRabbitMq((context, cfg) =>
				{
					var cfgRoot = context.GetRequiredService<IConfiguration>();
					var host = cfgRoot["RabbitMQ:Host"] ?? "localhost";
					var virtualHost = cfgRoot["RabbitMQ:VirtualHost"] ?? "/";
					var username = cfgRoot["RabbitMQ:Username"];
					var password = cfgRoot["RabbitMQ:Password"];

					if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
						throw new InvalidOperationException("RabbitMQ:Username e RabbitMQ:Password devem ser configurados.");

					cfg.Host(host, virtualHost, h =>
					{
						h.Username(username);
						h.Password(password);
					});

					cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);
					cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(1)));
					cfg.ConfigureEndpoints(context);
				});
			}
		});

		return services;
	}
}
