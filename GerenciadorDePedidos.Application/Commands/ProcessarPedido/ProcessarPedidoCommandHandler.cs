using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Events;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.ProcessarPedido;

public sealed class ProcessarPedidoCommandHandler(
	IPedidoRepository pedidoRepository,
	IProdutoRepository produtoRepository,
	IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
	IUnitOfWork unitOfWork,
	IEventBus eventBus,
	ICacheService cacheService) : IRequestHandler<ProcessarPedidoCommand, bool>
{
	public async Task<bool> Handle(ProcessarPedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdWithItensAsync(request.PedidoId, cancellationToken);
		if (pedido is null || pedido.Status != Status.Pendente)
			return false;

		var itensPorProduto = pedido.ItensPedidos
			.GroupBy(i => i.ProdutoId)
			.ToDictionary(
				g => g.Key,
				g => (Quantidade: g.Sum(i => i.Quantidade), PrecoUnitario: g.First().PrecoUnitario));

		var produtos = await produtoRepository.GetByIdsForUpdateAsync(itensPorProduto.Keys, cancellationToken);
		var produtosPorId = produtos.ToDictionary(p => p.Id);

		var estoqueDisponivel = itensPorProduto.All(kvp =>
			produtosPorId.TryGetValue(kvp.Key, out var produto) && produto.Estoque >= kvp.Value.Quantidade);

		if (!estoqueDisponivel)
		{
			pedido.Cancelar();
			await unitOfWork.SaveChangesAsync(cancellationToken);
			InvalidarCachePedido(pedido.Id, pedido.ClienteId);
			return false;
		}

		pedido.MarcarComoProcessado();
		var eventosEstoqueBaixo = new List<EstoqueBaixoEvent>();

		foreach (var (produtoId, itemInfo) in itensPorProduto)
		{
			var produto = produtosPorId[produtoId];
			produto.RemoverEstoque(itemInfo.Quantidade);

			var movimentacao = new MovimentacaoEstoque(
				itemInfo.Quantidade,
				Tipo.Remocao,
				produto.Id,
				pedido.ClienteId,
				itemInfo.PrecoUnitario,
				MotivoMovimentacao.VendaPedido,
				pedido.Id);
			await movimentacaoEstoqueRepository.AddAsync(movimentacao, cancellationToken);

			if (produto.EstaComEstoqueBaixo())
			{
				eventosEstoqueBaixo.Add(new EstoqueBaixoEvent(
					produto.Id,
					produto.Nome,
					produto.Estoque,
					produto.EstoqueMinimo,
					DateTime.UtcNow,
					pedido.Id));
			}

			cacheService.Remove(CacheKeyHelper.GetProdutoByIdKey(produto.Id));
		}

		pedido.Concluir();

		// Publica eventos antes do SaveChanges para persistir no Outbox na mesma transação
		foreach (var evento in eventosEstoqueBaixo)
			await eventBus.PublishAsync(evento, cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		InvalidarCachePedido(pedido.Id, pedido.ClienteId);
		cacheService.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix);

		return true;
	}

	private void InvalidarCachePedido(int pedidoId, Guid clienteId)
	{
		cacheService.Remove(CacheKeyHelper.GetPedidoByIdKey(pedidoId));
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosAdminPrefix());
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosClientePrefix(clienteId));
		cacheService.Remove(CacheKeyHelper.GetClienteByIdKey(clienteId));
	}
}
