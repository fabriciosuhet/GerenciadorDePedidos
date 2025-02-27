using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace GerenciadorDePedidos.Infrastructure.Extensions;

public static class AddValidatorExtensions
{
	public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
	{
		services.AddFluentValidationAutoValidation();
		return services;
	}
	
}