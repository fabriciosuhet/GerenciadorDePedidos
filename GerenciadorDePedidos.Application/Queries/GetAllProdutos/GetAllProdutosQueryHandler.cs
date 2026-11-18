using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllProdutos;

public sealed class GetAllProdutosQueryHandler(IProdutoRepository produtoRepository)
	: IRequestHandler<GetAllProdutosQuery, PagedResultModel<ProdutoViewModel>>
{
	public async Task<PagedResultModel<ProdutoViewModel>> Handle(GetAllProdutosQuery request, CancellationToken cancellationToken)
	{
		var totalCount = await produtoRepository.GetCountAsync(request.Query, cancellationToken);
		if (totalCount == 0)
			return new PagedResultModel<ProdutoViewModel>([], 0, request.PageNumber, request.PageSize);

		var produtos = await produtoRepository.GetPagedAsync(
			request.Query,
			(request.PageNumber - 1) * request.PageSize,
			request.PageSize,
			cancellationToken);

		var items = produtos
			.Select(p => new ProdutoViewModel(p.Id, p.Nome, p.Preco, p.Estoque))
			.ToList();

		return new PagedResultModel<ProdutoViewModel>(items, totalCount, request.PageNumber, request.PageSize);
	}
}