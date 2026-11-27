using GerenciadorDePedidos.Application.Commands.RemoverProdutoEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class RemoverProdutoEstoqueCommandHandlerTests
{
	[Fact]
	public async Task Handle_CommandValid_RemoveMovimentacaoProdutoEstoqueAndUpdatesProduto()
	{
		const int produtoId = 1;
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto 1", 19.99m, 20) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new RemoverProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new RemoverProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			unitOfWorkMock.Object);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(produto.Id, result);
		Assert.Equal(10, produto.Estoque);
		produtoRepositoryMock.Verify(r => r.Update(produto), Times.Once);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(
			It.Is<MovimentacaoEstoque>(m =>
				m.ProdutoId == produtoId &&
				m.Quantidade == 10 &&
				m.TipoMovimentacao == Tipo.Remocao &&
				m.ClienteId == clienteId &&
				m.PrecoUnitario == 19.99m &&
				m.Motivo == MotivoMovimentacao.AjusteManual &&
				m.PedidoId == null),
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

		var command = new RemoverProdutoEstoqueCommand(produtoId, 10, Guid.NewGuid());
		var handler = new RemoverProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			unitOfWorkMock.Object);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"O ID: {produtoId} do produto nao foi encontrado", exception.Message);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_NegativeQuantidade_ThrowsArgumentException()
	{
		const int produtoId = 1;
		var produto = new Produto("Produto teste", 19.99m, 10) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new RemoverProdutoEstoqueCommand(produtoId, -5, Guid.NewGuid());
		var handler = new RemoverProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			unitOfWorkMock.Object);

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O valor não pode ser menor ou igual a 0", exception.Message);
		produtoRepositoryMock.Verify(r => r.Update(It.IsAny<Produto>()), Times.Never);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_QuantidadeExceedsEstoque_ThrowsArgumentException()
	{
		const int produtoId = 1;
		var produto = new Produto("Produto teste", 19.99m, 5) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var movimentacaoRepositoryMock = new Mock<IRepository<MovimentacaoEstoque, int>>();
		var unitOfWorkMock = new Mock<IUnitOfWork>();

		var command = new RemoverProdutoEstoqueCommand(produtoId, 10, Guid.NewGuid());
		var handler = new RemoverProdutoEstoqueCommandHandler(
			produtoRepositoryMock.Object,
			movimentacaoRepositoryMock.Object,
			unitOfWorkMock.Object);

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("Não é possível remover 10 do estoque, pois excede o disponível: 5", exception.Message);
		produtoRepositoryMock.Verify(r => r.Update(It.IsAny<Produto>()), Times.Never);
		movimentacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}