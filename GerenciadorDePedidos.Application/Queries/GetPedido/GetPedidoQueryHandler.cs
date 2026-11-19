using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetPedido;

public sealed class GetPedidoQueryHandler(
	IPedidoRepository pedidoRepository,
	ICurrentUserService currentUserService)
	: IRequestHandler<GetPedidoQuery, PedidoResponseDTO?>
{
	public async Task<PedidoResponseDTO?> Handle(GetPedidoQuery request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdDetailsAsync(request.Id, cancellationToken);
		if (pedido is null)
			return null;

		if (!currentUserService.IsAdmin && pedido.ClienteId != currentUserService.ClienteId)
			throw new UnauthorizedAccessException("Você não tem permissão para visualizar este pedido.");

		return pedido.ToResponseDto();
	}
}