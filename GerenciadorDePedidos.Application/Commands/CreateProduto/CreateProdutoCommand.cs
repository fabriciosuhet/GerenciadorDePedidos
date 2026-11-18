using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreateProduto;

public sealed record CreateProdutoCommand(string Nome, decimal Preco, int Estoque) : IRequest<int>;