using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetProduto;

public class GetProdutoQuery : IRequest<ProdutoDetailsViewModel>
{
	public int Id { get; private set; }

	public GetProdutoQuery(int id)
	{
		Id = id;
	}
}