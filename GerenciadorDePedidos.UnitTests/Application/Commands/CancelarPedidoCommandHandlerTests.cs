using GerenciadorDePedidos.Application.Commands.CancelarPedido;
using GerenciadorDePedidos.Application.Validators;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class CancelarPedidoCommandHandlerTests
{
	private static ICurrentUserService CreateUser(Guid clienteId, bool isAdmin = false)
	{
		var mock = new Mock<ICurrentUserService>();
		mock.Setup(u => u.ClienteId).Returns(clienteId);
		mock.Setup(u => u.IsAdmin).Returns(isAdmin);
		return mock.Object;
	}

	[Fact]
	public async Task Handle_PedidoProcessado_DeveCancelarEstornarEstoqueERegistrarMovimentacaoComSnapshot()
	{
		var clienteId = Guid.NewGuid();
		var produto1 = new Produto("Monitor", 1000m, 8) { Id = 1 };
		var produto2 = new Produto("Mouse", 150m, 5) { Id = 2 };
		produto2.Desativar(); // Mesmo produto desativado deve receber estorno

		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 50 };
		pedido.AdicionarItem(produto1, 2);
		pedido.AdicionarItem(produto2, 3);
		pedido.MarcarComoProcessado();

		// Altera o preço do produto depois da venda para validar que o estorno usa o snapshot do item
		produto1.AlterarPreco(1300m);

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(50, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		produtoRepoMock
			.Setup(r => r.GetByIdsIncludingInactiveForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([produto1, produto2]);

		var movimentacaoRepoMock = new Mock<IMovimentacaoEstoqueRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var cacheMock = new Mock<ICacheService>();
		var realtimeMock = new Mock<IPedidoRealtimeNotifier>();

		var handler = new CancelarPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			movimentacaoRepoMock.Object,
			unitOfWorkMock.Object,
			CreateUser(clienteId),
			cacheMock.Object,
			realtimeMock.Object);

		var result = await handler.Handle(new CancelarPedidoCommand(50), CancellationToken.None);

		Assert.Equal(50, result.PedidoId);
		Assert.Equal(Status.Cancelado, result.Status);
		realtimeMock.Verify(n => n.NotificarStatusAtualizadoAsync(
			It.Is<PedidoStatusAtualizadoDTO>(dto =>
				dto.PedidoId == 50 &&
				dto.ClienteId == clienteId &&
				dto.StatusAnterior == Status.Processado &&
				dto.StatusAtual == Status.Cancelado),
			It.IsAny<CancellationToken>()), Times.Once);
		Assert.True(result.EstoqueEstornado);
		Assert.Equal(5, result.UnidadesEstornadas);
		Assert.Equal(10, produto1.Estoque);
		Assert.Equal(8, produto2.Estoque);

		movimentacaoRepoMock.Verify(r => r.AddRangeAsync(
			It.Is<IEnumerable<MovimentacaoEstoque>>(lista =>
				lista.Count() == 2 &&
				lista.Any(m =>
					m.ProdutoId == 1 &&
					m.Quantidade == 2 &&
					m.TipoMovimentacao == Tipo.Adicao &&
					m.PrecoUnitario == 1000m &&
					m.Motivo == MotivoMovimentacao.EstornoCancelamento &&
					m.PedidoId == 50) &&
				lista.Any(m =>
					m.ProdutoId == 2 &&
					m.Quantidade == 3 &&
					m.TipoMovimentacao == Tipo.Adicao &&
					m.PrecoUnitario == 150m &&
					m.Motivo == MotivoMovimentacao.EstornoCancelamento &&
					m.PedidoId == 50)),
			It.IsAny<CancellationToken>()), Times.Once);

		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		cacheMock.Verify(c => c.Remove(CacheKeyHelper.GetPedidoByIdKey(50)), Times.Once);
		cacheMock.Verify(c => c.RemoveMany(
			It.Is<IEnumerable<string>>(keys =>
				keys.Contains(CacheKeyHelper.GetProdutoByIdKey(1)) &&
				keys.Contains(CacheKeyHelper.GetProdutoByIdKey(2)))), Times.Once);
		cacheMock.Verify(c => c.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix), Times.Once);
	}

	[Fact]
	public async Task Handle_PedidoPendente_DeveCancelarSemEstornarEstoque()
	{
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Teclado", 200m, 10) { Id = 1 };
		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 51 };
		pedido.AdicionarItem(produto, 2);

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(51, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		var movimentacaoRepoMock = new Mock<IMovimentacaoEstoqueRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();
		var cacheMock = new Mock<ICacheService>();

		var handler = new CancelarPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			movimentacaoRepoMock.Object,
			unitOfWorkMock.Object,
			CreateUser(clienteId),
			cacheMock.Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		var result = await handler.Handle(new CancelarPedidoCommand(51), CancellationToken.None);

		Assert.Equal(Status.Cancelado, result.Status);
		Assert.False(result.EstoqueEstornado);
		Assert.Equal(0, result.UnidadesEstornadas);
		Assert.Equal(10, produto.Estoque);
		produtoRepoMock.Verify(r => r.GetByIdsIncludingInactiveForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
		movimentacaoRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<MovimentacaoEstoque>>(), It.IsAny<CancellationToken>()), Times.Never);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_PedidoJaCancelado_DeveLancarInvalidOperationException()
	{
		var clienteId = Guid.NewGuid();
		var pedido = new Pedido(clienteId, Status.Cancelado) { Id = 52 };

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(52, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var handler = new CancelarPedidoCommandHandler(
			pedidoRepoMock.Object,
			new Mock<IProdutoRepository>().Object,
			new Mock<IMovimentacaoEstoqueRepository>().Object,
			new Mock<IUnitOfWork>().Object,
			CreateUser(clienteId),
			new Mock<ICacheService>().Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			handler.Handle(new CancelarPedidoCommand(52), CancellationToken.None));
	}

	[Fact]
	public async Task Handle_PedidoConcluidoOuDevolvido_DeveLancarInvalidOperationExceptionSemEstornarEstoque()
	{
		var clienteId = Guid.NewGuid();
		var pedidoConcluido = new Pedido(clienteId, Status.Pendente) { Id = 55 };
		pedidoConcluido.MarcarComoProcessado();
		pedidoConcluido.Concluir();

		var pedidoDevolvido = new Pedido(clienteId, Status.Pendente) { Id = 56 };
		pedidoDevolvido.MarcarComoProcessado();
		pedidoDevolvido.Concluir();
		pedidoDevolvido.Devolver("Item com defeito");

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(55, It.IsAny<CancellationToken>())).ReturnsAsync(pedidoConcluido);
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(56, It.IsAny<CancellationToken>())).ReturnsAsync(pedidoDevolvido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var handler = new CancelarPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			new Mock<IMovimentacaoEstoqueRepository>().Object,
			unitOfWorkMock.Object,
			CreateUser(clienteId, isAdmin: true),
			new Mock<ICacheService>().Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			handler.Handle(new CancelarPedidoCommand(55), CancellationToken.None));
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			handler.Handle(new CancelarPedidoCommand(56), CancellationToken.None));

		produtoRepoMock.Verify(r => r.GetByIdsIncludingInactiveForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_UsuarioComumCancelandoPedidoDeOutro_DeveLancarUnauthorizedAccessException()
	{
		var pedido = new Pedido(Guid.NewGuid(), Status.Pendente) { Id = 53 };

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(53, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var handler = new CancelarPedidoCommandHandler(
			pedidoRepoMock.Object,
			new Mock<IProdutoRepository>().Object,
			new Mock<IMovimentacaoEstoqueRepository>().Object,
			new Mock<IUnitOfWork>().Object,
			CreateUser(Guid.NewGuid(), isAdmin: false),
			new Mock<ICacheService>().Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
			handler.Handle(new CancelarPedidoCommand(53), CancellationToken.None));
	}

	[Fact]
	public async Task Handle_PedidoNaoEncontrado_DeveLancarKeyNotFoundException()
	{
		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Pedido?)null);

		var handler = new CancelarPedidoCommandHandler(
			pedidoRepoMock.Object,
			new Mock<IProdutoRepository>().Object,
			new Mock<IMovimentacaoEstoqueRepository>().Object,
			new Mock<IUnitOfWork>().Object,
			CreateUser(Guid.NewGuid(), isAdmin: true),
			new Mock<ICacheService>().Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		await Assert.ThrowsAsync<KeyNotFoundException>(() =>
			handler.Handle(new CancelarPedidoCommand(999), CancellationToken.None));
	}

	[Fact]
	public void Validator_ComPedidoIdInvalido_DeveFalhar()
	{
		var validator = new CancelarPedidoCommandValidator();
		Assert.False(validator.Validate(new CancelarPedidoCommand(0)).IsValid);
		Assert.True(validator.Validate(new CancelarPedidoCommand(1)).IsValid);
	}
}

