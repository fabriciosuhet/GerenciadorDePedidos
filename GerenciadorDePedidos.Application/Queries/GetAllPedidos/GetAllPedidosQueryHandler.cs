using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public sealed class GetAllPedidosQueryHandler(IPedidoRepository pedidoRepository)
	: IRequestHandler<GetAllPedidosQuery, PagedResultModel<PedidoViewModel>>
{
	public async Task<PagedResultModel<PedidoViewModel>> Handle(GetAllPedidosQuery request, CancellationToken cancellationToken)
	{
		var totalCount = await pedidoRepository.GetCountAsync(request.Query, cancellationToken);
		if (totalCount == 0)
			return new PagedResultModel<PedidoViewModel>([], 0, request.PageNumber, request.PageSize);

		var pedidos = await pedidoRepository.GetPagedAsync(
			request.Query,
			(request.PageNumber - 1) * request.PageSize,
			request.PageSize,
			cancellationToken);

		var items = pedidos
			.Select(p => new PedidoViewModel(p.Id, p.DataPedido.ToBrasiliaTime(), p.Total))
			.ToList();

		return new PagedResultModel<PedidoViewModel>(items, totalCount, request.PageNumber, request.PageSize);
	}
}