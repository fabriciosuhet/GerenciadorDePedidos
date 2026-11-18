using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;

public sealed class AdicionarProdutoEstoqueCommandHandler(
	IRepository<Produto, int> produtoRepository,
	IRepository<MovimentacaoEstoque, int> movimentacaoEstoqueRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<AdicionarProdutoEstoqueCommand, int>
{
	public async Task<int> Handle(AdicionarProdutoEstoqueCommand request, CancellationToken cancellationToken)
	{
		var produto = await produtoRepository.GetByIdAsync(request.ProdutoId, cancellationToken)
			?? throw new KeyNotFoundException($"O ID: {request.ProdutoId} de produto nao foi encontrado");

		produto.AdicionarEstoque(request.Quantidade);

		var movimentacao = new MovimentacaoEstoque(request.Quantidade, Tipo.Adicao, produto.Id, request.ClienteId);

		await movimentacaoEstoqueRepository.AddAsync(movimentacao, cancellationToken);
		produtoRepository.Update(produto);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return produto.Id;
	}
}