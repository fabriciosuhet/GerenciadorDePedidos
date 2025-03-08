using System.Net;
using System.Text.Json.Serialization;
using FluentValidation;
using GerenciadorDePedidos.API.Filters;
using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Application.Validators;
using GerenciadorDePedidos.Infrastructure.Extensions;
using GerenciadorDePedidos.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddValidatorsFromAssemblyContaining<CreateClineteCommandValidator>();
builder.Services.AddControllers(opt => opt.Filters.Add(typeof(ValidationFilter)))
	.AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

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

app.Run();