using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeletePedido;

public class DeletePedidoCommandHandler : IRequestHandler<DeletePedidoCommand, Unit>
{
	private readonly IRepository<Pedido, int> _pedidoRepository;

    public DeletePedidoCommandHandler(IRepository<Pedido, int> pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<Unit> Handle(DeletePedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await _pedidoRepository.GetByIdAsync(request.Id);
		if (pedido is null)
			throw new KeyNotFoundException("Pedido nao encontrado");
		
		await _pedidoRepository.DeleteAsync(pedido.Id);
		return Unit.Value;
	}
}