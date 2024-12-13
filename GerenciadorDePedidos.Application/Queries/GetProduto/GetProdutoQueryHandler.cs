using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetProduto;

public class GetProdutoQueryHandler : IRequestHandler<GetProdutoQuery, ProdutoDetailsViewModel>
{
	private readonly IProdutoRepository _produtoRepository;

	public GetProdutoQueryHandler(IProdutoRepository produtoRepository) 
		=> _produtoRepository = produtoRepository;
	

	public async Task<ProdutoDetailsViewModel> Handle(GetProdutoQuery request, CancellationToken cancellationToken)
	{
		var produto = await _produtoRepository.GetByIdAsync(request.Id);
		if (produto is null) return null;
		var produtoDetailsViewModel = new ProdutoDetailsViewModel(
			produto.Nome,
			produto.Preco,
			produto.Estoque
		);
		return produtoDetailsViewModel;
	}
}