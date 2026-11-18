using GerenciadorDePedidos.Application.Commands.CreatePedido;
using GerenciadorDePedidos.Core.DTOs;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class CreatePedidoCommandHandlerTests
{
	[Fact]
	public async Task Handle_ValidCommand_ShouldCreatePedidoWithSingleBatchQuery()
	{
		const int produtoId1 = 1;
		const int produtoId2 = 2;
		var clienteId = Guid.NewGuid();
		var command = new CreatePedidoCommand(
			[
				new ItemPedidoDTO { ProdutoId = produtoId1, Quantidade = 2 },
				new ItemPedidoDTO { ProdutoId = produtoId2, Quantidade = 3 }
			],
			clienteId,
			Status.Pendente);

		var produto1 = new Produto("Produto 1", 10.00m, 5) { Id = produtoId1 };
		var produto2 = new Produto("Produto 2", 20.00m, 5) { Id = produtoId2 };

		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock
			.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([produto1, produto2]);

		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		var handler = new CreatePedidoCommandHandler(pedidoRepositoryMock.Object, produtoRepositoryMock.Object);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(0, result);
		produtoRepositoryMock.Verify(
			r => r.GetByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == 2), It.IsAny<CancellationToken>()),
			Times.Once);
		pedidoRepositoryMock.Verify(r => r.AddAsync(It.Is<Pedido>(p =>
			p.ClienteId == clienteId &&
			p.Status == Status.Pendente &&
			p.ItensPedidos.Count == 2 &&
			p.Total == 80m), It.IsAny<CancellationToken>()), Times.Once);
		pedidoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_EmptyItensPedidos_ThrowsArgumentException()
	{
		var command = new CreatePedidoCommand([], Guid.NewGuid(), Status.Pendente);

		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		var handler = new CreatePedidoCommandHandler(pedidoRepositoryMock.Object, produtoRepositoryMock.Object);

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O pedido deve conter pelo menos um item.", exception.Message);
		produtoRepositoryMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
		pedidoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_ProdutoNotFound_ThrowsArgumentException()
	{
		const int produtoId = 1;
		var command = new CreatePedidoCommand(
			[new ItemPedidoDTO { ProdutoId = produtoId, Quantidade = 2 }],
			Guid.NewGuid(),
			Status.Pendente);

		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock
			.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		var pedidoRepositoryMock = new Mock<IRepository<Pedido, int>>();
		var handler = new CreatePedidoCommandHandler(pedidoRepositoryMock.Object, produtoRepositoryMock.Object);

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"O produto com o ID {produtoId} nao foi encontrado.", exception.Message);
		produtoRepositoryMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Once);
		pedidoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}