using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;

public class GetMovimentacaoEstoqueQuery : IRequest<MovimentacaoEstoqueDetailsDTO>
{
	public Guid Id { get; private set; }

	public GetMovimentacaoEstoqueQuery(Guid id)
	{
		Id = id;
	}
}