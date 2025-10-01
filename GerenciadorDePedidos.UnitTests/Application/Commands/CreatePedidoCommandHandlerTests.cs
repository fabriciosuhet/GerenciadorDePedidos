using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class CreatePedidoCommandHandlerTests
{
	[Fact]
	public async Task Handle_ValidCommand_ShouldCreatePedido()
	{
		// Arrange
		var produtoId = 1;
		var clienteId = Guid.NewGuid();
		var command = new CreatePedidoCommand
		{
			ItensPedidos = new List<ItemPedidoDTO>
			{
				new ItemPedidoDTO { produtoId = produtoId, Quantidade = 2 }
			},
			ClienteId = clienteId,
			Status = Status.Pendente
		};

		var produto = new Produto("Produto Teste", 10.00m, 5) { Id = produtoId };

		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync(produto);
		
		// Mock de IPedidoRepository
		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
        pedidoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Pedido>())).Returns(Task.CompletedTask);
		
		var handler = new CreatePedidoCommandHandler(pedidoRepositoryMock.Object, produtoRepositoryMock.Object);
		
		
		// Act
		var result = await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.NotEqual(1, result);
		produtoRepositoryMock.Verify(r => r.GetByIdAsync(produtoId), Times.Once());
		pedidoRepositoryMock.Verify(r => r.AddAsync(It.Is<Pedido>(p =>
			p.ClienteId.Equals(clienteId) &&
			p.Status.Equals(Status.Pendente) &&
			p.ItensPedidos.Count.Equals(1) &&
			p.ItensPedidos.Any(i =>
				i.ProdutoId.Equals(produtoId) && i.Quantidade.Equals(2) && i.PrecoUnitario.Equals(10.00m)) &&
			p.Total.Equals(20m)
		)), Times.Once());
	}

	[Fact]
	public async Task Handle_EmptyItensPedidos_ThrowsArgumentException()
	{
		// Arrange
		var clienteId = Guid.NewGuid();
		var command = new CreatePedidoCommand
		{
			ItensPedidos = new List<ItemPedidoDTO>(),
			ClienteId = clienteId,
			Status = Status.Pendente
		};

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
        var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
        var handler = new CreatePedidoCommandHandler(pedidoRepositoryMock.Object, produtoRepositoryMock.Object);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O pedido deve conter pelo menos um item.", exception.Message);
		produtoRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never());
		pedidoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pedido>()), Times.Never());
	}

	[Fact]
	public async Task Handle_ProdutoNotFound_ThrowsArgumentException()
	{
		var produtoId = 1;
		var clienteId = Guid.NewGuid();
		var command = new CreatePedidoCommand
		{
			ItensPedidos = new List<ItemPedidoDTO>
			{
				new ItemPedidoDTO { produtoId = produtoId, Quantidade = 2 }
			},
			ClienteId = clienteId,
			Status = Status.Pendente
		};
		
		// Mock de iProdutoRepository retornando null (produto nao encontrado)
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync((Produto)null);
		
		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		var handler = new CreatePedidoCommandHandler(pedidoRepositoryMock.Object, produtoRepositoryMock.Object);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"O produto com o ID {produtoId} nao foi encontrado.", exception.Message);
		produtoRepositoryMock.Verify(r => r.GetByIdAsync(produtoId), Times.Once());
		pedidoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pedido>()), Times.Never());
	}
}