using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CancelarPedido;

public sealed record CancelarPedidoCommand(int PedidoId) : IRequest<CancelarPedidoViewModel>;

