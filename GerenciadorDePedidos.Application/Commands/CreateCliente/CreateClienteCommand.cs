using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Enums;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateCliente;

public sealed record CreateClienteCommand(
	string NomeCompleto,
	Role Role,
	string Senha,
	string Email,
	string Telefone) : IRequest<CreateClienteViewModel>;