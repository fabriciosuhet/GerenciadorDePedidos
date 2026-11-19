using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
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
	ICacheService cacheService) : IRequestHandler<ProcessarPedidoCommand, bool>
{
	public async Task<bool> Handle(ProcessarPedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdWithItensAsync(request.PedidoId, cancellationToken);
		if (pedido is null || pedido.Status != Status.Pendente)
			return false;

		var quantidadePorProduto = pedido.ItensPedidos
			.GroupBy(i => i.ProdutoId)
			.ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));

		var produtos = await produtoRepository.GetByIdsForUpdateAsync(quantidadePorProduto.Keys, cancellationToken);
		var produtosPorId = produtos.ToDictionary(p => p.Id);

		var estoqueDisponivel = quantidadePorProduto.All(kvp =>
			produtosPorId.TryGetValue(kvp.Key, out var produto) && produto.Estoque >= kvp.Value);

		if (!estoqueDisponivel)
		{
			pedido.Cancelar();
			await unitOfWork.SaveChangesAsync(cancellationToken);
			InvalidarCachePedido(pedido.Id, pedido.ClienteId);
			return false;
		}

		pedido.MarcarComoProcessado();

		foreach (var (produtoId, quantidade) in quantidadePorProduto)
		{
			var produto = produtosPorId[produtoId];
			produto.RemoverEstoque(quantidade);

			var movimentacao = new MovimentacaoEstoque(quantidade, Tipo.Remocao, produto.Id, pedido.ClienteId);
			await movimentacaoEstoqueRepository.AddAsync(movimentacao, cancellationToken);

			cacheService.Remove(CacheKeyHelper.GetProdutoByIdKey(produto.Id));
		}

		pedido.Concluir();
		await unitOfWork.SaveChangesAsync(cancellationToken);

		InvalidarCachePedido(pedido.Id, pedido.ClienteId);
		cacheService.Remove(CacheKeyHelper.GetAllProdutosKey(null));

		return true;
	}

	private void InvalidarCachePedido(int pedidoId, Guid clienteId)
	{
		cacheService.Remove(CacheKeyHelper.GetPedidoByIdKey(pedidoId));
		cacheService.Remove(CacheKeyHelper.GetAllPedidosKey(null));
		cacheService.Remove(CacheKeyHelper.GetAllPedidosKey(null, clienteId));
	}
}
