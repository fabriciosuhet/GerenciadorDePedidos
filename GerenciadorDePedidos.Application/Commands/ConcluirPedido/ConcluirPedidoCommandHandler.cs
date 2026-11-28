using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ConcluirPedido;

public sealed class ConcluirPedidoCommandHandler(
	IPedidoRepository pedidoRepository,
	IUnitOfWork unitOfWork,
	ICacheService cacheService,
	IPedidoRealtimeNotifier realtimeNotifier) : IRequestHandler<ConcluirPedidoCommand, ConcluirPedidoViewModel>
{
	public async Task<ConcluirPedidoViewModel> Handle(ConcluirPedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdWithItensAsync(request.PedidoId, cancellationToken)
			?? throw new KeyNotFoundException("Pedido nao encontrado");

		var statusAnterior = pedido.Status;
		pedido.Concluir();

		await unitOfWork.SaveChangesAsync(cancellationToken);
		AppDiagnostics.PedidosConcluidos.Add(1);

		cacheService.Remove(CacheKeyHelper.GetPedidoByIdKey(pedido.Id));
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosAdminPrefix());
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosClientePrefix(pedido.ClienteId));
		cacheService.Remove(CacheKeyHelper.GetClienteByIdKey(pedido.ClienteId));

		var concluidoEmBrasilia = pedido.ConcluidoEm!.Value.ToBrasiliaTime();
		await realtimeNotifier.NotificarStatusAtualizadoAsync(
			new PedidoStatusAtualizadoDTO(
				pedido.Id,
				pedido.ClienteId,
				statusAnterior,
				pedido.Status,
				pedido.Total,
				concluidoEmBrasilia),
			cancellationToken);

		return new ConcluirPedidoViewModel(
			pedido.Id,
			pedido.Status,
			concluidoEmBrasilia);
	}
}

