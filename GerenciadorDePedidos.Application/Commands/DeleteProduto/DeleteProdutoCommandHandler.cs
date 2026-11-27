using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteProduto;

public sealed class DeleteProdutoCommandHandler(IProdutoRepository produtoRepository)
	: IRequestHandler<DeleteProdutoCommand, Unit>
{
	public async Task<Unit> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto = await produtoRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("Produto nao encontrado");

		produto.Desativar();
		produtoRepository.Update(produto);
		await produtoRepository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}