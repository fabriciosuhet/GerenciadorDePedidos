using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteCliente;

public class DeleteClienteCommand : IRequest<Unit>
{
	public Guid Id { get; set; }

	public DeleteClienteCommand(Guid id)
	{
		Id = id;
	}
}