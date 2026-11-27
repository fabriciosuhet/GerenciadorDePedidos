using GerenciadorDePedidos.API.Consumers;
using GerenciadorDePedidos.API.Extensions;
using GerenciadorDePedidos.API.Filters;
using GerenciadorDePedidos.API.Middleware;
using GerenciadorDePedidos.Application;
using GerenciadorDePedidos.Infrastructure.Extensions;
using GerenciadorDePedidos.Infrastructure.Persistence;
using GerenciadorDePedidos.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability();

builder.Services.AddControllers(opt => opt.Filters.Add<ValidationFilter>());
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
	.AddApplication()
	.AddDatabase(builder.Configuration)
	.AddRepositories()
	.AddAuthService()
	.AddJwtAuthentication(builder.Configuration)
	.AddCorsService()
	.AddSwaggerService()
	.AddCacheService(builder.Configuration)
	.AddReportServices()
	.AddPolicyController()
	.AddRateLimitingService(builder.Configuration)
	.AddMessaging(builder.Configuration, typeof(PedidoCriadoConsumer).Assembly)
	.AddAppHealthChecks(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseObservabilityRequestLogging();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapAppHealthChecks();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();
	if (db.Database.IsRelational())
		db.Database.Migrate();

	await AdminSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();

public partial class Program;