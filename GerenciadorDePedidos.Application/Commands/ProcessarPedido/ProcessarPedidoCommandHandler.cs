using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Events;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Observability;
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
	ICacheService cacheService,
	IPedidoRealtimeNotifier realtimeNotifier) : IRequestHandler<ProcessarPedidoCommand, bool>
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
			var statusAnteriorCancelamento = pedido.Status;
			pedido.Cancelar();
			await unitOfWork.SaveChangesAsync(cancellationToken);
			AppDiagnostics.PedidosCancelados.Add(1, new KeyValuePair<string, object?>("motivo", "estoque_insuficiente"));
			InvalidarCachePedido(pedido.Id, pedido.ClienteId);
			await realtimeNotifier.NotificarStatusAtualizadoAsync(
				new PedidoStatusAtualizadoDTO(
					pedido.Id,
					pedido.ClienteId,
					statusAnteriorCancelamento,
					pedido.Status,
					pedido.Total,
					(pedido.AtualizadoEm ?? DateTime.UtcNow).ToBrasiliaTime()),
				cancellationToken);
			return false;
		}

		var statusAnterior = pedido.Status;
		pedido.MarcarComoProcessado();
		var movimentacoes = new List<MovimentacaoEstoque>(itensPorProduto.Count);
		var eventosEstoqueBaixo = new List<EstoqueBaixoEvent>();
		var chavesProdutosAfetados = new List<string>(itensPorProduto.Count);

		foreach (var (produtoId, itemInfo) in itensPorProduto)
		{
			var produto = produtosPorId[produtoId];
			produto.RemoverEstoque(itemInfo.Quantidade);

			movimentacoes.Add(new MovimentacaoEstoque(
				itemInfo.Quantidade,
				Tipo.Remocao,
				produto.Id,
				pedido.ClienteId,
				itemInfo.PrecoUnitario,
				MotivoMovimentacao.VendaPedido,
				pedido.Id));

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

			chavesProdutosAfetados.Add(CacheKeyHelper.GetProdutoByIdKey(produto.Id));
		}

		if (movimentacoes.Count > 0)
			await movimentacaoEstoqueRepository.AddRangeAsync(movimentacoes, cancellationToken);

		// Publica eventos antes do SaveChanges para persistir no Outbox na mesma transação
		foreach (var evento in eventosEstoqueBaixo)
			await eventBus.PublishAsync(evento, cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);
		AppDiagnostics.PedidosProcessados.Add(1);

		InvalidarCachePedido(pedido.Id, pedido.ClienteId);
		if (chavesProdutosAfetados.Count > 0)
		{
			cacheService.RemoveMany(chavesProdutosAfetados);
			cacheService.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix);
		}

		await realtimeNotifier.NotificarStatusAtualizadoAsync(
			new PedidoStatusAtualizadoDTO(
				pedido.Id,
				pedido.ClienteId,
				statusAnterior,
				pedido.Status,
				pedido.Total,
				(pedido.AtualizadoEm ?? DateTime.UtcNow).ToBrasiliaTime()),
			cancellationToken);

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
