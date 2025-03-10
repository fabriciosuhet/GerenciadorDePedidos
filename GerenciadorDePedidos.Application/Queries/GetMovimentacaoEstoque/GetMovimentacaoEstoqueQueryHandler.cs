using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;

public class GetMovimentacaoEstoqueQueryHandler : IRequestHandler<GetMovimentacaoEstoqueQuery, MovimentacaoEstoqueDetailsDTO>
{
	private readonly IMovimentacaoEstoqueRepository _repository;

	public GetMovimentacaoEstoqueQueryHandler(IMovimentacaoEstoqueRepository repository) => _repository = repository;

	public async Task<MovimentacaoEstoqueDetailsDTO> Handle(GetMovimentacaoEstoqueQuery request, CancellationToken cancellationToken)
	{
		var movimentacaoEstoque = await _repository.GetByIdAsync(request.Id);
		if (movimentacaoEstoque is null)
			return null;

		var fusoHorarioBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
		
		var movimentacaoEstoqueDetailsDto = new MovimentacaoEstoqueDetailsDTO
		{
			Id = movimentacaoEstoque.Id,
			ProdutoId = movimentacaoEstoque.ProdutoId,
			ProdutoNome = movimentacaoEstoque.Produto.Nome,
			ClienteNome = movimentacaoEstoque.Cliente.NomeCompleto,
			Data = TimeZoneInfo.ConvertTimeFromUtc(movimentacaoEstoque.DataMovimentacao, fusoHorarioBrasilia),
			Quantidade = movimentacaoEstoque.Quantidade,
			TipoMovimentacao = movimentacaoEstoque.TipoMovimentacao,
		};
		
		return movimentacaoEstoqueDetailsDto;
		
	}
}