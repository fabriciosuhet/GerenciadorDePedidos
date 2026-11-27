using MediatR;

namespace GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;

public sealed record AdicionarProdutoEstoqueCommand(
	int ProdutoId,
	int Quantidade) : IRequest<int>;