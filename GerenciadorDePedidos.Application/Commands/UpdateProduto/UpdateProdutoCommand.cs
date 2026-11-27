using MediatR;

namespace GerenciadorDePedidos.Application.Commands.UpdateProduto;

public sealed record UpdateProdutoCommand(
	int Id,
	string Nome,
	decimal Preco,
	int Estoque,
	int? EstoqueMinimo = null) : IRequest<Unit>;