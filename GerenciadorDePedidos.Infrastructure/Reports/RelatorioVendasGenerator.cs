using System.Globalization;
using System.Text;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GerenciadorDePedidos.Infrastructure.Reports;

public sealed class RelatorioVendasGenerator : IRelatorioVendasGenerator
{
	private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

	static RelatorioVendasGenerator()
	{
		QuestPDF.Settings.License = LicenseType.Community;
	}

	public ArquivoRelatorioDTO Gerar(RelatorioVendasDTO relatorio, FormatoRelatorio formato)
		=> formato switch
		{
			FormatoRelatorio.Csv => GerarCsv(relatorio),
			FormatoRelatorio.Pdf => GerarPdf(relatorio),
			_ => throw new ArgumentOutOfRangeException(nameof(formato), formato, "Formato de relatório inválido.")
		};

	private static ArquivoRelatorioDTO GerarCsv(RelatorioVendasDTO relatorio)
	{
		const int totalColunas = 7;
		var sb = new StringBuilder();

		// Cabeçalho do relatório
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "RELATÓRIO DE VENDAS");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Período",
			CsvReportHelper.Escape(FormatarPeriodo(relatorio.Periodo)),
			"Vigência",
			$"{CsvReportHelper.FormatDate(relatorio.DataInicio)} a {CsvReportHelper.FormatDate(relatorio.DataFim)}",
			"Gerado em (Brasília)",
			CsvReportHelper.FormatDateTimeBrasilia(relatorio.GeradoEmUtc));
		CsvReportHelper.AppendBlankRow(sb, totalColunas);

		// Seção 1: Resumo Executivo
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "RESUMO EXECUTIVO");
		CsvReportHelper.AppendRow(sb, totalColunas, "Indicador", "Valor", "Detalhamento");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Total de Pedidos",
			CsvReportHelper.FormatInteger(relatorio.TotalPedidos),
			$"{relatorio.PedidosConcluidos} concluído(s) | {relatorio.PedidosPendentes} pendente(s) | {relatorio.PedidosProcessados} processado(s) | {relatorio.PedidosCancelados} cancelado(s)");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Unidades Vendidas",
			CsvReportHelper.FormatInteger(relatorio.TotalUnidadesVendidas),
			"Itens de pedidos concluídos");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Faturamento Concluído",
			CsvReportHelper.FormatCurrency(relatorio.FaturamentoTotalConcluido),
			"Receita efetivada no período");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Valor Cancelado",
			CsvReportHelper.FormatCurrency(relatorio.ValorTotalCancelado),
			"Pedidos cancelados no período");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Ticket Médio",
			CsvReportHelper.FormatCurrency(relatorio.TicketMedio),
			"Média por pedido concluído");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Taxa de Cancelamento",
			CsvReportHelper.FormatPercent(relatorio.TaxaCancelamentoPercentual),
			"Percentual sobre o total de pedidos");
		CsvReportHelper.AppendBlankRow(sb, totalColunas);

		// Seção 2: Top Produtos
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "TOP PRODUTOS");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"#",
			"Cód. Produto",
			"Produto",
			"Qtd. Vendida",
			"Faturamento Total",
			"Participação");

		if (relatorio.TopProdutos.Count == 0)
		{
			CsvReportHelper.AppendRow(sb, totalColunas, "Sem produtos vendidos no período.");
		}
		else
		{
			var posicao = 1;
			foreach (var prod in relatorio.TopProdutos)
			{
				CsvReportHelper.AppendRow(
					sb,
					totalColunas,
					posicao.ToString(PtBr),
					prod.ProdutoId.ToString(PtBr),
					CsvReportHelper.Escape(prod.ProdutoNome),
					CsvReportHelper.FormatInteger(prod.QuantidadeVendida),
					CsvReportHelper.FormatCurrency(prod.FaturamentoTotal),
					CsvReportHelper.FormatPercent(prod.ParticipacaoPercentual));
				posicao++;
			}

			CsvReportHelper.AppendRow(
				sb,
				totalColunas,
				"TOTAL",
				string.Empty,
				$"{relatorio.TopProdutos.Count} produto(s)",
				CsvReportHelper.FormatInteger(relatorio.TopProdutos.Sum(p => p.QuantidadeVendida)),
				CsvReportHelper.FormatCurrency(relatorio.TopProdutos.Sum(p => p.FaturamentoTotal)),
				CsvReportHelper.FormatPercent(relatorio.TopProdutos.Sum(p => p.ParticipacaoPercentual)));
		}

		CsvReportHelper.AppendBlankRow(sb, totalColunas);

		// Seção 3: Clientes Mais Ativos
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "CLIENTES MAIS ATIVOS");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"#",
			"Cliente",
			"Pedidos Concluídos",
			"Pedidos Cancelados",
			"Total Comprado",
			"Ticket Médio",
			"ID do Cliente");

		if (relatorio.ClientesMaisAtivos.Count == 0)
		{
			CsvReportHelper.AppendRow(sb, totalColunas, "Sem clientes com pedidos no período.");
		}
		else
		{
			var posicao = 1;
			foreach (var cliente in relatorio.ClientesMaisAtivos)
			{
				CsvReportHelper.AppendRow(
					sb,
					totalColunas,
					posicao.ToString(PtBr),
					CsvReportHelper.Escape(cliente.ClienteNome),
					CsvReportHelper.FormatInteger(cliente.PedidosConcluidos),
					CsvReportHelper.FormatInteger(cliente.PedidosCancelados),
					CsvReportHelper.FormatCurrency(cliente.ValorTotalComprado),
					CsvReportHelper.FormatCurrency(cliente.TicketMedio),
					cliente.ClienteId.ToString());
				posicao++;
			}

			CsvReportHelper.AppendRow(
				sb,
				totalColunas,
				"TOTAL",
				$"{relatorio.ClientesMaisAtivos.Count} cliente(s)",
				CsvReportHelper.FormatInteger(relatorio.ClientesMaisAtivos.Sum(c => c.PedidosConcluidos)),
				CsvReportHelper.FormatInteger(relatorio.ClientesMaisAtivos.Sum(c => c.PedidosCancelados)),
				CsvReportHelper.FormatCurrency(relatorio.ClientesMaisAtivos.Sum(c => c.ValorTotalComprado)),
				CsvReportHelper.FormatCurrency(relatorio.TicketMedio));
		}

		CsvReportHelper.AppendBlankRow(sb, totalColunas);

		// Seção 4: Pedidos Detalhados
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "PEDIDOS DETALHADOS");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Nº Pedido",
			"Data/Hora (Brasília)",
			"Cliente",
			"Status",
			"Qtd. Itens",
			"Total Unidades",
			"Valor Total");

		if (relatorio.Pedidos.Count == 0)
		{
			CsvReportHelper.AppendRow(sb, totalColunas, "Sem pedidos registrados no período.");
		}
		else
		{
			foreach (var pedido in relatorio.Pedidos)
			{
				CsvReportHelper.AppendRow(
					sb,
					totalColunas,
					pedido.PedidoId.ToString(PtBr),
					CsvReportHelper.FormatDateTimeBrasilia(pedido.DataPedido),
					CsvReportHelper.Escape(pedido.ClienteNome),
					CsvReportHelper.Escape(FormatarStatus(pedido.Status)),
					CsvReportHelper.FormatInteger(pedido.QuantidadeItens),
					CsvReportHelper.FormatInteger(pedido.TotalUnidades),
					CsvReportHelper.FormatCurrency(pedido.ValorTotal));
			}

			CsvReportHelper.AppendRow(
				sb,
				totalColunas,
				"TOTAL",
				$"{relatorio.Pedidos.Count} pedido(s)",
				string.Empty,
				string.Empty,
				CsvReportHelper.FormatInteger(relatorio.Pedidos.Sum(p => p.QuantidadeItens)),
				CsvReportHelper.FormatInteger(relatorio.Pedidos.Sum(p => p.TotalUnidades)),
				CsvReportHelper.FormatCurrency(relatorio.Pedidos.Sum(p => p.ValorTotal)));
		}

		return new ArquivoRelatorioDTO(
			CsvReportHelper.ToUtf8WithBom(sb.ToString()),
			"text/csv; charset=utf-8",
			ConstruirNomeArquivo(relatorio, "csv"));
	}

	private static ArquivoRelatorioDTO GerarPdf(RelatorioVendasDTO relatorio)
	{
		var bytes = Document.Create(container =>
		{
			container.Page(page =>
			{
				page.Size(PageSizes.A4.Landscape());
				page.Margin(28);
				page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken4));

				page.Header().Element(h => ComporCabecalho(h, relatorio));
				page.Content().PaddingVertical(10).Element(c => ComporConteudo(c, relatorio));
				page.Footer().Element(ComporRodape);
			});
		}).GeneratePdf();

		return new ArquivoRelatorioDTO(
			bytes,
			"application/pdf",
			ConstruirNomeArquivo(relatorio, "pdf"));
	}

	private static void ComporCabecalho(IContainer container, RelatorioVendasDTO relatorio)
	{
		container.Background("#1E3A5F").Padding(14).Row(row =>
		{
			row.RelativeItem().Column(col =>
			{
				col.Item().Text("Relatório de Vendas e Pedidos")
					.FontSize(16).Bold().FontColor(Colors.White);
				col.Item().PaddingTop(2)
					.Text($"Período {FormatarPeriodo(relatorio.Periodo)} | {relatorio.DataInicio.ToString("dd/MM/yyyy", PtBr)} a {relatorio.DataFim.ToString("dd/MM/yyyy", PtBr)}")
					.FontSize(10).FontColor(Colors.Grey.Lighten3);
			});

			row.ConstantItem(180).AlignRight().AlignMiddle().Column(col =>
			{
				col.Item().AlignRight().Text("Gerenciador de Pedidos")
					.FontSize(10).Bold().FontColor(Colors.White);
				col.Item().AlignRight()
					.Text($"Emitido em {relatorio.GeradoEmUtc.ToString("dd/MM/yyyy HH:mm", PtBr)} UTC")
					.FontSize(8).FontColor(Colors.Grey.Lighten3);
			});
		});
	}

	private static void ComporConteudo(IContainer container, RelatorioVendasDTO relatorio)
	{
		container.Column(col =>
		{
			col.Spacing(14);
			col.Item().Element(c => ComporResumoExecutivo(c, relatorio));

			if (relatorio.Pedidos.Count == 0)
			{
				col.Item()
					.Border(1).BorderColor(Colors.Grey.Lighten2)
					.Background(Colors.Grey.Lighten4)
					.Padding(16)
					.AlignCenter()
					.Text("Nenhum pedido registrado no período selecionado.")
					.FontSize(11).Italic().FontColor(Colors.Grey.Darken2);
				return;
			}

			if (relatorio.TopProdutos.Count > 0)
				col.Item().Element(c => ComporTopProdutos(c, relatorio.TopProdutos));

			col.Item().Element(c => ComporClientesMaisAtivos(c, relatorio.ClientesMaisAtivos));
			col.Item().Element(c => ComporTabelaPedidos(c, relatorio.Pedidos));
		});
	}

	private static void ComporResumoExecutivo(IContainer container, RelatorioVendasDTO relatorio)
	{
		container.Column(col =>
		{
			col.Item().PaddingBottom(6).Text("Resumo Executivo")
				.FontSize(12).Bold().FontColor("#1E3A5F");

			col.Item().Row(row =>
			{
				row.Spacing(8);
				AdicionarCard(row, "Total de Pedidos", relatorio.TotalPedidos.ToString(PtBr),
					$"{relatorio.PedidosConcluidos} concluídos / {relatorio.PedidosCancelados} cancelados");
				AdicionarCard(row, "Faturamento Concluído", FormatarMoeda(relatorio.FaturamentoTotalConcluido),
					$"{relatorio.TotalUnidadesVendidas} unidades vendidas", "#15803D");
				AdicionarCard(row, "Ticket Médio", FormatarMoeda(relatorio.TicketMedio),
					"Média por pedido concluído", "#1E3A5F");
				AdicionarCard(row, "Valor Cancelado", FormatarMoeda(relatorio.ValorTotalCancelado),
					$"Taxa de cancelamento: {FormatarDecimal(relatorio.TaxaCancelamentoPercentual)}%", "#B91C1C");
			});
		});
	}

	private static void AdicionarCard(
		RowDescriptor row,
		string titulo,
		string valorPrincipal,
		string detalhe,
		string? corValor = null)
	{
		row.RelativeItem()
			.Border(1).BorderColor(Colors.Grey.Lighten2)
			.Background(Colors.Grey.Lighten5)
			.Padding(8)
			.Column(col =>
			{
				col.Item().Text(titulo).FontSize(8).FontColor(Colors.Grey.Darken1);
				col.Item().PaddingVertical(2).Text(valorPrincipal)
					.FontSize(13).Bold().FontColor(corValor ?? Colors.Grey.Darken4);
				col.Item().Text(detalhe).FontSize(7.5f).FontColor(Colors.Grey.Darken2);
			});
	}

	private static void ComporTopProdutos(IContainer container, IReadOnlyList<RelatorioVendaProdutoRankingDTO> produtos)
	{
		container.Column(col =>
		{
			col.Item().PaddingBottom(6).Text("Produtos Mais Vendidos (Pedidos Concluídos)")
				.FontSize(12).Bold().FontColor("#1E3A5F");

			col.Item().Table(table =>
			{
				table.ColumnsDefinition(columns =>
				{
					columns.ConstantColumn(45);
					columns.RelativeColumn(3.5f);
					columns.ConstantColumn(90);
					columns.RelativeColumn(1.5f);
					columns.ConstantColumn(90);
				});

				table.Header(header =>
				{
					CelulaCabecalho(header.Cell(), "ID");
					CelulaCabecalho(header.Cell(), "Produto");
					CelulaCabecalho(header.Cell(), "Qtd. Vendida", alignRight: true);
					CelulaCabecalho(header.Cell(), "Faturamento", alignRight: true);
					CelulaCabecalho(header.Cell(), "Participação", alignRight: true);
				});

				for (var i = 0; i < produtos.Count; i++)
				{
					var prod = produtos[i];
					var fundo = i % 2 == 1 ? Colors.Grey.Lighten5 : Colors.White;

					CelulaDado(table.Cell(), prod.ProdutoId.ToString(PtBr), fundo);
					CelulaDado(table.Cell(), prod.ProdutoNome, fundo);
					CelulaDado(table.Cell(), prod.QuantidadeVendida.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(prod.FaturamentoTotal), fundo, alignRight: true);
					CelulaDado(table.Cell(), $"{FormatarDecimal(prod.ParticipacaoPercentual)}%", fundo, alignRight: true);
				}
			});
		});
	}

	private static void ComporClientesMaisAtivos(IContainer container, IReadOnlyList<RelatorioVendaClienteRankingDTO> clientes)
	{
		container.Column(col =>
		{
			col.Item().PaddingBottom(6).Text("Clientes Mais Ativos")
				.FontSize(12).Bold().FontColor("#1E3A5F");

			col.Item().Table(table =>
			{
				table.ColumnsDefinition(columns =>
				{
					columns.RelativeColumn(3f);
					columns.ConstantColumn(85);
					columns.ConstantColumn(85);
					columns.RelativeColumn(1.5f);
					columns.RelativeColumn(1.5f);
				});

				table.Header(header =>
				{
					CelulaCabecalho(header.Cell(), "Cliente");
					CelulaCabecalho(header.Cell(), "Concluídos", alignRight: true);
					CelulaCabecalho(header.Cell(), "Cancelados", alignRight: true);
					CelulaCabecalho(header.Cell(), "Total Comprado", alignRight: true);
					CelulaCabecalho(header.Cell(), "Ticket Médio", alignRight: true);
				});

				for (var i = 0; i < clientes.Count; i++)
				{
					var cliente = clientes[i];
					var fundo = i % 2 == 1 ? Colors.Grey.Lighten5 : Colors.White;

					CelulaDado(table.Cell(), cliente.ClienteNome, fundo);
					CelulaDado(table.Cell(), cliente.PedidosConcluidos.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), cliente.PedidosCancelados.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(cliente.ValorTotalComprado), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(cliente.TicketMedio), fundo, alignRight: true);
				}
			});
		});
	}

	private static void ComporTabelaPedidos(IContainer container, IReadOnlyList<RelatorioVendaPedidoItemDTO> pedidos)
	{
		container.Column(col =>
		{
			col.Item().PaddingBottom(6).Text("Detalhamento dos Pedidos")
				.FontSize(12).Bold().FontColor("#1E3A5F");

			col.Item().Table(table =>
			{
				table.ColumnsDefinition(columns =>
				{
					columns.ConstantColumn(48);
					columns.ConstantColumn(105);
					columns.RelativeColumn(2.6f);
					columns.ConstantColumn(85);
					columns.ConstantColumn(60);
					columns.ConstantColumn(65);
					columns.RelativeColumn(1.4f);
				});

				table.Header(header =>
				{
					CelulaCabecalho(header.Cell(), "Pedido");
					CelulaCabecalho(header.Cell(), "Data/Hora (UTC)");
					CelulaCabecalho(header.Cell(), "Cliente");
					CelulaCabecalho(header.Cell(), "Status");
					CelulaCabecalho(header.Cell(), "Itens", alignRight: true);
					CelulaCabecalho(header.Cell(), "Unidades", alignRight: true);
					CelulaCabecalho(header.Cell(), "Valor Total", alignRight: true);
				});

				for (var i = 0; i < pedidos.Count; i++)
				{
					var pedido = pedidos[i];
					var fundo = i % 2 == 1 ? Colors.Grey.Lighten5 : Colors.White;
					var corStatus = pedido.Status switch
					{
						Status.Concluido => "#15803D",
						Status.Cancelado => "#B91C1C",
						_ => "#B45309"
					};

					CelulaDado(table.Cell(), $"#{pedido.PedidoId}", fundo);
					CelulaDado(table.Cell(), pedido.DataPedido.ToString("dd/MM/yyyy HH:mm", PtBr), fundo);
					CelulaDado(table.Cell(), pedido.ClienteNome, fundo);
					table.Cell()
						.Background(fundo)
						.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3)
						.PaddingVertical(4).PaddingHorizontal(4)
						.Text(FormatarStatus(pedido.Status))
						.Bold().FontColor(corStatus);
					CelulaDado(table.Cell(), pedido.QuantidadeItens.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), pedido.TotalUnidades.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(pedido.ValorTotal), fundo, alignRight: true);
				}
			});
		});
	}

	private static void ComporRodape(IContainer container)
	{
		container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(6).Row(row =>
		{
			row.RelativeItem().Text("Gerenciador de Pedidos — Relatório de Vendas e Pedidos")
				.FontSize(8).FontColor(Colors.Grey.Darken1);

			row.ConstantItem(120).AlignRight().Text(text =>
			{
				text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
				text.Span("Página ");
				text.CurrentPageNumber();
				text.Span(" de ");
				text.TotalPages();
			});
		});
	}

	private static void CelulaCabecalho(IContainer cell, string texto, bool alignRight = false)
	{
		var c = cell
			.Background("#E8EEF5")
			.BorderBottom(1).BorderColor("#1E3A5F")
			.PaddingVertical(5).PaddingHorizontal(4);

		if (alignRight)
			c.AlignRight().Text(texto).Bold().FontSize(8.5f).FontColor("#1E3A5F");
		else
			c.Text(texto).Bold().FontSize(8.5f).FontColor("#1E3A5F");
	}

	private static void CelulaDado(IContainer cell, string texto, string background, bool alignRight = false)
	{
		var c = cell
			.Background(background)
			.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3)
			.PaddingVertical(4).PaddingHorizontal(4);

		if (alignRight)
			c.AlignRight().Text(texto).FontSize(8.5f);
		else
			c.Text(texto).FontSize(8.5f);
	}

	private static string ConstruirNomeArquivo(RelatorioVendasDTO relatorio, string extensao)
	{
		var periodoSlug = relatorio.Periodo switch
		{
			PeriodoRelatorio.Semana => "semana",
			PeriodoRelatorio.Mes => "mes",
			PeriodoRelatorio.Ano => "ano",
			_ => "periodo"
		};

		return $"relatorio-vendas-{periodoSlug}-{relatorio.DataInicio:yyyyMMdd}-{relatorio.DataFim:yyyyMMdd}.{extensao}";
	}

	private static string FormatarPeriodo(PeriodoRelatorio periodo)
		=> periodo switch
		{
			PeriodoRelatorio.Semana => "Semanal",
			PeriodoRelatorio.Mes => "Mensal",
			PeriodoRelatorio.Ano => "Anual",
			_ => periodo.ToString()
		};

	private static string FormatarStatus(Status status)
		=> status switch
		{
			Status.Pendente => "Pendente",
			Status.Processado => "Processado",
			Status.Concluido => "Concluído",
			Status.Cancelado => "Cancelado",
			_ => status.ToString()
		};

	private static string FormatarMoeda(decimal valor)
		=> CsvReportHelper.FormatCurrency(valor);

	private static string FormatarDecimal(decimal valor)
		=> valor.ToString("F2", PtBr);
}

