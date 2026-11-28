using System.Reflection;
using GerenciadorDePedidos.Application.Queries.ExportarRelatorioVendas;
using GerenciadorDePedidos.Application.Validators;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Queries;

public class ExportarRelatorioVendasTests
{
	[Fact]
	public async Task Handle_ComPedidosNoPeriodo_DeveConsolidarMetricasRankingsEChamarGerador()
	{
		var maria = new Cliente("Maria Silva", "11999999991", Role.Usuario);
		var joao = new Cliente("João Souza", "11999999992", Role.Usuario);
		var notebook = new Produto("Notebook", 4000m, 10) { Id = 1 };
		var mouse = new Produto("Mouse", 200m, 20) { Id = 2 };

		// Pedido 1 (Maria, Concluído): 1 Notebook (4000) + 2 Mouses (400) = 4400
		var p1 = CriarPedido(1, maria, Status.Concluido, new DateTime(2026, 11, 5, 10, 0, 0, DateTimeKind.Utc),
			(notebook, 1), (mouse, 2));
		// Pedido 2 (João, Concluído): 3 Mouses = 600
		var p2 = CriarPedido(2, joao, Status.Concluido, new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc),
			(mouse, 3));
		// Pedido 3 (João, Cancelado): 1 Notebook = 4000
		var p3 = CriarPedido(3, joao, Status.Cancelado, new DateTime(2026, 11, 15, 16, 0, 0, DateTimeKind.Utc),
			(notebook, 1));

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock
			.Setup(r => r.GetByPeriodoAsync(
				new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync([p3, p2, p1]);

		RelatorioVendasDTO? capturado = null;
		var arquivoEsperado = new ArquivoRelatorioDTO([1, 2, 3], "application/pdf", "relatorio-vendas.pdf");

		var generatorMock = new Mock<IRelatorioVendasGenerator>();
		generatorMock
			.Setup(g => g.Gerar(It.IsAny<RelatorioVendasDTO>(), FormatoRelatorio.Pdf))
			.Callback<RelatorioVendasDTO, FormatoRelatorio>((dto, _) => capturado = dto)
			.Returns(arquivoEsperado);

		var handler = new ExportarRelatorioVendasQueryHandler(pedidoRepoMock.Object, generatorMock.Object);
		var result = await handler.Handle(
			new ExportarRelatorioVendasQuery(
				PeriodoRelatorio.Mes,
				FormatoRelatorio.Pdf,
				new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc)),
			CancellationToken.None);

		Assert.Same(arquivoEsperado, result);
		Assert.NotNull(capturado);
		Assert.Equal(3, capturado!.TotalPedidos);
		Assert.Equal(2, capturado.PedidosConcluidos);
		Assert.Equal(1, capturado.PedidosCancelados);
		Assert.Equal(6, capturado.TotalUnidadesVendidas); // 1 + 2 + 3
		Assert.Equal(5000m, capturado.FaturamentoTotalConcluido); // 4400 + 600
		Assert.Equal(4000m, capturado.ValorTotalCancelado);
		Assert.Equal(2500m, capturado.TicketMedio);
		Assert.Equal(33.33m, capturado.TaxaCancelamentoPercentual);

		// Top Produtos: Notebook (4000 = 80%), Mouse (1000 = 20%)
		Assert.Equal(2, capturado.TopProdutos.Count);
		Assert.Equal("Notebook", capturado.TopProdutos[0].ProdutoNome);
		Assert.Equal(4000m, capturado.TopProdutos[0].FaturamentoTotal);
		Assert.Equal(80m, capturado.TopProdutos[0].ParticipacaoPercentual);
		Assert.Equal("Mouse", capturado.TopProdutos[1].ProdutoNome);
		Assert.Equal(5, capturado.TopProdutos[1].QuantidadeVendida);
		Assert.Equal(20m, capturado.TopProdutos[1].ParticipacaoPercentual);

		// Clientes mais ativos: Maria (4400) > João (600, 1 cancelado)
		Assert.Equal(2, capturado.ClientesMaisAtivos.Count);
		Assert.Equal("Maria Silva", capturado.ClientesMaisAtivos[0].ClienteNome);
		Assert.Equal(4400m, capturado.ClientesMaisAtivos[0].ValorTotalComprado);
		Assert.Equal("João Souza", capturado.ClientesMaisAtivos[1].ClienteNome);
		Assert.Equal(1, capturado.ClientesMaisAtivos[1].PedidosCancelados);
	}

	[Fact]
	public void Validator_DeveValidarEnumsDePeriodoEFormato()
	{
		var validator = new ExportarRelatorioVendasQueryValidator();
		Assert.True(validator.Validate(new ExportarRelatorioVendasQuery(PeriodoRelatorio.Semana, FormatoRelatorio.Csv)).IsValid);
		Assert.False(validator.Validate(new ExportarRelatorioVendasQuery((PeriodoRelatorio)99, (FormatoRelatorio)99)).IsValid);
	}

	private static Pedido CriarPedido(
		int id,
		Cliente cliente,
		Status statusFinal,
		DateTime dataUtc,
		params (Produto Produto, int Quantidade)[] itens)
	{
		var pedido = new Pedido(cliente.Id, Status.Pendente) { Id = id };
		foreach (var (produto, quantidade) in itens)
		{
			pedido.AdicionarItem(produto, quantidade);
			var item = pedido.ItensPedidos.Last();
			typeof(ItemPedido).GetProperty(nameof(ItemPedido.Produto), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
				.SetValue(item, produto);
		}

		if (statusFinal == Status.Concluido)
		{
			pedido.MarcarComoProcessado();
			pedido.Concluir();
		}
		else if (statusFinal == Status.Cancelado)
		{
			pedido.Cancelar();
		}
		else if (statusFinal == Status.Devolvido)
		{
			pedido.MarcarComoProcessado();
			pedido.Concluir();
			pedido.Devolver("Devolução de teste");
		}

		typeof(Pedido).GetProperty(nameof(Pedido.Cliente), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
			.SetValue(pedido, cliente);
		typeof(Pedido).GetProperty(nameof(Pedido.DataPedido), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
			.SetValue(pedido, dataUtc);

		return pedido;
	}
}

