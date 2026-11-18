using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateProduto;

public sealed class CreateProdutoCommandHandler(IRepository<Produto, int> produtoRepository)
	: IRequestHandler<CreateProdutoCommand, int>
{
	public async Task<int> Handle(CreateProdutoCommand request, CancellationToken cancellationToken)
	{
		var produto = new Produto(request.Nome, request.Preco, request.Estoque);
		await produtoRepository.AddAsync(produto, cancellationToken);
		await produtoRepository.SaveChangesAsync(cancellationToken);
		return produto.Id;
	}
}