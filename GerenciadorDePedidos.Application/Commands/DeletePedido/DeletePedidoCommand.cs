using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeletePedido;

public class DeletePedidoCommand : IRequest<Unit>
{
	public Guid Id { get; private set; }

	public DeletePedidoCommand(Guid id)
	{
		Id = id;
	}
}