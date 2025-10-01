using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllProdutos;

public class GetAllProdutosQueryHandler : IRequestHandler<GetAllProdutosQuery, PagedResultModel<ProdutoViewModel>>
{
	private readonly IRepository<Produto, int> _produtoRepository;
	private readonly IProdutoRepository _repository;

    public GetAllProdutosQueryHandler(IRepository<Produto, int> produtoRepository, IProdutoRepository repository)
    {
        _produtoRepository = produtoRepository;
        _repository = repository;
    }

    public async Task<PagedResultModel<ProdutoViewModel>> Handle(GetAllProdutosQuery request, CancellationToken cancellationToken)
	{
		var totalCount = await _produtoRepository.GetCountAsync();
		var produto = await _produtoRepository.GetPagedAsync(
			(request.PageNumber - 1) * request.PageSize, request.PageSize);

		var produtos = await _repository.GetAllProdutos(request.Query);

		var produtosViewModel = produtos.Select(p => new ProdutoViewModel(
			p.Id,
			p.Nome,
			p.Preco,
			p.Estoque
		)).ToList();
		
		return new PagedResultModel<ProdutoViewModel>(produtosViewModel, totalCount, request.PageNumber, request.PageSize);
	}
}