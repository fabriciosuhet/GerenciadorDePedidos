using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ProcessarPedido;

public sealed record ProcessarPedidoCommand(int PedidoId) : IRequest<bool>;
