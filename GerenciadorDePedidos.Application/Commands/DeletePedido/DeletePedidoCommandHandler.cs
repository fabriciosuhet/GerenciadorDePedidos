using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeletePedido;

public sealed class DeletePedidoCommandHandler(
	IRepository<Pedido, int> pedidoRepository,
	ICacheService cacheService)
	: IRequestHandler<DeletePedidoCommand, Unit>
{
	public async Task<Unit> Handle(DeletePedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("Pedido nao encontrado");

		await pedidoRepository.DeleteAsync(pedido.Id, cancellationToken);
		await pedidoRepository.SaveChangesAsync(cancellationToken);

		cacheService.Remove(CacheKeyHelper.GetPedidoByIdKey(pedido.Id));
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosAdminPrefix());
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosClientePrefix(pedido.ClienteId));
		cacheService.Remove(CacheKeyHelper.GetClienteByIdKey(pedido.ClienteId));
		return Unit.Value;
	}
}