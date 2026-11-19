using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<LoginClienteViewModel>;