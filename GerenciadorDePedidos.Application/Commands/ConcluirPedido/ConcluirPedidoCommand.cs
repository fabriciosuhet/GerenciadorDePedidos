using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ConcluirPedido;

public sealed record ConcluirPedidoCommand(int PedidoId) : IRequest<ConcluirPedidoViewModel>;

