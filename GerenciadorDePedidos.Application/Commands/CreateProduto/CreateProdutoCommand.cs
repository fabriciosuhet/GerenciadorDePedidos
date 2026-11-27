using GerenciadorDePedidos.Core.Entities;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateProduto;

public sealed record CreateProdutoCommand(
	string Nome,
	decimal Preco,
	int Estoque,
	int EstoqueMinimo = Produto.EstoqueMinimoPadrao) : IRequest<int>;