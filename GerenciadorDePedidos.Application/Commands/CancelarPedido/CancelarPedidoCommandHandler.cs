using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.CancelarPedido;

public sealed class CancelarPedidoCommandHandler(
	IPedidoRepository pedidoRepository,
	IProdutoRepository produtoRepository,
	IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
	IUnitOfWork unitOfWork,
	ICurrentUserService currentUserService,
	ICacheService cacheService) : IRequestHandler<CancelarPedidoCommand, CancelarPedidoViewModel>
{
	public async Task<CancelarPedidoViewModel> Handle(CancelarPedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdWithItensAsync(request.PedidoId, cancellationToken)
			?? throw new KeyNotFoundException("Pedido nao encontrado");

		if (!currentUserService.IsAdmin)
		{
			if (currentUserService.ClienteId is null)
				throw new UnauthorizedAccessException("Usuário não autenticado.");

			if (pedido.ClienteId != currentUserService.ClienteId.Value)
				throw new UnauthorizedAccessException("Você não tem permissão para cancelar este pedido.");
		}

		var requerEstorno = pedido.CancelarComEstorno();
		var unidadesEstornadas = 0;
		var chavesProdutosAfetados = new List<string>();

		if (requerEstorno && pedido.ItensPedidos.Count > 0)
		{
			var itensPorProduto = pedido.ItensPedidos
				.GroupBy(i => i.ProdutoId)
				.ToDictionary(
					g => g.Key,
					g => (Quantidade: g.Sum(i => i.Quantidade), PrecoUnitario: g.First().PrecoUnitario));

			var produtos = await produtoRepository.GetByIdsIncludingInactiveForUpdateAsync(
				itensPorProduto.Keys,
				cancellationToken);
			var produtosPorId = produtos.ToDictionary(p => p.Id);
			var responsavelId = currentUserService.ClienteId ?? pedido.ClienteId;
			var movimentacoes = new List<MovimentacaoEstoque>(itensPorProduto.Count);

			foreach (var (produtoId, itemInfo) in itensPorProduto)
			{
				if (!produtosPorId.TryGetValue(produtoId, out var produto))
					continue;

				produto.AdicionarEstoque(itemInfo.Quantidade);
				unidadesEstornadas += itemInfo.Quantidade;

				movimentacoes.Add(new MovimentacaoEstoque(
					itemInfo.Quantidade,
					Tipo.Adicao,
					produto.Id,
					responsavelId,
					itemInfo.PrecoUnitario,
					MotivoMovimentacao.EstornoCancelamento,
					pedido.Id));

				chavesProdutosAfetados.Add(CacheKeyHelper.GetProdutoByIdKey(produto.Id));
			}

			if (movimentacoes.Count > 0)
				await movimentacaoEstoqueRepository.AddRangeAsync(movimentacoes, cancellationToken);
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);
		AppDiagnostics.PedidosCancelados.Add(1, new KeyValuePair<string, object?>("motivo", "cancelamento_manual"));

		if (chavesProdutosAfetados.Count > 0)
		{
			cacheService.RemoveMany(chavesProdutosAfetados);
			cacheService.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix);
		}

		cacheService.Remove(CacheKeyHelper.GetPedidoByIdKey(pedido.Id));
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosAdminPrefix());
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosClientePrefix(pedido.ClienteId));
		cacheService.Remove(CacheKeyHelper.GetClienteByIdKey(pedido.ClienteId));

		return new CancelarPedidoViewModel(
			pedido.Id,
			pedido.Status,
			requerEstorno && unidadesEstornadas > 0,
			unidadesEstornadas);
	}
}

