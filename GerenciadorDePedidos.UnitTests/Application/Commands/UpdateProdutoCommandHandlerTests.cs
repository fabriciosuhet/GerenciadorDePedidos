using GerenciadorDePedidos.Application.Commands.UpdateProduto;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class UpdateProdutoCommandHandlerTests
{
	[Fact]
	public async Task Handle_ProdutoExistente_DeveAtualizarDadosESalvar()
	{
		const int produtoId = 1;
		var produto = new Produto("Antigo", 10m, 5) { Id = produtoId };

		var repositoryMock = new Mock<IRepository<Produto, int>>();
		repositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

		var handler = new UpdateProdutoCommandHandler(repositoryMock.Object);
		var command = new UpdateProdutoCommand(produtoId, "Novo", 25m, 15);

		await handler.Handle(command, CancellationToken.None);

		Assert.Equal("Novo", produto.Nome);
		Assert.Equal(25m, produto.Preco);
		Assert.Equal(15, produto.Estoque);
		repositoryMock.Verify(r => r.Update(produto), Times.Once);
		repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ProdutoNaoEncontrado_DeveLancarKeyNotFoundException()
	{
		const int produtoId = 99;
		var repositoryMock = new Mock<IRepository<Produto, int>>();
		repositoryMock.Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

		var handler = new UpdateProdutoCommandHandler(repositoryMock.Object);
		var command = new UpdateProdutoCommand(produtoId, "Novo", 25m, 15);

		await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		repositoryMock.Verify(r => r.Update(It.IsAny<Produto>()), Times.Never);
	}
}
