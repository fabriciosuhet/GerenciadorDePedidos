using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetPedido;

public class GetPedidoQueryHandler : IRequestHandler<GetPedidoQuery, PedidoViewModel>
{
	private readonly IPedidoRepository _pedidoRepository;

	public GetPedidoQueryHandler(IPedidoRepository pedidoRepository)
	{
		_pedidoRepository = pedidoRepository;
	}

	public async Task<PedidoViewModel> Handle(GetPedidoQuery request, CancellationToken cancellationToken)
	{
		var pedido = await _pedidoRepository.GetByIdAsync(request.Id);
		if (pedido is null) return null;
		var pedidoViewModel = new PedidoViewModel(
			pedido.ItensPedidos,
			pedido.Total
		);
		return pedidoViewModel;

	}
}