using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllPedidos;

public class GetAllPedidosQueryHandler : IRequestHandler<GetAllPedidosQuery, PagedResultModel<PedidoViewModel>>
{
	private readonly IRepository<Pedido, int> _pedidoRepository;

    public GetAllPedidosQueryHandler(IRepository<Pedido, int> pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<PagedResultModel<PedidoViewModel>> Handle(GetAllPedidosQuery request, CancellationToken cancellationToken)
	{
		var totalCount = await _pedidoRepository.GetCountAsync();
		var pedidos = await _pedidoRepository.GetPagedAsync((request.PageNumber - 1) * request.PageSize,
			request.PageSize);


		var pedidoViewModel = pedidos.Select(p => new PedidoViewModel(
			p.Id,
			p.Total
		)).ToList();
		
		return new PagedResultModel<PedidoViewModel>(pedidoViewModel, totalCount, request.PageNumber, request.PageSize);
	}
}