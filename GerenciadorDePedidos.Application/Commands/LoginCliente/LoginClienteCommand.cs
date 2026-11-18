using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.LoginCliente;

public sealed record LoginClienteCommand(string Email, string Password) : IRequest<LoginClienteViewModel?>;