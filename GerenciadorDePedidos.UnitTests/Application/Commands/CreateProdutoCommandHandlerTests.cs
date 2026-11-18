using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class CreateProdutoCommandHandlerTests
{
	[Fact]
	public async Task Handle_ValidCommand_ShouldCreateProduto()
	{
		var command = new CreateProdutoCommand("Produto Teste", 19.99m, 10);

		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		var handler = new CreateProdutoCommandHandler(produtoRepositoryMock.Object);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(0, result);
		produtoRepositoryMock.Verify(r => r.AddAsync(
			It.Is<Produto>(p => p.Nome == "Produto Teste" && p.Preco == 19.99m && p.Estoque == 10),
			It.IsAny<CancellationToken>()), Times.Once);
		produtoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}