using GerenciadorDePedidos.Application.Commands.ConcluirPedido;
using GerenciadorDePedidos.Application.Commands.DevolverPedido;
using GerenciadorDePedidos.Application.Validators;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class ConcluirEDevolverPedidoCommandHandlerTests
{
	private static ICurrentUserService CreateAdmin(Guid adminId)
	{
		var mock = new Mock<ICurrentUserService>();
		mock.Setup(u => u.ClienteId).Returns(adminId);
		mock.Setup(u => u.IsAdmin).Returns(true);
		return mock.Object;
	}

	[Fact]
	public async Task ConcluirHandle_PedidoProcessado_DeveConcluirESalvarInvalidandoCache()
	{
		var clienteId = Guid.NewGuid();
		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 70 };
		pedido.MarcarComoProcessado();

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(70, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var uowMock = new Mock<IUnitOfWork>();
		var cacheMock = new Mock<ICacheService>();
		var realtimeMock = new Mock<IPedidoRealtimeNotifier>();

		var handler = new ConcluirPedidoCommandHandler(pedidoRepoMock.Object, uowMock.Object, cacheMock.Object, realtimeMock.Object);

		var result = await handler.Handle(new ConcluirPedidoCommand(70), CancellationToken.None);

		Assert.Equal(70, result.PedidoId);
		Assert.Equal(Status.Concluido, result.Status);
		Assert.NotNull(pedido.ConcluidoEm);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		cacheMock.Verify(c => c.Remove(CacheKeyHelper.GetPedidoByIdKey(70)), Times.Once);
		cacheMock.Verify(c => c.Remove(CacheKeyHelper.GetClienteByIdKey(clienteId)), Times.Once);
		realtimeMock.Verify(n => n.NotificarStatusAtualizadoAsync(
			It.Is<PedidoStatusAtualizadoDTO>(dto =>
				dto.PedidoId == 70 &&
				dto.ClienteId == clienteId &&
				dto.StatusAnterior == Status.Processado &&
				dto.StatusAtual == Status.Concluido),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task ConcluirHandle_PedidoPendenteOuCancelado_DeveLancarInvalidOperationException()
	{
		var pedidoPendente = new Pedido(Guid.NewGuid(), Status.Pendente) { Id = 71 };
		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(71, It.IsAny<CancellationToken>())).ReturnsAsync(pedidoPendente);

		var uowMock = new Mock<IUnitOfWork>();
		var handler = new ConcluirPedidoCommandHandler(
			pedidoRepoMock.Object,
			uowMock.Object,
			new Mock<ICacheService>().Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			handler.Handle(new ConcluirPedidoCommand(71), CancellationToken.None));
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task DevolverHandle_PedidoConcluidoComRetornoEstoque_DeveDevolverEstornarComMotivoDevolucaoESnapshot()
	{
		var clienteId = Guid.NewGuid();
		var adminId = Guid.NewGuid();
		var produto = new Produto("Monitor", 1200m, 8) { Id = 1 };

		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 80 };
		pedido.AdicionarItem(produto, 2);
		pedido.MarcarComoProcessado();
		pedido.Concluir();
		produto.AlterarPreco(1500m); // Garante uso do snapshot (1200m)

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(80, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		produtoRepoMock
			.Setup(r => r.GetByIdsIncludingInactiveForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([produto]);

		var movRepoMock = new Mock<IMovimentacaoEstoqueRepository>();
		var uowMock = new Mock<IUnitOfWork>();
		var cacheMock = new Mock<ICacheService>();
		var realtimeMock = new Mock<IPedidoRealtimeNotifier>();

		var handler = new DevolverPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			movRepoMock.Object,
			uowMock.Object,
			CreateAdmin(adminId),
			cacheMock.Object,
			realtimeMock.Object);

		var result = await handler.Handle(
			new DevolverPedidoCommand(80, "Cliente desistiu dentro do prazo de 7 dias", RetornarAoEstoque: true),
			CancellationToken.None);

		Assert.Equal(80, result.PedidoId);
		Assert.Equal(Status.Devolvido, result.Status);
		realtimeMock.Verify(n => n.NotificarStatusAtualizadoAsync(
			It.Is<PedidoStatusAtualizadoDTO>(dto =>
				dto.PedidoId == 80 &&
				dto.ClienteId == clienteId &&
				dto.StatusAnterior == Status.Concluido &&
				dto.StatusAtual == Status.Devolvido &&
				dto.MotivoDevolucao == "Cliente desistiu dentro do prazo de 7 dias"),
			It.IsAny<CancellationToken>()), Times.Once);
		Assert.Equal("Cliente desistiu dentro do prazo de 7 dias", result.Motivo);
		Assert.True(result.EstoqueEstornado);
		Assert.Equal(2, result.UnidadesEstornadas);
		Assert.Equal(10, produto.Estoque);

		movRepoMock.Verify(r => r.AddRangeAsync(
			It.Is<IEnumerable<MovimentacaoEstoque>>(lista =>
				lista.Count() == 1 &&
				lista.Any(m =>
					m.ProdutoId == 1 &&
					m.Quantidade == 2 &&
					m.TipoMovimentacao == Tipo.Adicao &&
					m.PrecoUnitario == 1200m &&
					m.Motivo == MotivoMovimentacao.Devolucao &&
					m.ClienteId == adminId &&
					m.PedidoId == 80)),
			It.IsAny<CancellationToken>()), Times.Once);

		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		cacheMock.Verify(c => c.RemoveMany(
			It.Is<IEnumerable<string>>(keys => keys.Contains(CacheKeyHelper.GetProdutoByIdKey(1)))), Times.Once);
		cacheMock.Verify(c => c.RemoveByPrefix(CacheKeyHelper.AllProdutosPrefix), Times.Once);
	}

	[Fact]
	public async Task DevolverHandle_SemRetornoAoEstoque_NaoDeveAlterarEstoqueNemCriarMovimentacao()
	{
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Monitor", 1200m, 8) { Id = 1 };

		var pedido = new Pedido(clienteId, Status.Pendente) { Id = 81 };
		pedido.AdicionarItem(produto, 2);
		pedido.MarcarComoProcessado();
		pedido.Concluir();

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(81, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var produtoRepoMock = new Mock<IProdutoRepository>();
		var movRepoMock = new Mock<IMovimentacaoEstoqueRepository>();
		var uowMock = new Mock<IUnitOfWork>();

		var handler = new DevolverPedidoCommandHandler(
			pedidoRepoMock.Object,
			produtoRepoMock.Object,
			movRepoMock.Object,
			uowMock.Object,
			CreateAdmin(Guid.NewGuid()),
			new Mock<ICacheService>().Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		var result = await handler.Handle(
			new DevolverPedidoCommand(81, "Produto entregue avariado", RetornarAoEstoque: false),
			CancellationToken.None);

		Assert.Equal(Status.Devolvido, result.Status);
		Assert.False(result.EstoqueEstornado);
		Assert.Equal(0, result.UnidadesEstornadas);
		Assert.Equal(8, produto.Estoque);

		produtoRepoMock.Verify(r => r.GetByIdsIncludingInactiveForUpdateAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
		movRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<MovimentacaoEstoque>>(), It.IsAny<CancellationToken>()), Times.Never);
		uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task DevolverHandle_PedidoNaoConcluido_DeveLancarInvalidOperationException()
	{
		var pedido = new Pedido(Guid.NewGuid(), Status.Pendente) { Id = 82 };
		pedido.MarcarComoProcessado();

		var pedidoRepoMock = new Mock<IPedidoRepository>();
		pedidoRepoMock.Setup(r => r.GetByIdWithItensAsync(82, It.IsAny<CancellationToken>())).ReturnsAsync(pedido);

		var handler = new DevolverPedidoCommandHandler(
			pedidoRepoMock.Object,
			new Mock<IProdutoRepository>().Object,
			new Mock<IMovimentacaoEstoqueRepository>().Object,
			new Mock<IUnitOfWork>().Object,
			CreateAdmin(Guid.NewGuid()),
			new Mock<ICacheService>().Object,
			new Mock<IPedidoRealtimeNotifier>().Object);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			handler.Handle(new DevolverPedidoCommand(82, "Motivo válido"), CancellationToken.None));
	}

	[Fact]
	public void Validators_ConcluirEDevolver_DevemValidarCorretamente()
	{
		var concluirValidator = new ConcluirPedidoCommandValidator();
		Assert.True(concluirValidator.Validate(new ConcluirPedidoCommand(1)).IsValid);
		Assert.False(concluirValidator.Validate(new ConcluirPedidoCommand(0)).IsValid);

		var devolverValidator = new DevolverPedidoCommandValidator();
		Assert.True(devolverValidator.Validate(new DevolverPedidoCommand(1, "Produto defeituoso")).IsValid);
		Assert.False(devolverValidator.Validate(new DevolverPedidoCommand(0, "Produto defeituoso")).IsValid);
		Assert.False(devolverValidator.Validate(new DevolverPedidoCommand(1, "")).IsValid);
		Assert.False(devolverValidator.Validate(new DevolverPedidoCommand(1, new string('A', 501))).IsValid);
	}
}

