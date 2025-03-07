using GerenciadorDePedidos.Application.Commands.DeletePedido;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeletePedidoCommandHandlerTests
{
	[Fact]
	public async Task Handle_PedidoExist_ShouldDeletePedido()
	{
		// Arrange
		var pedidoId = Guid.NewGuid();
		var pedidoMock = new Pedido {Id = pedidoId};
		
		var pedidoRepositoryMock = new Mock<IPedidoRepository>();
		pedidoRepositoryMock.Setup(p => p.GetByIdAsync(pedidoId)).ReturnsAsync(pedidoMock);
		
		var handler = new DeletePedidoCommandHandler(pedidoRepositoryMock.Object);
		var command = new DeletePedidoCommand(pedidoId);
		
		// Act
		await handler.Handle(command, CancellationToken.None);
		
		// Assert
		pedidoRepositoryMock.Verify(p => p.DeleteAsync(pedidoId), Times.Once);
		
	}

	[Fact]
	public async Task Handle_PedidoNotExist_ThrowsShouldException()
	{
		// Arrange
		var pedidoId = Guid.NewGuid();
		var pedidoRepositoryMock = new Mock<IPedidoRepository>();
		
		pedidoRepositoryMock.Setup(p => p.GetByIdAsync(pedidoId)).ReturnsAsync((Pedido)null);
		
		var handler = new DeletePedidoCommandHandler(pedidoRepositoryMock.Object);
		var command = new DeletePedidoCommand(pedidoId);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("Pedido nao encontrado", exception.Message);
		pedidoRepositoryMock.Verify(p => p.DeleteAsync(It.IsAny<Guid>()), Times.Never);
	}
}