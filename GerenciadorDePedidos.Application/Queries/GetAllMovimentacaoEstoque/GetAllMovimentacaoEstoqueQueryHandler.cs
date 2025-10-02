using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;

public class GetAllMovimentacaoEstoqueQueryHandler : IRequestHandler<GetAllMovimentacaoEstoqueQuery, PagedResultModel<MovimentacaoEstoqueResponseDTO>>
{
	private readonly IRepository<MovimentacaoEstoque, int> _baseRepository;
	private readonly IMovimentacaoEstoqueRepository _movimentacoEstoqueRepository;

    public GetAllMovimentacaoEstoqueQueryHandler(IRepository<MovimentacaoEstoque, int> baseRepository, IMovimentacaoEstoqueRepository movimentacoEstoqueRepository)
    {
        _baseRepository = baseRepository;
        _movimentacoEstoqueRepository = movimentacoEstoqueRepository;
    }

    public async Task<PagedResultModel<MovimentacaoEstoqueResponseDTO>> Handle(GetAllMovimentacaoEstoqueQuery request, CancellationToken cancellationToken)
	{
		var count = await _baseRepository.GetCountAsync();

		var movimentacoes = await _movimentacoEstoqueRepository.GetAllMovimentacaoAsync(request.Query);
        

        if (count.Equals(0)) 
			throw new ArgumentException("Nenhum registro encontrado");

		var movimentacaoEstoque = await _baseRepository.GetPagedAsync(
			(request.PageNumber - 1) * request.PageSize, request.PageSize);
		
		
		var movimentacaoEstoqueDto = movimentacoes.Select(me => new MovimentacaoEstoqueResponseDTO
		{
			Id = me.Id,
			ProdutoId = me.ProdutoId,
			ProdutoNome = me.Produto?.Nome ?? "Produto Desconhecido",
		}).ToList();
		
		return new PagedResultModel<MovimentacaoEstoqueResponseDTO>(movimentacaoEstoqueDto, count, request.PageNumber, request.PageSize);
	}
}