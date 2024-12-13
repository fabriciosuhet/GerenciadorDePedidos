using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public class CreatePedidoCommandHandler : IRequestHandler<CreatePedidoCommand, Guid>
{
	private readonly IPedidoRepository _pedidoRepository;

	public CreatePedidoCommandHandler(IPedidoRepository pedidoRepository)
	{
		_pedidoRepository = pedidoRepository;
	}

	public async Task<Guid> Handle(CreatePedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = new Pedido();
		await _pedidoRepository.AddAsync(pedido);
		return pedido.Id;
	}
}