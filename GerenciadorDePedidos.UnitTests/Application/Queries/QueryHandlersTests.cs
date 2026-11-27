using GerenciadorDePedidos.Application.Queries.GetAllClientes;
using GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetAllPedidos;
using GerenciadorDePedidos.Application.Queries.GetAllProdutos;
using GerenciadorDePedidos.Application.Queries.GetCliente;
using GerenciadorDePedidos.Application.Queries.GetDashboard;
using GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetPedido;
using GerenciadorDePedidos.Application.Queries.GetProduto;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Queries;

public class QueryHandlersTests
{
	private static ICurrentUserService AdminUser()
	{
		var mock = new Mock<ICurrentUserService>();
		mock.Setup(u => u.IsAdmin).Returns(true);
		mock.Setup(u => u.ClienteId).Returns(Guid.NewGuid());
		return mock.Object;
	}

	private static ICurrentUserService CommonUser(Guid clienteId)
	{
		var mock = new Mock<ICurrentUserService>();
		mock.Setup(u => u.IsAdmin).Returns(false);
		mock.Setup(u => u.ClienteId).Returns(clienteId);
		return mock.Object;
	}

	[Fact]
	public async Task GetAllClientes_QuandoVazio_DeveRetornarListaVazia()
	{
		var repoMock = new Mock<IClienteRepository>();
		repoMock.Setup(r => r.GetCountAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(0);

		var handler = new GetAllClientesQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetAllClientesQuery(), CancellationToken.None);

		Assert.Empty(result.Items);
		Assert.Equal(0, result.TotalCount);
	}

	[Fact]
	public async Task GetAllClientes_ComRegistros_DeveRetornarPaginado()
	{
		var clientes = new List<Cliente> { new("Ana", "11999999999", Role.Usuario) };
		var repoMock = new Mock<IClienteRepository>();
		repoMock.Setup(r => r.GetCountAsync("Ana", It.IsAny<CancellationToken>())).ReturnsAsync(1);
		repoMock.Setup(r => r.GetPagedAsync("Ana", 0, 10, It.IsAny<CancellationToken>())).ReturnsAsync(clientes);

		var handler = new GetAllClientesQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetAllClientesQuery("Ana"), CancellationToken.None);

