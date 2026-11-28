using System.Globalization;
using System.Text;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GerenciadorDePedidos.Infrastructure.Reports;

public sealed class RelatorioMovimentacaoEstoqueGenerator : IRelatorioMovimentacaoEstoqueGenerator
{
	private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

	static RelatorioMovimentacaoEstoqueGenerator()
	{
		QuestPDF.Settings.License = LicenseType.Community;
	}

	public ArquivoRelatorioDTO Gerar(RelatorioMovimentacaoEstoqueDTO relatorio, FormatoRelatorio formato)
		=> formato switch
		{
			FormatoRelatorio.Csv => GerarCsv(relatorio),
			FormatoRelatorio.Pdf => GerarPdf(relatorio),
			_ => throw new ArgumentOutOfRangeException(nameof(formato), formato, "Formato de relatório inválido.")
		};

	private static ArquivoRelatorioDTO GerarCsv(RelatorioMovimentacaoEstoqueDTO relatorio)
	{
		const int totalColunas = 12;
		var sb = new StringBuilder();

		// Cabeçalho do relatório
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "RELATÓRIO DE MOVIMENTAÇÃO DE ESTOQUE");
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

		// Seção 1: Resumo do Período
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "RESUMO DO PERÍODO");
		CsvReportHelper.AppendRow(sb, totalColunas, "Indicador", "Valor", "Detalhamento");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Total de Movimentações",
			CsvReportHelper.FormatInteger(relatorio.TotalMovimentacoes),
			$"{relatorio.TotalEntradas} entrada(s) | {relatorio.TotalSaidas} saída(s)");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Unidades Adicionadas",
			CsvReportHelper.FormatInteger(relatorio.QuantidadeTotalAdicionada),
			"Entradas registradas no período");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Unidades Removidas",
			CsvReportHelper.FormatInteger(relatorio.QuantidadeTotalRemovida),
			"Saídas registradas no período");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Saldo de Quantidade",
			CsvReportHelper.FormatInteger(relatorio.SaldoQuantidade),
			"Entradas menos saídas");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Valor Total de Entradas",
			CsvReportHelper.FormatCurrency(relatorio.ValorTotalEntradas),
			"Soma financeira das entradas");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Valor Total de Saídas",
			CsvReportHelper.FormatCurrency(relatorio.ValorTotalSaidas),
			"Soma financeira das saídas");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Saldo Financeiro",
			CsvReportHelper.FormatCurrency(relatorio.SaldoFinanceiro),
			"Entradas menos saídas");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Valor Total Movimentado",
			CsvReportHelper.FormatCurrency(relatorio.ValorTotalMovimentado),
			"Volume financeiro total movimentado");
		CsvReportHelper.AppendBlankRow(sb, totalColunas);

		// Seção 2: Resumo por Produto
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "RESUMO POR PRODUTO");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"Cód. Produto",
			"Produto",
			"Preço Unitário",
			"Estoque Atual",
			"Qtd. Adicionada",
			"Qtd. Removida",
			"Saldo Qtd.",
			"Valor Entradas",
			"Valor Saídas",
			"Valor Movimentado");

		if (relatorio.ResumoPorProduto.Count == 0)
		{
			CsvReportHelper.AppendRow(sb, totalColunas, "Sem produtos movimentados no período.");
		}
		else
		{
			foreach (var prod in relatorio.ResumoPorProduto)
			{
				CsvReportHelper.AppendRow(
					sb,
					totalColunas,
					prod.ProdutoId.ToString(PtBr),
					CsvReportHelper.Escape(prod.ProdutoNome),
					CsvReportHelper.FormatCurrency(prod.PrecoUnitario),
					CsvReportHelper.FormatInteger(prod.EstoqueAtual),
					CsvReportHelper.FormatInteger(prod.QuantidadeAdicionada),
					CsvReportHelper.FormatInteger(prod.QuantidadeRemovida),
					CsvReportHelper.FormatInteger(prod.SaldoQuantidade),
					CsvReportHelper.FormatCurrency(prod.ValorEntradas),
					CsvReportHelper.FormatCurrency(prod.ValorSaidas),
					CsvReportHelper.FormatCurrency(prod.ValorTotalMovimentado));
			}

			CsvReportHelper.AppendRow(
				sb,
				totalColunas,
				"TOTAL",
				$"{relatorio.ResumoPorProduto.Count} produto(s)",
				string.Empty,
				string.Empty,
				CsvReportHelper.FormatInteger(relatorio.QuantidadeTotalAdicionada),
				CsvReportHelper.FormatInteger(relatorio.QuantidadeTotalRemovida),
				CsvReportHelper.FormatInteger(relatorio.SaldoQuantidade),
				CsvReportHelper.FormatCurrency(relatorio.ValorTotalEntradas),
				CsvReportHelper.FormatCurrency(relatorio.ValorTotalSaidas),
				CsvReportHelper.FormatCurrency(relatorio.ValorTotalMovimentado));
		}

		CsvReportHelper.AppendBlankRow(sb, totalColunas);

		// Seção 3: Movimentações Detalhadas
		CsvReportHelper.AppendSectionHeader(sb, totalColunas, "MOVIMENTAÇÕES DETALHADAS");
		CsvReportHelper.AppendRow(
			sb,
			totalColunas,
			"ID",
			"Data/Hora (Brasília)",
			"Tipo",
			"Motivo",
			"Nº Pedido",
			"Cód. Produto",
			"Produto",
			"Responsável",
			"Quantidade",
			"Preço Unitário",
			"Valor Total",
			"Estoque Atual");

		if (relatorio.Movimentacoes.Count == 0)
		{
			CsvReportHelper.AppendRow(sb, totalColunas, "Sem movimentações registradas no período.");
		}
		else
		{
			foreach (var item in relatorio.Movimentacoes)
			{
				CsvReportHelper.AppendRow(
					sb,
					totalColunas,
					item.Id.ToString(PtBr),
					CsvReportHelper.FormatDateTimeBrasilia(item.DataMovimentacao),
					CsvReportHelper.Escape(FormatarTipo(item.TipoMovimentacao)),
					CsvReportHelper.Escape(FormatarMotivo(item.Motivo)),
					item.PedidoId?.ToString(PtBr) ?? "-",
					item.ProdutoId.ToString(PtBr),
					CsvReportHelper.Escape(item.ProdutoNome),
					CsvReportHelper.Escape(item.ClienteNome),
					CsvReportHelper.FormatInteger(item.Quantidade),
					CsvReportHelper.FormatCurrency(item.PrecoUnitario),
					CsvReportHelper.FormatCurrency(item.ValorTotal),
					CsvReportHelper.FormatInteger(item.EstoqueAtualProduto));
			}

			CsvReportHelper.AppendRow(
				sb,
				totalColunas,
				"TOTAL",
				$"{relatorio.Movimentacoes.Count} movimentação(ões)",
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				CsvReportHelper.FormatInteger(relatorio.Movimentacoes.Sum(m => m.Quantidade)),
				string.Empty,
				CsvReportHelper.FormatCurrency(relatorio.ValorTotalMovimentado));
		}

		return new ArquivoRelatorioDTO(
			CsvReportHelper.ToUtf8WithBom(sb.ToString()),
			"text/csv; charset=utf-8",
			ConstruirNomeArquivo(relatorio, "csv"));
	}

	private static ArquivoRelatorioDTO GerarPdf(RelatorioMovimentacaoEstoqueDTO relatorio)
	{
		var bytes = Document.Create(container =>
		{
			container.Page(page =>
			{
				page.Size(PageSizes.A4.Landscape());
				page.Margin(28);
				page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken4));

				page.Header().Element(header => ComporCabecalho(header, relatorio));
				page.Content().PaddingVertical(10).Element(content => ComporConteudo(content, relatorio));
				page.Footer().Element(ComporRodape);
			});
		}).GeneratePdf();

		return new ArquivoRelatorioDTO(
			bytes,
			"application/pdf",
			ConstruirNomeArquivo(relatorio, "pdf"));
	}

	private static void ComporCabecalho(IContainer container, RelatorioMovimentacaoEstoqueDTO relatorio)
	{
		container.Background("#1E3A5F").Padding(14).Row(row =>
		{
			row.RelativeItem().Column(col =>
			{
				col.Item().Text("Relatório de Movimentações de Estoque")
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

	private static void ComporConteudo(IContainer container, RelatorioMovimentacaoEstoqueDTO relatorio)
	{
		container.Column(col =>
		{
			col.Spacing(14);

			col.Item().Element(c => ComporResumoExecutivo(c, relatorio));

			if (relatorio.Movimentacoes.Count == 0)
			{
				col.Item()
					.Border(1).BorderColor(Colors.Grey.Lighten2)
					.Background(Colors.Grey.Lighten4)
					.Padding(16)
					.AlignCenter()
					.Text("Nenhuma movimentação de estoque registrada no período selecionado.")
					.FontSize(11).Italic().FontColor(Colors.Grey.Darken2);
				return;
			}

			col.Item().Element(c => ComporResumoPorProduto(c, relatorio.ResumoPorProduto));
			col.Item().Element(c => ComporTabelaMovimentacoes(c, relatorio.Movimentacoes));
		});
	}

	private static void ComporResumoExecutivo(IContainer container, RelatorioMovimentacaoEstoqueDTO relatorio)
	{
		container.Column(col =>
		{
			col.Item().PaddingBottom(6).Text("Resumo Executivo")
				.FontSize(12).Bold().FontColor("#1E3A5F");

			col.Item().Row(row =>
			{
				row.Spacing(8);
				AdicionarCard(row, "Movimentações", relatorio.TotalMovimentacoes.ToString(PtBr),
					$"{relatorio.TotalEntradas} entradas / {relatorio.TotalSaidas} saídas");
				AdicionarCard(row, "Unidades (Saldo)", relatorio.SaldoQuantidade.ToString("+#;-#;0", PtBr),
					$"+{relatorio.QuantidadeTotalAdicionada} / -{relatorio.QuantidadeTotalRemovida}");
				AdicionarCard(row, "Valor Entradas", FormatarMoeda(relatorio.ValorTotalEntradas),
					"Custo/valor de reposição", "#15803D");
				AdicionarCard(row, "Valor Saídas", FormatarMoeda(relatorio.ValorTotalSaidas),
					"Valor movimentado em saídas", "#B91C1C");
				AdicionarCard(row, "Saldo Financeiro", FormatarMoeda(relatorio.SaldoFinanceiro),
					$"Total girado: {FormatarMoeda(relatorio.ValorTotalMovimentado)}", "#1E3A5F");
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

	private static void ComporResumoPorProduto(IContainer container, IReadOnlyList<ResumoProdutoMovimentacaoDTO> produtos)
	{
		container.Column(col =>
		{
			col.Item().PaddingBottom(6).Text("Resumo por Produto")
				.FontSize(12).Bold().FontColor("#1E3A5F");

			col.Item().Table(table =>
			{
				table.ColumnsDefinition(columns =>
				{
					columns.ConstantColumn(45);
					columns.RelativeColumn(3);
					columns.RelativeColumn(1.2f);
					columns.ConstantColumn(60);
					columns.ConstantColumn(55);
					columns.ConstantColumn(55);
					columns.ConstantColumn(55);
					columns.RelativeColumn(1.3f);
					columns.RelativeColumn(1.3f);
				});

				table.Header(header =>
				{
					CelulaCabecalho(header.Cell(), "ID");
					CelulaCabecalho(header.Cell(), "Produto");
					CelulaCabecalho(header.Cell(), "Preço Unit.", alignRight: true);
					CelulaCabecalho(header.Cell(), "Estoque", alignRight: true);
					CelulaCabecalho(header.Cell(), "Entradas", alignRight: true);
					CelulaCabecalho(header.Cell(), "Saídas", alignRight: true);
					CelulaCabecalho(header.Cell(), "Saldo", alignRight: true);
					CelulaCabecalho(header.Cell(), "Valor Entradas", alignRight: true);
					CelulaCabecalho(header.Cell(), "Valor Saídas", alignRight: true);
				});

				for (var i = 0; i < produtos.Count; i++)
				{
					var prod = produtos[i];
					var fundo = i % 2 == 1 ? Colors.Grey.Lighten5 : Colors.White;

					CelulaDado(table.Cell(), prod.ProdutoId.ToString(PtBr), fundo);
					CelulaDado(table.Cell(), prod.ProdutoNome, fundo);
					CelulaDado(table.Cell(), FormatarMoeda(prod.PrecoUnitario), fundo, alignRight: true);
					CelulaDado(table.Cell(), prod.EstoqueAtual.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), prod.QuantidadeAdicionada.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), prod.QuantidadeRemovida.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), prod.SaldoQuantidade.ToString("+#;-#;0", PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(prod.ValorEntradas), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(prod.ValorSaidas), fundo, alignRight: true);
				}
			});
		});
	}

	private static void ComporTabelaMovimentacoes(
		IContainer container,
		IReadOnlyList<MovimentacaoEstoqueRelatorioItemDTO> movimentacoes)
	{
		container.Column(col =>
		{
			col.Item().PaddingBottom(6).Text("Detalhamento das Movimentações")
				.FontSize(12).Bold().FontColor("#1E3A5F");

			col.Item().Table(table =>
			{
				table.ColumnsDefinition(columns =>
				{
					columns.ConstantColumn(34);
					columns.ConstantColumn(86);
					columns.ConstantColumn(52);
					columns.ConstantColumn(78);
					columns.ConstantColumn(44);
					columns.RelativeColumn(2.1f);
					columns.RelativeColumn(1.7f);
					columns.ConstantColumn(40);
					columns.RelativeColumn(1.1f);
					columns.RelativeColumn(1.2f);
					columns.ConstantColumn(48);
				});

				table.Header(header =>
				{
					CelulaCabecalho(header.Cell(), "ID");
					CelulaCabecalho(header.Cell(), "Data/Hora (UTC)");
					CelulaCabecalho(header.Cell(), "Tipo");
					CelulaCabecalho(header.Cell(), "Motivo");
					CelulaCabecalho(header.Cell(), "Pedido");
					CelulaCabecalho(header.Cell(), "Produto");
					CelulaCabecalho(header.Cell(), "Responsável");
					CelulaCabecalho(header.Cell(), "Qtd.", alignRight: true);
					CelulaCabecalho(header.Cell(), "Preço Unit.", alignRight: true);
					CelulaCabecalho(header.Cell(), "Valor Total", alignRight: true);
					CelulaCabecalho(header.Cell(), "Est. Atual", alignRight: true);
				});

				for (var i = 0; i < movimentacoes.Count; i++)
				{
					var item = movimentacoes[i];
					var fundo = i % 2 == 1 ? Colors.Grey.Lighten5 : Colors.White;
					var corTipo = item.TipoMovimentacao == Tipo.Adicao ? "#15803D" : "#B91C1C";

					CelulaDado(table.Cell(), item.Id.ToString(PtBr), fundo);
					CelulaDado(table.Cell(), item.DataMovimentacao.ToString("dd/MM/yyyy HH:mm", PtBr), fundo);
					table.Cell()
						.Background(fundo)
						.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3)
						.PaddingVertical(4).PaddingHorizontal(4)
						.Text(FormatarTipo(item.TipoMovimentacao))
						.Bold().FontColor(corTipo);
					CelulaDado(table.Cell(), FormatarMotivo(item.Motivo), fundo);
					CelulaDado(table.Cell(), item.PedidoId.HasValue ? $"#{item.PedidoId}" : "-", fundo);
					CelulaDado(table.Cell(), item.ProdutoNome, fundo);
					CelulaDado(table.Cell(), item.ClienteNome, fundo);
					CelulaDado(table.Cell(), item.Quantidade.ToString(PtBr), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(item.PrecoUnitario), fundo, alignRight: true);
					CelulaDado(table.Cell(), FormatarMoeda(item.ValorTotal), fundo, alignRight: true);
					CelulaDado(table.Cell(), item.EstoqueAtualProduto.ToString(PtBr), fundo, alignRight: true);
				}
			});
		});
	}

	private static void ComporRodape(IContainer container)
	{
		container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(6).Row(row =>
		{
			row.RelativeItem().Text("Gerenciador de Pedidos — Relatório de Movimentações de Estoque")
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

	private static string ConstruirNomeArquivo(RelatorioMovimentacaoEstoqueDTO relatorio, string extensao)
	{
		var periodoSlug = relatorio.Periodo switch
		{
			PeriodoRelatorio.Semana => "semana",
			PeriodoRelatorio.Mes => "mes",
			PeriodoRelatorio.Ano => "ano",
			_ => "periodo"
		};

		return $"relatorio-movimentacoes-estoque-{periodoSlug}-{relatorio.DataInicio:yyyyMMdd}-{relatorio.DataFim:yyyyMMdd}.{extensao}";
	}

	private static string FormatarPeriodo(PeriodoRelatorio periodo)
		=> periodo switch
		{
			PeriodoRelatorio.Semana => "Semanal",
			PeriodoRelatorio.Mes => "Mensal",
			PeriodoRelatorio.Ano => "Anual",
			_ => periodo.ToString()
		};

	private static string FormatarTipo(Tipo tipo)
		=> tipo == Tipo.Adicao ? "Entrada" : "Saída";

	private static string FormatarMotivo(MotivoMovimentacao motivo)
		=> motivo switch
		{
			MotivoMovimentacao.AjusteManual => "Ajuste Manual",
			MotivoMovimentacao.VendaPedido => "Venda (Pedido)",
			MotivoMovimentacao.EstornoCancelamento => "Estorno",
			MotivoMovimentacao.Devolucao => "Devolução",
			_ => motivo.ToString()
		};

	private static string FormatarMoeda(decimal valor)
		=> CsvReportHelper.FormatCurrency(valor);
}

