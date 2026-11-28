using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DevolverPedido;

public sealed record DevolverPedidoCommand(
	int PedidoId,
	string Motivo,
	bool RetornarAoEstoque = true) : IRequest<DevolverPedidoViewModel>;

