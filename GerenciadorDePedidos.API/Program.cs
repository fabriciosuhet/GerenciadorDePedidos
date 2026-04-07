using System.Text.Json.Serialization;
using FluentValidation;
using GerenciadorDePedidos.API.Filters;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Validators;
using GerenciadorDePedidos.Infrastructure.Extensions;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddValidatorsFromAssemblyContaining<CreateClineteCommandValidator>();
builder.Services.AddControllers(opt => opt.Filters.Add(typeof(ValidationFilter)));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDatabase(builder.Configuration)
	.AddRepositories()
	.AddAuthService()
	.AddJwtAuthentication(builder.Configuration)
	.AddCorsService()
	.AddSwaggerService()
	.AddFluentValidationServices()
	.AddCacheService();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProdutoCommand).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GerenciadorDePedidosDbContext>();
    db.Database.Migrate();
}

app.Run();