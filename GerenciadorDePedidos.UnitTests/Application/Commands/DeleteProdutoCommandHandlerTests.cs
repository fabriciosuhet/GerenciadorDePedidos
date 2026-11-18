using GerenciadorDePedidos.Application.Commands.DeleteProduto;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeleteProdutoCommandHandlerTests
{
	[Fact]
	public async Task Handle_ProdutoExist_ShouldDeleteProduto()
	{
		const int produtoId = 1;
		var produtoMock = new Produto("Produto", 10m, 5) { Id = produtoId };

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produtoMock);

		var handler = new DeleteProdutoCommandHandler(produtoRepositoryMock.Object);
		var command = new DeleteProdutoCommand(produtoId);

		await handler.Handle(command, CancellationToken.None);

		produtoRepositoryMock.Verify(r => r.DeleteAsync(produtoId, It.IsAny<CancellationToken>()), Times.Once);
		produtoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ProdutoNotExist_ThrowsKeyNotFoundException()
	{
		const int produtoId = 1;
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

		var handler = new DeleteProdutoCommandHandler(produtoRepositoryMock.Object);
		var command = new DeleteProdutoCommand(produtoId);

		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("Produto nao encontrado", exception.Message);
		produtoRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}