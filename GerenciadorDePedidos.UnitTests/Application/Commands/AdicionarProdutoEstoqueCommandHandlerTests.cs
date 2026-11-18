using GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class AdicionarProdutoEstoqueCommandHandlerTests
{
	[Fact]
	public async Task Handle_CommandValid_AddsMovimentacaoEstoqueAndUpdatesProduto()
	{
		const int produtoId = 1;
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto Teste", 11.99m, 10) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new AdicionarProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new AdicionarProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			unitOfWorkMock.Object);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(produtoId, result);
		Assert.Equal(20, produto.Estoque);
		produtoRepositoryMock.Verify(r => r.Update(produto), Times.Once);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(
			It.Is<MovimentacaoEstoque>(m =>
				m.Quantidade == 10 &&
				m.TipoMovimentacao == Tipo.Adicao &&
				m.ProdutoId == produtoId &&
				m.ClienteId == clienteId),
			It.IsAny<CancellationToken>()), Times.Once);
		unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_InvalidProdutoId_ThrowsKeyNotFoundException()
	{
		const int produtoId = 1;
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new AdicionarProdutoEstoqueCommand(produtoId, 10, Guid.NewGuid());
		var handler = new AdicionarProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			unitOfWorkMock.Object);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"O ID: {produtoId} de produto nao foi encontrado", exception.Message);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_NegativeQuantidade_ThrowsArgumentException()
	{
		const int produtoId = 1;
		var produto = new Produto("Produto Teste", 10.99m, 10) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new AdicionarProdutoEstoqueCommand(produtoId, -5, Guid.NewGuid());
		var handler = new AdicionarProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			unitOfWorkMock.Object);

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O valor não pode ser menor ou igual a 0", exception.Message);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
		produtoRepositoryMock.Verify(r => r.Update(It.IsAny<Produto>()), Times.Never);
	}
}