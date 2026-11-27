using MediatR;

namespace GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;

public sealed record RemoverProdutoEstoqueCommand(
	int ProdutoId,
	int Quantidade) : IRequest<int>;