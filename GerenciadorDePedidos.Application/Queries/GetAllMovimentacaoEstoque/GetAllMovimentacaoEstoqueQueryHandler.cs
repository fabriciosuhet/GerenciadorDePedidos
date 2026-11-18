using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;

public sealed class GetAllMovimentacaoEstoqueQueryHandler(IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository)
	: IRequestHandler<GetAllMovimentacaoEstoqueQuery, PagedResultModel<MovimentacaoEstoqueResponseDTO>>
{
	public async Task<PagedResultModel<MovimentacaoEstoqueResponseDTO>> Handle(
		GetAllMovimentacaoEstoqueQuery request,
		CancellationToken cancellationToken)
	{
		var count = await movimentacaoEstoqueRepository.GetCountAsync(request.Query, cancellationToken);
		if (count == 0)
			return new PagedResultModel<MovimentacaoEstoqueResponseDTO>([], 0, request.PageNumber, request.PageSize);

		var movimentacoes = await movimentacaoEstoqueRepository.GetPagedAsync(
			request.Query,
			(request.PageNumber - 1) * request.PageSize,
			request.PageSize,
			cancellationToken);

		var items = movimentacoes.Select(me => new MovimentacaoEstoqueResponseDTO
		{
			Id = me.Id,
			ProdutoId = me.ProdutoId,
			ProdutoNome = me.Produto?.Nome ?? "Produto Desconhecido"
		}).ToList();

		return new PagedResultModel<MovimentacaoEstoqueResponseDTO>(items, count, request.PageNumber, request.PageSize);
	}
}