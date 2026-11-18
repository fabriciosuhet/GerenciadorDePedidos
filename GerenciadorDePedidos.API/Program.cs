using GerenciadorDePedidos.API.Filters;
using GerenciadorDePedidos.API.Middleware;
using GerenciadorDePedidos.Application;
using GerenciadorDePedidos.Infrastructure.Extensions;
using GerenciadorDePedidos.Infrastructure.Persistence;
using GerenciadorDePedidos.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
	.AddCacheService()
	.AddPolicyController();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

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