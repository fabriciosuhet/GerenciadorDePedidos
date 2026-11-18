using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public sealed class CreatePedidoCommandHandler(
	IRepository<Pedido, int> pedidoRepository,
	IProdutoRepository produtoRepository) : IRequestHandler<CreatePedidoCommand, int>
{
	public async Task<int> Handle(CreatePedidoCommand request, CancellationToken cancellationToken)
	{
		if (request.ItensPedidos is null || request.ItensPedidos.Count == 0)
			throw new ArgumentException("O pedido deve conter pelo menos um item.");

		var produtoIds = request.ItensPedidos.Select(i => i.ProdutoId).Distinct().ToList();
		var produtos = await produtoRepository.GetByIdsAsync(produtoIds, cancellationToken);
		var produtosPorId = produtos.ToDictionary(p => p.Id);

		var pedido = new Pedido(request.ClienteId, request.Status);

		foreach (var itemDto in request.ItensPedidos)
		{
			if (!produtosPorId.TryGetValue(itemDto.ProdutoId, out var produto))
				throw new ArgumentException($"O produto com o ID {itemDto.ProdutoId} nao foi encontrado.");

			pedido.AdicionarItem(produto, itemDto.Quantidade);
		}

		await pedidoRepository.AddAsync(pedido, cancellationToken);
		await pedidoRepository.SaveChangesAsync(cancellationToken);

		return pedido.Id;
	}
}