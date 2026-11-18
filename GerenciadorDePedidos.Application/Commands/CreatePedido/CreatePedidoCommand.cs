using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public sealed record CreatePedidoCommand(
	List<ItemPedidoDTO> ItensPedidos,
	Guid ClienteId,
	Status Status) : IRequest<int>;