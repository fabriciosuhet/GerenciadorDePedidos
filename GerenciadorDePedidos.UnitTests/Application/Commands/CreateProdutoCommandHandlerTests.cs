using GerenciadorDePedidos.Application.Commands.CreateProduto;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class CreateProdutoCommandHandlerTests
{
	[Fact]
	public async Task Handler_ValidCommand_ShouldCreateProduto()
	{
		// Arrange
		var command = new CreateProdutoCommand
		{
			Estoque = 10,
			Nome = "Produto Teste",
			Preco = 19.99m
		};
		
		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		produtoRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Produto>())).Returns(Task.CompletedTask);
		
		var handler = new CreateProdutoCommandHandler(produtoRepositoryMock.Object);
		
		// Act
		var result = await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.NotEqual(1, result);
		produtoRepositoryMock.Verify(r => r.AddAsync(It.Is<Produto>(p =>
			p.Nome.Equals("Produto Teste") &&
			p.Preco.Equals(19.99m) &&
			p.Estoque.Equals(10) &&
			p.Id.Equals(result))), Times.Once());
	}

	[Fact]
	public async Task Handler_NomeVazio_ShouldThrowArgumentException()
	{
		// Arrange
		var command = new CreateProdutoCommand
		{
			Estoque = 10,
			Nome = "",
			Preco = 19.99m
		};
		
		var produtoRepositoryMock = new Mock<IRepository<Produto, int>>();
		var handler = new CreateProdutoCommandHandler(produtoRepositoryMock.Object);
		
		// Act e Assert
		await handler.Handle(command, CancellationToken.None);
		produtoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Produto>()), Times.Once());
	}
	
}