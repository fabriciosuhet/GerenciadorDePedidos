using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;

public sealed class GetMovimentacaoEstoqueQueryHandler(IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository)
	: IRequestHandler<GetMovimentacaoEstoqueQuery, MovimentacaoEstoqueDetailsDTO?>
{
	public async Task<MovimentacaoEstoqueDetailsDTO?> Handle(
		GetMovimentacaoEstoqueQuery request,
		CancellationToken cancellationToken)
	{
		var movimentacao = await movimentacaoEstoqueRepository.GetByIdMovimentacaoAsync(request.Id, cancellationToken);
		if (movimentacao is null)
			return null;

		return new MovimentacaoEstoqueDetailsDTO
		{
			Id = movimentacao.Id,
			ProdutoId = movimentacao.ProdutoId,
			ProdutoNome = movimentacao.Produto?.Nome ?? string.Empty,
			ClienteNome = movimentacao.Cliente?.NomeCompleto ?? string.Empty,
			Data = movimentacao.DataMovimentacao.ToBrasiliaTime(),
			Quantidade = movimentacao.Quantidade,
			TipoMovimentacao = movimentacao.TipoMovimentacao
		};
	}
}