		Assert.Single(result.Items);
		Assert.Equal("Ana", result.Items[0].NomeCompleto);
	}

	[Fact]
	public async Task GetCliente_NaoEncontrado_DeveRetornarNull()
	{
		var repoMock = new Mock<IClienteRepository>();
		repoMock.Setup(r => r.GetDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

		var handler = new GetClienteQueryHandler(repoMock.Object, AdminUser());
		var result = await handler.Handle(new GetClienteQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.Null(result);
	}

	[Fact]
	public async Task GetCliente_UsuarioComumConsultandoOutro_DeveLancarUnauthorizedAccessException()
	{
		var handler = new GetClienteQueryHandler(new Mock<IClienteRepository>().Object, CommonUser(Guid.NewGuid()));
		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new GetClienteQuery(Guid.NewGuid()), CancellationToken.None));
	}

	[Fact]
	public async Task GetAllProdutos_ComFiltro_DeveRetornarProdutosPaginados()
	{
		var produtos = new List<Produto> { new("Mouse", 150m, 10) { Id = 1 } };
		var repoMock = new Mock<IProdutoRepository>();
		repoMock.Setup(r => r.GetCountAsync("Mouse", It.IsAny<CancellationToken>())).ReturnsAsync(1);
		repoMock.Setup(r => r.GetPagedAsync("Mouse", 0, 10, It.IsAny<CancellationToken>())).ReturnsAsync(produtos);

		var handler = new GetAllProdutosQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetAllProdutosQuery("Mouse"), CancellationToken.None);

		Assert.Single(result.Items);
		Assert.Equal("Mouse", result.Items[0].Nome);
		Assert.Equal(1, result.TotalCount);
	}

	[Fact]
	public async Task GetProduto_Encontrado_DeveRetornarDetalhes()
	{
		var produto = new Produto("Teclado", 200m, 5) { Id = 1 };
		var repoMock = new Mock<IRepository<Produto, int>>();
		repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var handler = new GetProdutoQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetProdutoQuery(1), CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal("Teclado", result.Nome);
		Assert.Equal(200m, result.Preco);
	}

	[Fact]
	public async Task GetAllPedidos_UsuarioComum_DeveFiltrarPeloProprioClienteId()
	{
		var clienteId = Guid.NewGuid();
		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 1 };
		pedido.AdicionarItem(new Produto("Monitor", 1000m, 5) { Id = 10 }, 2);

		var repoMock = new Mock<IPedidoRepository>();
		repoMock.Setup(r => r.GetCountAsync(null, clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(1);
		repoMock.Setup(r => r.GetPagedAsync(null, 0, 10, clienteId, It.IsAny<CancellationToken>())).ReturnsAsync([pedido]);

		var handler = new GetAllPedidosQueryHandler(repoMock.Object, CommonUser(clienteId));
		var result = await handler.Handle(new GetAllPedidosQuery(), CancellationToken.None);

		Assert.Single(result.Items);
		Assert.Equal(2000m, result.Items[0].Total);
	}

	[Fact]
	public async Task GetPedido_DonoDoPedido_DeveMapearItens()
	{
		var clienteId = Guid.NewGuid();
		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 1 };
		pedido.AdicionarItem(new Produto("Cabo", 25m, 10) { Id = 2 }, 3);

		var repoMock = new Mock<IPedidoRepository>();
		repoMock.Setup(r => r.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var handler = new GetPedidoQueryHandler(repoMock.Object, CommonUser(clienteId));
		var result = await handler.Handle(new GetPedidoQuery(1), CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(75m, result.Total);
		Assert.Single(result.ItensPedidos);
	}

	[Fact]
	public async Task GetPedido_UsuarioConsultandoPedidoDeOutro_DeveLancarUnauthorizedAccessException()
	{
		var pedido = new Pedido(Guid.NewGuid(), Status.Pendente) { Id = 1 };
		var repoMock = new Mock<IPedidoRepository>();
		repoMock.Setup(r => r.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var handler = new GetPedidoQueryHandler(repoMock.Object, CommonUser(Guid.NewGuid()));
		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new GetPedidoQuery(1), CancellationToken.None));
	}

	[Fact]
	public async Task GetDashboard_DeveRetornarMetricasConsolidadas()
	{
		var clienteRepo = new Mock<IClienteRepository>();
		clienteRepo.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

		var produtoRepo = new Mock<IProdutoRepository>();
		produtoRepo.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);
		produtoRepo.Setup(r => r.GetLowStockCountAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(2);

		var topProdutos = new List<TopProdutoDashboardDTO>
		{
			new(1, "Notebook Pro", 4, 10000m),
			new(2, "Mouse", 10, 2500m)
		};

		var pedidoRepo = new Mock<IPedidoRepository>();
		pedidoRepo.Setup(r => r.GetDashboardStatsAsync(null, null, 5, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PedidoDashboardStatsDTO(20, 3, 15, 2, 12500m, topProdutos));

		var handler = new GetDashboardQueryHandler(clienteRepo.Object, produtoRepo.Object, pedidoRepo.Object);
		var result = await handler.Handle(new GetDashboardQuery(), CancellationToken.None);

		Assert.Equal(5, result.TotalClientes);
		Assert.Equal(12, result.TotalProdutos);
		Assert.Equal(2, result.ProdutosComEstoqueBaixo);
		Assert.Equal(20, result.TotalPedidos);
		Assert.Equal(15, result.PedidosConcluidos);
		Assert.Equal(12500m, result.FaturamentoTotalConcluido);
		Assert.Equal(833.33m, result.TicketMedio);
		Assert.Equal(10m, result.TaxaCancelamentoPercentual);
		Assert.NotNull(result.TopProdutos);
		Assert.Equal(2, result.TopProdutos!.Count);
		Assert.Equal("Notebook Pro", result.TopProdutos[0].ProdutoNome);
	}

	[Fact]
	public async Task GetDashboard_ComFiltroDePeriodo_DevePassarIntervaloParaRepositorio()
	{
		var clienteRepo = new Mock<IClienteRepository>();
		clienteRepo.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);

		var produtoRepo = new Mock<IProdutoRepository>();
		produtoRepo.Setup(r => r.GetCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(8);
		produtoRepo.Setup(r => r.GetLowStockCountAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(1);

		var pedidoRepo = new Mock<IPedidoRepository>();
		pedidoRepo.Setup(r => r.GetDashboardStatsAsync(
				new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
				3,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PedidoDashboardStatsDTO(4, 0, 3, 1, 3000m, []));

		var handler = new GetDashboardQueryHandler(clienteRepo.Object, produtoRepo.Object, pedidoRepo.Object);
		var result = await handler.Handle(
			new GetDashboardQuery(PeriodoRelatorio.Mes, new DateTime(2026, 11, 15, 0, 0, 0, DateTimeKind.Utc), 3),
			CancellationToken.None);

		Assert.Equal(PeriodoRelatorio.Mes, result.Periodo);
		Assert.Equal(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), result.DataInicio);
		Assert.Equal(new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc), result.DataFim);
		Assert.Equal(1000m, result.TicketMedio);
		Assert.Equal(25m, result.TaxaCancelamentoPercentual);
	}

	[Fact]
	public async Task GetAllMovimentacoes_QuandoVazio_DeveRetornarListaVazia()
	{
		var repoMock = new Mock<IMovimentacaoEstoqueRepository>();
		repoMock.Setup(r => r.GetCountAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(0);

		var handler = new GetAllMovimentacaoEstoqueQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetAllMovimentacaoEstoqueQuery(), CancellationToken.None);

		Assert.Empty(result.Items);
		Assert.Equal(0, result.TotalCount);
	}

	[Fact]
	public async Task GetMovimentacao_NaoEncontrada_DeveRetornarNull()
	{
		var repoMock = new Mock<IMovimentacaoEstoqueRepository>();
		repoMock.Setup(r => r.GetByIdMovimentacaoAsync(99, It.IsAny<CancellationToken>()))
			.ReturnsAsync((MovimentacaoEstoque?)null);

		var handler = new GetMovimentacaoEstoqueQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetMovimentacaoEstoqueQuery(99), CancellationToken.None);

		Assert.Null(result);
	}
}
