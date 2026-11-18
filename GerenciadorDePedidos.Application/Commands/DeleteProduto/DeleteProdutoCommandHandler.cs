using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteProduto;

public sealed class DeleteProdutoCommandHandler(IRepository<Produto, int> produtoRepository)
	: IRequestHandler<DeleteProdutoCommand, Unit>
{
	public async Task<Unit> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto = await produtoRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("Produto nao encontrado");

		await produtoRepository.DeleteAsync(produto.Id, cancellationToken);
		await produtoRepository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}