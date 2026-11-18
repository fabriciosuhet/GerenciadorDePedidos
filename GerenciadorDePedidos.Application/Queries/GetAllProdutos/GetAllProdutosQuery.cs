using GerenciadorDePedidos.Application.Models;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllProdutos;

public sealed record GetAllProdutosQuery(
	string? Query = null,
	int PageNumber = 1,
	int PageSize = 10) : IRequest<PagedResultModel<ProdutoViewModel>>;