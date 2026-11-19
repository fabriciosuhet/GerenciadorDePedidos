using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RevokeToken;

public sealed record RevokeTokenCommand(string RefreshToken) : IRequest<Unit>;