using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;

public sealed record GetAllMovimentacaoEstoqueQuery(
	string? Query = null,
	int PageNumber = 1,
	int PageSize = 10) : IRequest<PagedResultModel<MovimentacaoEstoqueResponseDTO>>;