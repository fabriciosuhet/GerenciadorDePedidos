using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetPedido;

public sealed record GetPedidoQuery(int Id) : IRequest<PedidoResponseDTO?>;