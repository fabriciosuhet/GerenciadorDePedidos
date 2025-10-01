using GerenciadorDePedidos.Application.Commands.DeleteProduto;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class DeleteProdutoCommandHandlerTests
{
	[Fact]
	public async Task Handler_ProdutoExist_ShouldDeleteProduto()
	{
		// Arrange
		var produtoId = 1;
		var produtoMock = new Produto {Id = produtoId};
		
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync(produtoMock);
		
		var handler = new DeleteProdutoCommandHandler(produtoRepositoryMock.Object);
		var command = new DeleteProdutoCommand(produtoId);
		
		// Act
		await handler.Handle(command, CancellationToken.None);
		
		// Assert
		produtoRepositoryMock.Verify(r => r.DeleteAsync(produtoId), Times.Once);
	}

	[Fact]
	public async Task Handler_ProdutoNotExist_ThrowsShouldException()
	{
		// Arrange
		var produtoId = 1;
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync((Produto) null);
		
		var handler = new DeleteProdutoCommandHandler(produtoRepositoryMock.Object);
		var command = new DeleteProdutoCommand(produtoId);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("Produto nao encontrado", exception.Message);
		produtoRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
	}
}