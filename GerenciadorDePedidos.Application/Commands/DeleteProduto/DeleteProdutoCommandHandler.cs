using GerenciadorDePedidos.Core.Exceptions;
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

		if (await produtoRepository.HasPedidoVinculadoAsync(produto.Id, cancellationToken))
			throw new ConflictException("Não é possível excluir este produto porque ele está vinculado a pedidos.");

		await produtoRepository.DeleteAsync(produto.Id, cancellationToken);
		await produtoRepository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}