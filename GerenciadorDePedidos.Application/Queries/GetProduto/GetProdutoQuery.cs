using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetProduto;

public class GetProdutoQuery : IRequest<ProdutoDetailsViewModel>
{
	public Guid Id { get; private set; }

	public GetProdutoQuery(Guid id)
	{
		Id = id;
	}
}