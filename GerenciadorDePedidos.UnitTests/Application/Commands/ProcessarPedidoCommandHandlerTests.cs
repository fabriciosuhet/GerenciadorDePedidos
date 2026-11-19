using GerenciadorDePedidos.Application.Commands.ProcessarPedido;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class ProcessarPedidoCommandHandlerTests
{
	[Fact]
	public async Task Handle_PedidoPendenteComEstoqueSuficiente_DeveConcluirEBaixarEstoque()
	{
		var clienteId = Guid.NewGuid();
		var produto1 = new Produto("Monitor", 1000m, 10) { Id = 1 };
		var produto2 = new Produto("Mouse", 100m, 5) { Id = 2 };

		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 42 };
		pedido.AdicionarItem(produto1, 2);
		pedido.AdicionarItem(produto2, 3);

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		produtoRepoMock
			.Setup(r => r.GetByIdsForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([produto1, produto2]);

		var movimentacaoRepoMock = new Mock<IMovimentacaoEstoqueRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var cacheMock = new Mock<ICacheService>();

		var handler = new ProcessarPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			movimentacaoRepoMock.Object,
			unitOfWorkMock.Object,
			cacheMock.Object);

		var result = await handler.Handle(new ProcessarPedidoCommand(42), CancellationToken.None);

		Assert.True(result);
		Assert.Equal(Status.Concluido, pedido.Status);
		Assert.Equal(8, produto1.Estoque);
		Assert.Equal(2, produto2.Estoque);
		movimentacaoRepoMock.Verify(r => r.AddAsync(
			It.Is<MovimentacaoEstoque>(m => m.ProdutoId == 1 && m.Quantidade == 2 && m.TipoMovimentacao == Tipo.Remocao && m.ClienteId == clienteId),
			It.IsAny<CancellationToken>()), Times.Once);
		movimentacaoRepoMock.Verify(r => r.AddAsync(
			It.Is<MovimentacaoEstoque>(m => m.ProdutoId == 2 && m.Quantidade == 3 && m.TipoMovimentacao == Tipo.Remocao && m.ClienteId == clienteId),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		cacheMock.Verify(c => c.Remove(CacheKeyHelper.GetClienteByIdKey(clienteId)), Times.Once);
	}

	[Fact]
	public async Task Handle_EstoqueInsuficiente_DeveCancelarPedidoSemAlterarEstoque()
	{
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Monitor", 1000m, 3) { Id = 1 };

		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 43 };
		pedido.AdicionarItem(produto, 2);
		pedido.AdicionarItem(produto, 2); // Total pedido = 4 > estoque 3

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(43, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		produtoRepoMock
			.Setup(r => r.GetByIdsForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([produto]);

		var movimentacaoRepoMock = new Mock<IMovimentacaoEstoqueRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var cacheMock = new Mock<ICacheService>();

		var handler = new ProcessarPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			movimentacaoRepoMock.Object,
			unitOfWorkMock.Object,
			cacheMock.Object);

		var result = await handler.Handle(new ProcessarPedidoCommand(43), CancellationToken.None);

		Assert.False(result);
		Assert.Equal(Status.Cancelado, pedido.Status);
		Assert.Equal(3, produto.Estoque);
		movimentacaoRepoMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		cacheMock.Verify(c => c.Remove(CacheKeyHelper.GetClienteByIdKey(clienteId)), Times.Once);
	}

	[Fact]
	public async Task Handle_PedidoJaConcluido_DeveSerIdempotente()
	{
		var pedido = new Pedido(Guid.NewGuid(), Status.Concluido) { Id = 44 };

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(44, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		var movimentacaoRepoMock = new Mock<IMovimentacaoEstoqueRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var cacheMock = new Mock<ICacheService>();

		var handler = new ProcessarPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			movimentacaoRepoMock.Object,
			unitOfWorkMock.Object,
			cacheMock.Object);

		var result = await handler.Handle(new ProcessarPedidoCommand(44), CancellationToken.None);

		Assert.False(result);
		produtoRepoMock.Verify(r => r.GetByIdsForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}
