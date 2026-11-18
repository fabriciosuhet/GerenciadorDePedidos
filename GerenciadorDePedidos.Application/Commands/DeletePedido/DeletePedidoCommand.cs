using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeletePedido;

public sealed record DeletePedidoCommand(int Id) : IRequest<Unit>;