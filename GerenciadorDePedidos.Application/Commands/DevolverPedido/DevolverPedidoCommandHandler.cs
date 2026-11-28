using GerenciadorDePedidos.Application.Mappings;
using GerenciadorDePedidos.Application.Models;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Observability;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Commands.DevolverPedido;

public sealed class DevolverPedidoCommandHandler(
	IPedidoRepository pedidoRepository,
	IProdutoRepository produtoRepository,
	IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
	IUnitOfWork unitOfWork,
	ICurrentUserService currentUserService,
	ICacheService cacheService) : IRequestHandler<DevolverPedidoCommand, DevolverPedidoViewModel>
{
	public async Task<DevolverPedidoViewModel> Handle(DevolverPedidoCommand request, CancellationToken cancellationToken)
	{
		var pedido = await pedidoRepository.GetByIdWithItensAsync(request.PedidoId, cancellationToken)
			?? throw new KeyNotFoundException("Pedido nao encontrado");

		pedido.Devolver(request.Motivo);

		var unidadesEstornadas = 0;

		if (request.RetornarAoEstoque && pedido.ItensPedidos.Count > 0)
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

			foreach (var (produtoId, itemInfo) in itensPorProduto)
			{
				if (!produtosPorId.TryGetValue(produtoId, out var produto))
					continue;

				produto.AdicionarEstoque(itemInfo.Quantidade);
				unidadesEstornadas += itemInfo.Quantidade;

				var movimentacao = new MovimentacaoEstoque(
					itemInfo.Quantidade,
					Tipo.Adicao,
					produto.Id,
					responsavelId,
					itemInfo.PrecoUnitario,
					MotivoMovimentacao.Devolucao,
					pedido.Id);

				await movimentacaoEstoqueRepository.AddAsync(movimentacao, cancellationToken);
				cacheService.Remove(CacheKeyHelper.GetProdutoByIdKey(produto.Id));
			}

			cacheService.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix);
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);
		AppDiagnostics.PedidosDevolvidos.Add(
			1,
			new KeyValuePair<string, object?>("retornar_ao_estoque", request.RetornarAoEstoque));

		cacheService.Remove(CacheKeyHelper.GetPedidoByIdKey(pedido.Id));
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosAdminPrefix());
		cacheService.RemoveByPrefix(CacheKeyHelper.GetAllPedidosClientePrefix(pedido.ClienteId));
		cacheService.Remove(CacheKeyHelper.GetClienteByIdKey(pedido.ClienteId));

		return new DevolverPedidoViewModel(
			pedido.Id,
			pedido.Status,
			pedido.MotivoDevolucao!,
			pedido.DevolvidoEm!.Value.ToBrasiliaTime(),
			request.RetornarAoEstoque && unidadesEstornadas > 0,
			unidadesEstornadas);
	}
}

