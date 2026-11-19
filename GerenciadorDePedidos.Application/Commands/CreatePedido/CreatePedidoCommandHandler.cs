using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Events;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CreatePedido;

public sealed class CreatePedidoCommandHandler(
	IRepository<Pedido, int> pedidoRepository,
	IProdutoRepository produtoRepository,
	IEventBus eventBus,
	ICurrentUserService currentUserService) : IRequestHandler<CreatePedidoCommand, int>
{
	public async Task<int> Handle(CreatePedidoCommand request, CancellationToken cancellationToken)
	{
		if (request.ItensPedidos is null || request.ItensPedidos.Count == 0)
			throw new ArgumentException("O pedido deve conter pelo menos um item.");

		if (!currentUserService.IsAdmin)
		{
			if (currentUserService.ClienteId is null)
				throw new UnauthorizedAccessException("Usuário não autenticado.");

			if (request.ClienteId != currentUserService.ClienteId.Value)
				throw new UnauthorizedAccessException("Você não pode criar pedidos em nome de outro cliente.");
		}

		var produtoIds = request.ItensPedidos.Select(i => i.ProdutoId).Distinct().ToList();
		var produtos = await produtoRepository.GetByIdsAsync(produtoIds, cancellationToken);
		var produtosPorId = produtos.ToDictionary(p => p.Id);

		var pedido = new Pedido(request.ClienteId, Status.Pendente);

		foreach (var itemDto in request.ItensPedidos)
		{
			if (!produtosPorId.TryGetValue(itemDto.ProdutoId, out var produto))
				throw new ArgumentException($"O produto com o ID {itemDto.ProdutoId} nao foi encontrado.");

			pedido.AdicionarItem(produto, itemDto.Quantidade);
		}

		await pedidoRepository.AddAsync(pedido, cancellationToken);
		await pedidoRepository.SaveChangesAsync(cancellationToken);

		await eventBus.PublishAsync(new PedidoCriadoEvent(pedido.Id), cancellationToken);

		return pedido.Id;
	}
}