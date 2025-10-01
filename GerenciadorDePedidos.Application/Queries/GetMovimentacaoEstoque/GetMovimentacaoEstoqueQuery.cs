using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;

public class GetMovimentacaoEstoqueQuery : IRequest<MovimentacaoEstoqueDetailsDTO>
{
	public int Id { get; private set; }

	public GetMovimentacaoEstoqueQuery(int id)
	{
		Id = id;
	}
}