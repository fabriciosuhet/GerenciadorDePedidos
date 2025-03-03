using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;

public class GetAllMovimentacaoEstoqueQueryHandler : IRequestHandler<GetAllMovimentacaoEstoqueQuery, IEnumerable<MovimentacaoEstoqueResponseDTO>>
{
	private readonly IMovimentacaoEstoqueRepository _repository;

	public GetAllMovimentacaoEstoqueQueryHandler(IMovimentacaoEstoqueRepository repository)
	{
		_repository = repository;
	}

	public async Task<IEnumerable<MovimentacaoEstoqueResponseDTO>> Handle(GetAllMovimentacaoEstoqueQuery request, CancellationToken cancellationToken)
	{
		var movimentacaoEstoque = await _repository.GetAllAsync(request.Query);
		
		if (movimentacaoEstoque is null) 
			throw new ArgumentException("Nenhum registro encontrado");
		
		
		var movimentacaoEstoqueDto = movimentacaoEstoque.Select(me => new MovimentacaoEstoqueResponseDTO
		{
			Id = me.Id,
			ProdutoId = me.ProdutoId,
			ProdutoNome = me.Produto?.Nome ?? "Produto Desconhecido",
		});
		
		return movimentacaoEstoqueDto;
	}
}