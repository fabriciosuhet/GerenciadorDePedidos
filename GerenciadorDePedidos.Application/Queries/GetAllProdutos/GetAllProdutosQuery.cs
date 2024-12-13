using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllProdutos;

public class GetAllProdutosQuery : IRequest<List<ProdutoViewModel>>
{
	public string? Query { get; private set; }
	
	public GetAllProdutosQuery(string query)
	{
		Query = query;
	}
}