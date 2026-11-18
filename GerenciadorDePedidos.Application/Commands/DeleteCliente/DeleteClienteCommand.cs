using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteCliente;

public sealed record DeleteClienteCommand(Guid Id) : IRequest<Unit>;