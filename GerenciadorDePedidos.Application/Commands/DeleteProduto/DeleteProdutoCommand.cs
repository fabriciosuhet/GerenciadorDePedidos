using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteProduto;

public class DeleteProdutoCommand : IRequest<Unit>
{
	public int Id { get; private set; }

	public DeleteProdutoCommand(int id)
	{
		Id = id;
	}
}