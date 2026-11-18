using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetProduto;

public sealed class GetProdutoQueryHandler(IRepository<Produto, int> produtoRepository)
	: IRequestHandler<GetProdutoQuery, ProdutoDetailsViewModel?>
{
	public async Task<ProdutoDetailsViewModel?> Handle(GetProdutoQuery request, CancellationToken cancellationToken)
	{
		var produto = await produtoRepository.GetByIdAsync(request.Id, cancellationToken);
		return produto is null
			? null
			: new ProdutoDetailsViewModel(produto.Nome, produto.Preco, produto.Estoque);
	}
}