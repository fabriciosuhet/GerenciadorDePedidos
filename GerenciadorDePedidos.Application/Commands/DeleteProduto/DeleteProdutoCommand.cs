using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteProduto;

public class DeleteProdutoCommand : IRequest<Unit>
{
	public Guid Id { get; private set; }

	public DeleteProdutoCommand(Guid id)
	{
		Id = id;
	}
}