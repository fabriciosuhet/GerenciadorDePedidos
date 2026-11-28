using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public sealed class GetAllPedidosQueryHandler(
	IPedidoRepository pedidoRepository,
	ICurrentUserService currentUserService)
	: IRequestHandler<GetAllPedidosQuery, PagedResultModel<PedidoViewModel>>
{
	public async Task<PagedResultModel<PedidoViewModel>> Handle(GetAllPedidosQuery request, CancellationToken cancellationToken)
	{
		Guid? clienteIdFiltro = currentUserService.IsAdmin ? null : currentUserService.ClienteId;

		if (!currentUserService.IsAdmin && clienteIdFiltro is null)
			throw new UnauthorizedAccessException("Usuário não autenticado.");

		var totalCount = await pedidoRepository.GetCountAsync(request.Query, clienteIdFiltro, cancellationToken);
		if (totalCount == 0)
			return new PagedResultModel<PedidoViewModel>([], 0, request.PageNumber, request.PageSize);

		var pedidos = await pedidoRepository.GetPagedAsync(
			request.Query,
			(request.PageNumber - 1) * request.PageSize,
			request.PageSize,
			clienteIdFiltro,
			cancellationToken);

		var items = pedidos
			.Select(p => new PedidoViewModel(p.Id, p.DataPedido.ToBrasiliaTime(), p.Total, p.Status))
			.ToList();

		return new PagedResultModel<PedidoViewModel>(items, totalCount, request.PageNumber, request.PageSize);
	}
}