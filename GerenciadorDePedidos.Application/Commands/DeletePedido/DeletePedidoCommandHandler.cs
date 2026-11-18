using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeletePedido;

public sealed class DeletePedidoCommandHandler(IRepository<Pedido, int> pedidoRepository)
	: IRequestHandler<DeletePedidoCommand, Unit>
{
	public async Task<Unit> Handle(DeletePedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("Pedido nao encontrado");

		await pedidoRepository.DeleteAsync(pedido.Id, cancellationToken);
		await pedidoRepository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}