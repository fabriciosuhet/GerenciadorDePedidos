using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;

public class GetAllMovimentacaoEstoqueQueryHandler : IRequestHandler<GetAllMovimentacaoEstoqueQuery, PagedResultModel<MovimentacaoEstoqueResponseDTO>>
{
	private readonly IMovimentacaoEstoqueRepository _repository;

	public GetAllMovimentacaoEstoqueQueryHandler(IMovimentacaoEstoqueRepository repository)
	{
		_repository = repository;
	}

	public async Task<PagedResultModel<MovimentacaoEstoqueResponseDTO>> Handle(GetAllMovimentacaoEstoqueQuery request, CancellationToken cancellationToken)
	{
		var count = await _repository.GetCountAsync(request.Query);
		
		if (count.Equals(0)) 
			throw new ArgumentException("Nenhum registro encontrado");

		var movimentacaoEstoque = await _repository.GetPagedAsync(request.Query,
			(request.PageNumber - 1) * request.PageSize, request.PageSize);
		
		
		var movimentacaoEstoqueDto = movimentacaoEstoque.Select(me => new MovimentacaoEstoqueResponseDTO
		{
			Id = me.Id,
			ProdutoId = me.ProdutoId,
			ProdutoNome = me.Produto?.Nome ?? "Produto Desconhecido",
		}).ToList();
		
		return new PagedResultModel<MovimentacaoEstoqueResponseDTO>(movimentacaoEstoqueDto, count, request.PageNumber, request.PageSize);
	}
}