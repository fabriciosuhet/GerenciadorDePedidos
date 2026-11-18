using GerenciadorDePedidos.Application.Queries.GetAllClientes;
using GerenciadorDePedidos.Application.Queries.GetAllMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetAllPedidos;
using GerenciadorDePedidos.Application.Queries.GetAllProdutos;
using GerenciadorDePedidos.Application.Queries.GetCliente;
using GerenciadorDePedidos.Application.Queries.GetMovimentacaoEstoque;
using GerenciadorDePedidos.Application.Queries.GetPedido;
using GerenciadorDePedidos.Application.Queries.GetProduto;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Queries;

public class QueryHandlersTests
{
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

		var handler = new GetClienteQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetClienteQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.Null(result);
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
	public async Task GetAllPedidos_ComRegistros_DeveCalcularTotalCorretamente()
	{
		var pedido = new Pedido(Guid.NewGuid(), Status.Pendente) { Id = 1 };
		pedido.AdicionarItem(new Produto("Monitor", 1000m, 5) { Id = 10 }, 2);

		var repoMock = new Mock<IPedidoRepository>();
		repoMock.Setup(r => r.GetCountAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(1);
		repoMock.Setup(r => r.GetPagedAsync(null, 0, 10, It.IsAny<CancellationToken>())).ReturnsAsync([pedido]);

		var handler = new GetAllPedidosQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetAllPedidosQuery(), CancellationToken.None);

		Assert.Single(result.Items);
		Assert.Equal(2000m, result.Items[0].Total);
	}

	[Fact]
	public async Task GetPedido_Encontrado_DeveMapearItens()
	{
		var pedido = new Pedido(Guid.NewGuid(), Status.Pendente) { Id = 1 };
		pedido.AdicionarItem(new Produto("Cabo", 25m, 10) { Id = 2 }, 3);

		var repoMock = new Mock<IPedidoRepository>();
		repoMock.Setup(r => r.GetByIdDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var handler = new GetPedidoQueryHandler(repoMock.Object);
		var result = await handler.Handle(new GetPedidoQuery(1), CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(75m, result.Total);
		Assert.Single(result.ItensPedidos);
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
