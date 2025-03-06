using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllProdutos;

public class GetAllProdutosQuery : IRequest<PagedResultModel<ProdutoViewModel>>
{
	public string? Query { get; private set; }
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 10;

	public GetAllProdutosQuery(string? query)
	{
		Query = query;
	}
}