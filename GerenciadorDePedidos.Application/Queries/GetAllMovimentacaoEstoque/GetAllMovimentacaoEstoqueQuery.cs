using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;

public class GetAllMovimentacaoEstoqueQuery : IRequest<IEnumerable<MovimentacaoEstoqueResponseDTO>>
{
	public string? Query { get; private set; }

	public GetAllMovimentacaoEstoqueQuery(string? query)
	{
		Query = query;
	}
}