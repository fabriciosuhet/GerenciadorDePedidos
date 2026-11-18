using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetPedido;

public sealed class GetPedidoQueryHandler(IPedidoRepository pedidoRepository)
	: IRequestHandler<GetPedidoQuery, PedidoResponseDTO?>
{
	public async Task<PedidoResponseDTO?> Handle(GetPedidoQuery request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdDetailsAsync(request.Id, cancellationToken);
		return pedido?.ToResponseDto();
	}
}