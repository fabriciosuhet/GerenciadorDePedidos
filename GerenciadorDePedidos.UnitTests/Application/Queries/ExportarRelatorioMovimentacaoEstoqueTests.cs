using System.Reflection;
using GerenciadorDePedidos.Application.Queries.ExportarRelatorioMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Validators;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Queries;

public class ExportarRelatorioMovimentacaoEstoqueTests
{
	[Fact]
	public void CalcularIntervalo_Semana_DeveRetornarSegundaADomingo()
	{
		// 2026-11-27 é sexta-feira; segunda-feira da semana é 2026-11-23
		var (inicio, fimExclusiva) = ExportarRelatorioMovimentacaoEstoqueQueryHandler.CalcularIntervalo(
			PeriodoRelatorio.Semana,
			new DateTime(2026, 11, 27, 15, 30, 0, DateTimeKind.Utc));

		Assert.Equal(new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), inicio);
		Assert.Equal(new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc), fimExclusiva);
	}

	[Fact]
	public void CalcularIntervalo_SemanaQuandoDomingo_DeveRetornarSemanaQueTerminaNoDomingo()
	{
		// 2026-11-29 é domingo; segunda-feira é 2026-11-23
		var (inicio, fimExclusiva) = ExportarRelatorioMovimentacaoEstoqueQueryHandler.CalcularIntervalo(
			PeriodoRelatorio.Semana,
			new DateTime(2026, 11, 29, 23, 59, 0, DateTimeKind.Utc));

		Assert.Equal(new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), inicio);
		Assert.Equal(new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc), fimExclusiva);
	}

	[Fact]
	public void CalcularIntervalo_Mes_DeveRetornarPrimeiroDiaAteProximoMes()
	{
		var (inicio, fimExclusiva) = ExportarRelatorioMovimentacaoEstoqueQueryHandler.CalcularIntervalo(
			PeriodoRelatorio.Mes,
			new DateTime(2026, 11, 27, 10, 0, 0, DateTimeKind.Utc));

		Assert.Equal(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), inicio);
		Assert.Equal(new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc), fimExclusiva);
	}

	[Fact]
	public void CalcularIntervalo_Ano_DeveRetornarAnoInteiro()
	{
		var (inicio, fimExclusiva) = ExportarRelatorioMovimentacaoEstoqueQueryHandler.CalcularIntervalo(
			PeriodoRelatorio.Ano,
			new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));

		Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), inicio);
		Assert.Equal(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), fimExclusiva);
	}

	[Fact]
	public async Task Handle_ComMovimentacoes_DeveConsolidarTotaisValoresEResumoPorProduto()
	{
		var admin = new Cliente("Admin Teste", "11999999999", Role.Admin);
		var notebook = new Produto("Notebook", 4000m, 12) { Id = 1 };
		var mouse = new Produto("Mouse", 100m, 30) { Id = 2 };

		var mov1 = CriarMovimentacao(10, 5, Tipo.Adicao, notebook, admin, new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc), 4000m, MotivoMovimentacao.AjusteManual);
		var mov2 = CriarMovimentacao(11, 2, Tipo.Remocao, notebook, admin, new DateTime(2026, 11, 12, 14, 0, 0, DateTimeKind.Utc), 4000m, MotivoMovimentacao.VendaPedido, 99);
		var mov3 = CriarMovimentacao(12, 10, Tipo.Adicao, mouse, admin, new DateTime(2026, 11, 15, 9, 0, 0, DateTimeKind.Utc), 100m, MotivoMovimentacao.AjusteManual);

		// Altera preço do notebook após as movimentações para validar que o relatório preserva o snapshot
		notebook.AlterarPreco(5500m);

		var repoMock = new Mock<IMovimentacaoEstoqueRepository>();
		repoMock
			.Setup(r => r.GetByPeriodoAsync(
				new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync([mov3, mov2, mov1]);

		RelatorioMovimentacaoEstoqueDTO? relatorioCapturado = null;
		var arquivoEsperado = new ArquivoRelatorioDTO([1, 2, 3], "application/pdf", "relatorio.pdf");

		var generatorMock = new Mock<IRelatorioMovimentacaoEstoqueGenerator>();
		generatorMock
			.Setup(g => g.Gerar(It.IsAny<RelatorioMovimentacaoEstoqueDTO>(), FormatoRelatorio.Pdf))
			.Callback<RelatorioMovimentacaoEstoqueDTO, FormatoRelatorio>((dto, _) => relatorioCapturado = dto)
			.Returns(arquivoEsperado);

		var handler = new ExportarRelatorioMovimentacaoEstoqueQueryHandler(repoMock.Object, generatorMock.Object);
		var result = await handler.Handle(
			new ExportarRelatorioMovimentacaoEstoqueQuery(
				PeriodoRelatorio.Mes,
				FormatoRelatorio.Pdf,
				new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc)),
			CancellationToken.None);

		Assert.Same(arquivoEsperado, result);
		Assert.NotNull(relatorioCapturado);
		Assert.Equal(PeriodoRelatorio.Mes, relatorioCapturado!.Periodo);
		Assert.Equal(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), relatorioCapturado.DataInicio);
		Assert.Equal(new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc), relatorioCapturado.DataFim);
		Assert.Equal(3, relatorioCapturado.TotalMovimentacoes);
		Assert.Equal(2, relatorioCapturado.TotalEntradas);
		Assert.Equal(1, relatorioCapturado.TotalSaidas);
		Assert.Equal(15, relatorioCapturado.QuantidadeTotalAdicionada);
		Assert.Equal(2, relatorioCapturado.QuantidadeTotalRemovida);
		Assert.Equal(13, relatorioCapturado.SaldoQuantidade);
		Assert.Equal(21000m, relatorioCapturado.ValorTotalEntradas); // (5 * 4000) + (10 * 100)
		Assert.Equal(8000m, relatorioCapturado.ValorTotalSaidas);     // 2 * 4000
		Assert.Equal(13000m, relatorioCapturado.SaldoFinanceiro);
		Assert.Equal(29000m, relatorioCapturado.ValorTotalMovimentado);

		Assert.Equal(2, relatorioCapturado.ResumoPorProduto.Count);
		var resumoNotebook = relatorioCapturado.ResumoPorProduto[0];
		Assert.Equal("Notebook", resumoNotebook.ProdutoNome);
		Assert.Equal(5, resumoNotebook.QuantidadeAdicionada);
		Assert.Equal(2, resumoNotebook.QuantidadeRemovida);
		Assert.Equal(3, resumoNotebook.SaldoQuantidade);
		Assert.Equal(20000m, resumoNotebook.ValorEntradas);
		Assert.Equal(8000m, resumoNotebook.ValorSaidas);
		Assert.Equal(28000m, resumoNotebook.ValorTotalMovimentado);
	}

	[Fact]
	public void Validator_ComValoresInvalidos_DeveFalhar()
	{
		var validator = new ExportarRelatorioMovimentacaoEstoqueQueryValidator();
		var result = validator.Validate(new ExportarRelatorioMovimentacaoEstoqueQuery(
			(PeriodoRelatorio)99,
			(FormatoRelatorio)99));

		Assert.False(result.IsValid);
		Assert.Equal(2, result.Errors.Count);
	}

	[Fact]
	public void Validator_ComValoresValidos_DevePassar()
	{
		var validator = new ExportarRelatorioMovimentacaoEstoqueQueryValidator();
		var result = validator.Validate(new ExportarRelatorioMovimentacaoEstoqueQuery(
			PeriodoRelatorio.Semana,
			FormatoRelatorio.Csv));

		Assert.True(result.IsValid);
	}

	private static MovimentacaoEstoque CriarMovimentacao(
		int id,
		int quantidade,
		Tipo tipo,
		Produto produto,
		Cliente cliente,
		DateTime dataUtc,
		decimal precoUnitario = 0m,
		MotivoMovimentacao motivo = MotivoMovimentacao.AjusteManual,
		int? pedidoId = null)
	{
		var mov = new MovimentacaoEstoque(quantidade, tipo, produto.Id, cliente.Id, precoUnitario, motivo, pedidoId) { Id = id };
		typeof(MovimentacaoEstoque)
			.GetProperty(nameof(MovimentacaoEstoque.Produto), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
			.SetValue(mov, produto);
		typeof(MovimentacaoEstoque)
			.GetProperty(nameof(MovimentacaoEstoque.Cliente), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
			.SetValue(mov, cliente);
		typeof(MovimentacaoEstoque)
			.GetProperty(nameof(MovimentacaoEstoque.DataMovimentacao), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
			.SetValue(mov, dataUtc);
		return mov;
	}
}

