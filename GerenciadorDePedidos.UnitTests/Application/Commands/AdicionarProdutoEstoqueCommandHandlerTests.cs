using GerenciadorDePedidos.Application.Commands.AdicionarPedidoEstoque;
using GerenciadorDePedidos.Application.Commands.AdicionarProdutoEstoque;
using GerenciadorDePedidos.Core.Entities;
using GerenciadorDePedidos.Core.Enums;
using GerenciadorDePedidos.Core.Repositories;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace GerenciadorDePedidos.UnitTests.Application.Commands;

public class AdicionarProdutoEstoqueCommandHandlerTests
{
	[Fact]
	public async Task Handle_CommandValid_AddsMovimentacaoEstoqueAndUpdatesProduto()
	{
		// Arrange
		var produtoId = Guid.NewGuid();
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto Teste", 11.99m, 10)
		{
			Id = produtoId,
		};
		
		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync(produto);
		produtoRepositoryMock.Setup(r => r.UpdateAsync(produtoId, It.IsAny<Produto>())).Returns(Task.CompletedTask);
		
		// Mock de IMovimentacaoEstoqueRepository
		var movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>();
		movimentacaoEstoqueRepositoryMock.Setup(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>()))
			.Returns(Task.CompletedTask);

		var command = new AdicionarProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new AdicionarProdutoEstoqueCommandHandler(produtoRepositoryMock.Object, movimentacaoEstoqueRepositoryMock.Object);
		
		// Act
		var result = await handler.Handle(command, CancellationToken.None);
		
		// Assert
		Assert.Equal(produtoId, result);
		produtoRepositoryMock.Verify(r => r.GetByIdAsync(produtoId), Times.Once());
		produtoRepositoryMock.Verify(r => r.UpdateAsync(produtoId, It.Is<Produto>(p => p == produto)), Times.Once());
		movimentacaoEstoqueRepositoryMock.Verify(r => r.AddAsync(It.Is<MovimentacaoEstoque>(
			m => m.Quantidade.Equals(10) && m.TipoMovimentacao.Equals(Tipo.Adicao) && m.ProdutoId.Equals(produtoId) &&
			     m.ClienteId.Equals(clienteId))));
	}

	[Fact]
	public async Task Handle_InvalidProdutoId_ThrowsKeyNotFoundException()
	{
		// Arrange 
		var produtoId = Guid.NewGuid();
		var clienteId = Guid.NewGuid();
		
		// Mock do IProdutoRepository
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync((Produto)null);
		
		// Mock de IMovimentacaoEstoqueRepository 
		var movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>();
		var command = new AdicionarProdutoEstoqueCommand(produtoId, 10, clienteId);
		var handler = new AdicionarProdutoEstoqueCommandHandler(produtoRepositoryMock.Object, movimentacaoEstoqueRepositoryMock.Object);
		
		// Act e Assert
		var excpetion = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal($"O ID: {produtoId} de produto nao foi encontrado", excpetion.Message);
		movimentacaoEstoqueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>()), Times.Never());
	}

	[Fact]
	public async Task Handle_NegativeQuantidade_ThrowsKeyArgumentException()
	{
		// Arrange
		var produtoId = Guid.NewGuid();
		var clienteId = Guid.NewGuid();
		var produto = new Produto("Produto Teste", 10.99m, 10)
		{
			Id = produtoId,
		};
		
		// Mock de IProdutoRepository
		var produtoRepositoryMock = new Mock<IProdutoRepository>();
		produtoRepositoryMock.Setup(r => r.GetByIdAsync(produtoId)).ReturnsAsync(produto);
		
		// Mock de IMovimentacaoEstoqueRepository
		var movimentacaoEstoqueRepositoryMock = new Mock<IMovimentacaoEstoqueRepository>();
		var command = new AdicionarProdutoEstoqueCommand(produtoId, -5, clienteId);
		var handler = new AdicionarProdutoEstoqueCommandHandler(produtoRepositoryMock.Object, movimentacaoEstoqueRepositoryMock.Object);
		
		// Act e Assert
		var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
		Assert.Equal("O valor não pode ser menor ou igual a 0", exception.Message);
		movimentacaoEstoqueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MovimentacaoEstoque>()), Times.Never());
		produtoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Produto>()), Times.Never());
	}
	
}