using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeletePedido;

public class DeletePedidoCommand : IRequest<Unit>
{
	public int Id { get; private set; }

	public DeletePedidoCommand(int id)
	{
		Id = id;
	}
}