using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllProdutos;

public class GetAllProdutosQueryHandler : IRequestHandler<GetAllProdutosQuery, List<ProdutoViewModel>>
{
	private readonly IProdutoRepository _produtoRepository;

	public GetAllProdutosQueryHandler(IProdutoRepository produtoRepository)
	{
		_produtoRepository = produtoRepository;
	}

	public async Task<List<ProdutoViewModel>> Handle(GetAllProdutosQuery request, CancellationToken cancellationToken)
	{
		var produto = await _produtoRepository.GetAllAsync(request.Query);
		var produtoViewModel = produto
			.Select(p => new ProdutoViewModel(p.Id, p.Nome, p.Preco, p.Estoque))
			.ToList();
		return produtoViewModel;
	}
}