using System.Reflection;
using FluentValidation;
using GerenciadorDePedidos.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		services.AddValidatorsFromAssembly(assembly);
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

		return services;
	}
}
