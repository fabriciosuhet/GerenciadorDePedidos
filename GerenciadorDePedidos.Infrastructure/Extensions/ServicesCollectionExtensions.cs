using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Infrastructure.Persistence;
using GerenciadorDePedidos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace GerenciadorDePedidos.Infrastructure.Extensions;

public static class ServicesCollectionExtensions 
{
	public static IServiceCollection AddRepositories(this IServiceCollection services)
	{
		services.AddScoped<IClienteRepository, ClienteRepository>();
		services.AddScoped<IProdutoRepository, ProdutoRepository>();
		services.AddScoped<IPedidoRepository, PedidoRepository>();
		return services;
	}

	public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection");
		services.AddDbContext<GerenciadorDePedidosDbContext>(opt =>
			opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
		return services;
	}

	public static IServiceCollection AddSwaggerService(this IServiceCollection services)
	{
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo {Title = "GerenciadorDePedidos.API", Version =  "v1"});
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "JWT Authorization header using the Bearer scheme."
			});
	
			c.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					[]
				}
			});
		});
		return services;
	}
	
	
}