using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Helpers;
using GerenciadorDePedidos.Core.Repositories;
using GerenciadorDePedidos.Core.Services;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeletePedidoCommandHandlerTests
{
	[Fact]
	public async Task Handle_PedidoExist_ShouldDeletePedido()
	{
		const int pedidoId = 1;
		var clienteId = Guid.NewGuid();
		var pedidoMock = new Pedido(clienteId, Status.Pendente) { Id = pedidoId };

		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		pedidoRepositoryMock.Setup(p => p.GetByIdAsync(pedidoId, It.IsAny<CancellationToken>())).ReturnsAsync(pedidoMock);
		var cacheMock = new Mock<ICacheService>();

		var handler = new DeletePedidoCommandHandler(pedidoRepositoryMock.Object, cacheMock.Object);
		var command = new DeletePedidoCommand(pedidoId);

		await handler.Handle(command, CancellationToken.None);

		pedidoRepositoryMock.Verify(p => p.DeleteAsync(pedidoId, It.IsAny<CancellationToken>()), Times.Once);
		pedidoRepositoryMock.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		cacheMock.Verify(c => c.Remove(CacheKeyHelper.GetClienteByIdKey(clienteId)), Times.Once);
		cacheMock.Verify(c => c.RemoveByPrefix(CacheKeyHelper.GetAllPedidosClientePrefix(clienteId)), Times.Once);
	}

	[Fact]
	public async Task Handle_PedidoNotExist_ThrowsKeyNotFoundException()
	{
		const int pedidoId = 1;
		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		pedidoRepositoryMock.Setup(p => p.GetByIdAsync(pedidoId, It.IsAny<CancellationToken>())).ReturnsAsync((Pedido?)null);
		var cacheMock = new Mock<ICacheService>();

		var handler = new DeletePedidoCommandHandler(pedidoRepositoryMock.Object, cacheMock.Object);
		var command = new DeletePedidoCommand(pedidoId);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("Pedido nao encontrado", exception.Message);
		pedidoRepositoryMock.Verify(p => p.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}