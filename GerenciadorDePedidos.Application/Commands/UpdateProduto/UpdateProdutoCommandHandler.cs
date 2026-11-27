using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateProduto;

public sealed class UpdateProdutoCommandHandler(IRepository<Produto, int> produtoRepository)
	: IRequestHandler<UpdateProdutoCommand, Unit>
{
	public async Task<Unit> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto = await produtoRepository.GetByIdAsync(request.Id, cancellationToken)
			?? throw new KeyNotFoundException("Produto nao encontrado");

		produto.AlterarDados(request.Nome, request.Preco, request.Estoque, request.EstoqueMinimo);
		produtoRepository.Update(produto);
		await produtoRepository.SaveChangesAsync(cancellationToken);
		return Unit.Value;
	}
}