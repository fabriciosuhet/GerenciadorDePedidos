using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateCliente;

public sealed record UpdateClienteCommand(Guid Id, string Email, string Telefone) : IRequest<Unit>;