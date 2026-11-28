using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using MediatR;

namespace GerenciadorDePedidos.Application.Queries.ExportarRelatorioVendas;

public sealed class ExportarRelatorioVendasQueryHandler(
	IPedidoRepository pedidoRepository,
	IRelatorioVendasGenerator relatorioVendasGenerator)
	: IRequestHandler<ExportarRelatorioVendasQuery, ArquivoRelatorioDTO>
{
	public async Task<ArquivoRelatorioDTO> Handle(
		ExportarRelatorioVendasQuery request,
		CancellationToken cancellationToken)
	{
		var geradoEmUtc = DateTime.UtcNow;
		var referencia = (request.DataReferencia ?? geradoEmUtc).Date;
		var (dataInicio, dataFimExclusiva) = PeriodoRelatorioHelper.CalcularIntervalo(request.Periodo, referencia);

		var pedidos = await pedidoRepository.GetByPeriodoAsync(dataInicio, dataFimExclusiva, cancellationToken);

		var pedidosConcluidos = pedidos.Where(p => p.Status == Status.Concluido).ToList();
		var pedidosCancelados = pedidos.Where(p => p.Status == Status.Cancelado).ToList();
		var pedidosDevolvidos = pedidos.Where(p => p.Status == Status.Devolvido).ToList();
		var pedidosPendentes = pedidos.Count(p => p.Status == Status.Pendente);
		var pedidosProcessados = pedidos.Count(p => p.Status == Status.Processado);

		var faturamentoConcluido = pedidosConcluidos.Sum(p => p.Total);
		var valorCancelado = pedidosCancelados.Sum(p => p.Total);
		var valorDevolvido = pedidosDevolvidos.Sum(p => p.Total);
		var totalUnidadesVendidas = pedidosConcluidos
			.SelectMany(p => p.ItensPedidos)
			.Sum(i => i.Quantidade);

		var ticketMedio = pedidosConcluidos.Count > 0
			? Math.Round(faturamentoConcluido / pedidosConcluidos.Count, 2)
			: 0m;

		var taxaCancelamento = pedidos.Count > 0
			? Math.Round((decimal)pedidosCancelados.Count / pedidos.Count * 100m, 2)
			: 0m;

		var topProdutos = pedidosConcluidos
			.SelectMany(p => p.ItensPedidos)
			.GroupBy(i => new { i.ProdutoId, Nome = i.Produto?.Nome ?? "Produto Desconhecido" })
			.Select(g =>
			{
				var faturamentoProduto = g.Sum(i => i.Total);
				var participacao = faturamentoConcluido > 0m
					? Math.Round(faturamentoProduto / faturamentoConcluido * 100m, 2)
					: 0m;

				return new RelatorioVendaProdutoRankingDTO(
					g.Key.ProdutoId,
					g.Key.Nome,
					g.Sum(i => i.Quantidade),
					faturamentoProduto,
					participacao);
			})
			.OrderByDescending(p => p.FaturamentoTotal)
			.ThenByDescending(p => p.QuantidadeVendida)
			.ThenBy(p => p.ProdutoNome)
			.ToList();

		var clientesMaisAtivos = pedidos
			.GroupBy(p => new { p.ClienteId, Nome = p.Cliente?.NomeCompleto ?? "Cliente Desconhecido" })
			.Select(g =>
			{
				var concluidosCliente = g.Where(p => p.Status == Status.Concluido).ToList();
				var canceladosCliente = g.Count(p => p.Status == Status.Cancelado);
				var valorComprado = concluidosCliente.Sum(p => p.Total);
				var ticketCliente = concluidosCliente.Count > 0
					? Math.Round(valorComprado / concluidosCliente.Count, 2)
					: 0m;

				return new RelatorioVendaClienteRankingDTO(
					g.Key.ClienteId,
					g.Key.Nome,
					concluidosCliente.Count,
					canceladosCliente,
					valorComprado,
					ticketCliente);
			})
			.OrderByDescending(c => c.ValorTotalComprado)
			.ThenByDescending(c => c.PedidosConcluidos)
			.ThenBy(c => c.ClienteNome)
			.ToList();

		var itensPedidos = pedidos
			.Select(p => new RelatorioVendaPedidoItemDTO(
				p.Id,
				p.DataPedido,
				p.ClienteId,
				p.Cliente?.NomeCompleto ?? "Cliente Desconhecido",
				p.Status,
				p.ItensPedidos.Count,
				p.ItensPedidos.Sum(i => i.Quantidade),
				p.Total))
			.ToList();

		var relatorio = new RelatorioVendasDTO(
			request.Periodo,
			dataInicio,
			dataFimExclusiva.AddDays(-1),
			geradoEmUtc,
			pedidos.Count,
			pedidosPendentes,
			pedidosProcessados,
			pedidosConcluidos.Count,
			pedidosCancelados.Count,
			totalUnidadesVendidas,
			faturamentoConcluido,
			valorCancelado,
			ticketMedio,
			taxaCancelamento,
			topProdutos,
			clientesMaisAtivos,
			itensPedidos,
			pedidosDevolvidos.Count,
			valorDevolvido);

		return relatorioVendasGenerator.Gerar(relatorio, request.Formato);
	}
}

