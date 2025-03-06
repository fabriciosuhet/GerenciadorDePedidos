using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public class GetAllPedidosQueryHandler : IRequestHandler<GetAllPedidosQuery, PagedResultModel<PedidoViewModel>>
{
	private readonly IPedidoRepository _pedidoRepository;

	public GetAllPedidosQueryHandler(IPedidoRepository pedidoRepository)
	{
		_pedidoRepository = pedidoRepository;
	}


	public async Task<PagedResultModel<PedidoViewModel>> Handle(GetAllPedidosQuery request, CancellationToken cancellationToken)
	{
		var totalCount = await _pedidoRepository.GetCountAsync(request.Query);
		var pedidos = await _pedidoRepository.GetPagedAsync(request.Query, (request.PageNumber - 1) * request.PageSize,
			request.PageSize);


		var pedidoViewModel = pedidos.Select(p => new PedidoViewModel(p.ItensPedidos, p.Total)).ToList();
		
		return new PagedResultModel<PedidoViewModel>(pedidoViewModel, totalCount, request.PageNumber, request.PageSize);
	}
}