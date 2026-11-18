using GerenciadorDePedidos.Core.DTOs;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;

public sealed record GetMovimentacaoEstoqueQuery(int Id) : IRequest<MovimentacaoEstoqueDetailsDTO?>;