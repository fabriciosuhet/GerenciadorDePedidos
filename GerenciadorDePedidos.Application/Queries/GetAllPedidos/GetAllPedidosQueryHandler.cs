using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public class GetAllPedidosQueryHandler : IRequestHandler<GetAllPedidosQuery, List<PedidoViewModel>>
{
	private readonly IPedidoRepository _pedidoRepository;

	public GetAllPedidosQueryHandler(IPedidoRepository pedidoRepository)
	{
		_pedidoRepository = pedidoRepository;
	}


	public async Task<List<PedidoViewModel>> Handle(GetAllPedidosQuery request, CancellationToken cancellationToken)
	{
		var pedido = await _pedidoRepository.GetAllAsync(request.Query);
		var pedidoViewModel = pedido
			.Select(p => new PedidoViewModel (p.ItensPedidos, p.Total))
			.ToList();
		return pedidoViewModel;
	}
}