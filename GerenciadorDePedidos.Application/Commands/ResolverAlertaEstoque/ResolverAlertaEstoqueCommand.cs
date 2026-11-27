using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ResolverAlertaEstoque;

public sealed record ResolverAlertaEstoqueCommand(int Id) : IRequest<Unit>;

