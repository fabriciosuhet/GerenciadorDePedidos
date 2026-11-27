using GerenciadorDePedidos.Application.Commands.DeleteProduto;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeleteProdutoCommandHandlerTests
{
	[Fact]
	public async Task Handle_ProdutoExist_ShouldSoftDeleteProduto()
	{
		const int produtoId = 1;
		var produto = new Produto("Produto", 10m, 5) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var handler = new DeleteProdutoCommandHandler(produtoRepositoryMock.Object);
		var command = new DeleteProdutoCommand(produtoId);

		await handler.Handle(command, CancellationToken.None);

		Assert.False(produto.Ativo);
		Assert.NotNull(produto.DesativadoEm);
		Assert.NotNull(produto.AtualizadoEm);
		produtoRepositoryMock.Verify(r => r.Update(produto), Times.Once);
		produtoRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
		produtoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ProdutoNotExist_ThrowsKeyNotFoundException()
	{
		const int produtoId = 1;
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

		var handler = new DeleteProdutoCommandHandler(produtoRepositoryMock.Object);
		var command = new DeleteProdutoCommand(produtoId);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("Produto nao encontrado", exception.Message);
		produtoRepositoryMock.Verify(r => r.Update(It.IsAny<Produto>()), Times.Never);
		produtoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}