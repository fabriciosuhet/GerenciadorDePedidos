using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;

public class GetAllMovimentacaoEstoqueQuery : IRequest<PagedResultModel<MovimentacaoEstoqueResponseDTO>>
{
	public string? Query { get; private set; }
	public int PageNumber { get; private set; } = 1;
	public int PageSize { get; private set; } = 10;
	
	public GetAllMovimentacaoEstoqueQuery(string? query)
	{
		Query = query;
	}
}