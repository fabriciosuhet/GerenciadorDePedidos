using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DeleteProduto;

public sealed record DeleteProdutoCommand(int Id) : IRequest<Unit>;