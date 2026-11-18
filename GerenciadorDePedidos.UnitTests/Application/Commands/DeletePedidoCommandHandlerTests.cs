using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeletePedidoCommandHandlerTests
{
	[Fact]
	public async Task Handle_PedidoExist_ShouldDeletePedido()
	{
		const int pedidoId = 1;
		var pedidoMock = new Pedido(Guid.NewGuid(), Status.Pendente) { Id = pedidoId };

		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		pedidoRepositoryMock.Setup(p => p.GetByIdAsync(pedidoId, It.IsAny<CancellationToken>())).ReturnsAsync(pedidoMock);

		var handler = new DeletePedidoCommandHandler(pedidoRepositoryMock.Object);
		var command = new DeletePedidoCommand(pedidoId);

		await handler.Handle(command, CancellationToken.None);

		pedidoRepositoryMock.Verify(p => p.DeleteAsync(pedidoId, It.IsAny<CancellationToken>()), Times.Once);
		pedidoRepositoryMock.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_PedidoNotExist_ThrowsKeyNotFoundException()
	{
		const int pedidoId = 1;
		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		pedidoRepositoryMock.Setup(p => p.GetByIdAsync(pedidoId, It.IsAny<CancellationToken>())).ReturnsAsync((Pedido?)null);

		var handler = new DeletePedidoCommandHandler(pedidoRepositoryMock.Object);
		var command = new DeletePedidoCommand(pedidoId);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("Pedido nao encontrado", exception.Message);
		pedidoRepositoryMock.Verify(p => p.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}